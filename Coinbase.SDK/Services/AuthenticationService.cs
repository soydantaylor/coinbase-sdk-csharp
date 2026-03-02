using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Coinbase.SDK.Exceptions;
using Coinbase.SDK.Interfaces;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;

namespace Coinbase.SDK.Services;

/// <summary>
/// Handles JWT token generation and validation for API authentication.
/// Supports both legacy ES256 (PEM EC keys) and new EdDSA (Ed25519 raw keys).
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IConfigurationManager _configurationManager;
    private readonly JwtSecurityTokenHandler _tokenHandler;
    private readonly Dictionary<string, (string token, DateTime expiration)> _cachedTokens;
    private readonly object _tokenLock = new();

    // Key material — one of these will be populated based on key format
    private ECDsa? _ecdsaKey;
    private Ed25519PrivateKeyParameters? _ed25519Key;
    private bool _useEdDsa;
    private bool _keyParsed;

    /// <summary>
    /// Initializes a new instance of the AuthenticationService class
    /// </summary>
    /// <param name="configurationManager">The configuration manager</param>
    public AuthenticationService(IConfigurationManager configurationManager)
    {
        _configurationManager = configurationManager ?? throw new ArgumentNullException(nameof(configurationManager));
        _tokenHandler = new JwtSecurityTokenHandler();
        _cachedTokens = new Dictionary<string, (string token, DateTime expiration)>();
    }

    /// <inheritdoc />
    public Task<string> GenerateTokenAsync(string httpMethod, string endpoint, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(httpMethod))
            throw new ArgumentException("HTTP method cannot be null or empty", nameof(httpMethod));

        if (string.IsNullOrWhiteSpace(endpoint))
            throw new ArgumentException("Endpoint cannot be null or empty", nameof(endpoint));

        lock (_tokenLock)
        {
            var cacheKey = $"{httpMethod} {endpoint}";

            var bufferSeconds = Math.Min(30, _configurationManager.TokenExpiration.TotalSeconds / 4);
            if (_cachedTokens.TryGetValue(cacheKey, out var cachedEntry) &&
                DateTime.UtcNow < cachedEntry.expiration.AddSeconds(-bufferSeconds))
            {
                return Task.FromResult(cachedEntry.token);
            }

            try
            {
                EnsureKeyParsed();

                var now = DateTime.UtcNow;
                var expiration = now.Add(_configurationManager.TokenExpiration);

                var baseUrlWithoutProtocol = _configurationManager.BaseUrl;
                if (baseUrlWithoutProtocol.StartsWith("https://"))
                    baseUrlWithoutProtocol = baseUrlWithoutProtocol.Substring("https://".Length);
                else if (baseUrlWithoutProtocol.StartsWith("http://"))
                    baseUrlWithoutProtocol = baseUrlWithoutProtocol.Substring("http://".Length);

                var uri = $"{httpMethod} {baseUrlWithoutProtocol}{endpoint}";

                string tokenString = _useEdDsa
                    ? GenerateEdDsaToken(now, expiration, uri)
                    : GenerateEs256Token(now, expiration, uri);

                _cachedTokens[cacheKey] = (tokenString, expiration);
                return Task.FromResult(tokenString);
            }
            catch (Exception ex) when (!(ex is CoinbaseAuthenticationException))
            {
                throw new CoinbaseAuthenticationException($"Failed to generate JWT token: {ex.Message}");
            }
        }
    }

    /// <inheritdoc />
    public Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Task.FromResult(false);

        try
        {
            var jwtToken = _tokenHandler.ReadJwtToken(token);

            if (jwtToken.ValidTo <= DateTime.UtcNow)
                return Task.FromResult(false);

            var requiredClaims = new[] { "sub", "iss", "nbf", "exp", "uri" };
            foreach (var claimType in requiredClaims)
            {
                if (!jwtToken.Claims.Any(c => c.Type == claimType))
                    return Task.FromResult(false);
            }

            var requiredHeaders = new[] { "kid", "nonce", "typ" };
            foreach (var headerName in requiredHeaders)
            {
                if (!jwtToken.Header.ContainsKey(headerName))
                    return Task.FromResult(false);
            }

            var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            var issClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "iss")?.Value;

            if (subClaim != _configurationManager.ApiKeyId || issClaim != "coinbase-cloud")
                return Task.FromResult(false);

            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    private void EnsureKeyParsed()
    {
        if (_keyParsed) return;

        var privateKey = _configurationManager.PrivateKey;
        if (string.IsNullOrWhiteSpace(privateKey))
            throw new CoinbaseAuthenticationException("Private key cannot be null or empty");

        // Detect key format: PEM keys start with "-----", raw keys are plain base64
        var normalized = privateKey.Replace("\\n", "\n").Trim();
        if (normalized.StartsWith("-----"))
        {
            _ecdsaKey = ParseEcPrivateKey(normalized);
            _useEdDsa = false;
        }
        else
        {
            _ed25519Key = ParseEd25519Key(normalized);
            _useEdDsa = true;
        }

        _keyParsed = true;
    }

    /// <summary>
    /// Generates a JWT signed with ES256 (ECDSA P-256) for legacy PEM keys
    /// </summary>
    private string GenerateEs256Token(DateTime now, DateTime expiration, string uri)
    {
        var claims = new[]
        {
            new Claim("sub", _configurationManager.ApiKeyId),
            new Claim("iss", "coinbase-cloud"),
            new Claim("nbf", new DateTimeOffset(now).ToUnixTimeSeconds().ToString()),
            new Claim("exp", new DateTimeOffset(expiration).ToUnixTimeSeconds().ToString()),
            new Claim("uri", uri)
        };

        var credentials = new SigningCredentials(new ECDsaSecurityKey(_ecdsaKey), SecurityAlgorithms.EcdsaSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiration,
            SigningCredentials = credentials
        };

        var token = _tokenHandler.CreateToken(tokenDescriptor);

        if (token is JwtSecurityToken jwtToken)
        {
            jwtToken.Header["kid"] = _configurationManager.ApiKeyId;
            jwtToken.Header["nonce"] = GenerateNonce();
        }

        return _tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Generates a JWT signed with EdDSA (Ed25519) for new CDP keys.
    /// Built manually since JwtSecurityTokenHandler doesn't support EdDSA.
    /// </summary>
    private string GenerateEdDsaToken(DateTime now, DateTime expiration, string uri)
    {
        var nbf = new DateTimeOffset(now).ToUnixTimeSeconds();
        var exp = new DateTimeOffset(expiration).ToUnixTimeSeconds();

        // Build header JSON
        var header = Newtonsoft.Json.JsonConvert.SerializeObject(new Dictionary<string, string>
        {
            ["alg"] = "EdDSA",
            ["typ"] = "JWT",
            ["kid"] = _configurationManager.ApiKeyId,
            ["nonce"] = GenerateNonce()
        });

        // Build payload JSON
        var payload = Newtonsoft.Json.JsonConvert.SerializeObject(new Dictionary<string, object>
        {
            ["sub"] = _configurationManager.ApiKeyId,
            ["iss"] = "coinbase-cloud",
            ["nbf"] = nbf,
            ["exp"] = exp,
            ["iat"] = nbf,
            ["uri"] = uri
        });

        var headerB64 = Base64UrlEncode(Encoding.UTF8.GetBytes(header));
        var payloadB64 = Base64UrlEncode(Encoding.UTF8.GetBytes(payload));
        var signingInput = $"{headerB64}.{payloadB64}";

        // Sign with Ed25519 using BouncyCastle
        var signer = new Ed25519Signer();
        signer.Init(true, _ed25519Key);
        var inputBytes = Encoding.UTF8.GetBytes(signingInput);
        signer.BlockUpdate(inputBytes, 0, inputBytes.Length);
        var signature = signer.GenerateSignature();

        var signatureB64 = Base64UrlEncode(signature);
        return $"{signingInput}.{signatureB64}";
    }

    /// <summary>
    /// Parses a PEM format EC private key
    /// </summary>
    private ECDsa ParseEcPrivateKey(string privateKeyPem)
    {
        try
        {
            var keyData = privateKeyPem
                .Replace("-----BEGIN PRIVATE KEY-----", "")
                .Replace("-----END PRIVATE KEY-----", "")
                .Replace("-----BEGIN EC PRIVATE KEY-----", "")
                .Replace("-----END EC PRIVATE KEY-----", "")
                .Replace("\n", "")
                .Replace("\r", "")
                .Replace(" ", "")
                .Trim();

            if (string.IsNullOrWhiteSpace(keyData))
                throw new CoinbaseAuthenticationException("Private key contains no valid data after removing PEM headers");

            var keyBytes = Convert.FromBase64String(keyData);
            var ecdsa = ECDsa.Create();

            try
            {
                ecdsa.ImportECPrivateKey(keyBytes, out _);
                return ecdsa;
            }
            catch
            {
                try
                {
                    ecdsa.ImportPkcs8PrivateKey(keyBytes, out _);
                    return ecdsa;
                }
                catch (Exception ex)
                {
                    throw new CoinbaseAuthenticationException(
                        $"Unable to parse EC private key. Ensure it's in valid PEM format. Error: {ex.Message}");
                }
            }
        }
        catch (FormatException ex)
        {
            throw new CoinbaseAuthenticationException($"Invalid base64 encoding in private key: {ex.Message}");
        }
        catch (Exception ex) when (!(ex is CoinbaseAuthenticationException))
        {
            throw new CoinbaseAuthenticationException($"Failed to parse private key: {ex.Message}");
        }
    }

    /// <summary>
    /// Parses a raw base64-encoded Ed25519 private key (new CDP format).
    /// The key is 64 bytes: 32-byte seed + 32-byte public key.
    /// </summary>
    private Ed25519PrivateKeyParameters ParseEd25519Key(string rawBase64)
    {
        try
        {
            var keyBytes = Convert.FromBase64String(rawBase64.Trim());

            if (keyBytes.Length == 64)
            {
                // 64 bytes = seed (32) + public key (32); use the seed
                var seed = new byte[32];
                Array.Copy(keyBytes, 0, seed, 0, 32);
                return new Ed25519PrivateKeyParameters(seed, 0);
            }
            else if (keyBytes.Length == 32)
            {
                // 32 bytes = seed only
                return new Ed25519PrivateKeyParameters(keyBytes, 0);
            }
            else
            {
                throw new CoinbaseAuthenticationException(
                    $"Unexpected Ed25519 key length: {keyBytes.Length} bytes. Expected 32 or 64.");
            }
        }
        catch (FormatException ex)
        {
            throw new CoinbaseAuthenticationException($"Invalid base64 encoding in Ed25519 key: {ex.Message}");
        }
        catch (Exception ex) when (!(ex is CoinbaseAuthenticationException))
        {
            throw new CoinbaseAuthenticationException($"Failed to parse Ed25519 key: {ex.Message}");
        }
    }

    private string GenerateNonce()
    {
        var bytes = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string Base64UrlEncode(byte[] data)
    {
        return Convert.ToBase64String(data)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    /// <summary>
    /// Disposes of the ECDsa key
    /// </summary>
    public void Dispose()
    {
        _ecdsaKey?.Dispose();
    }
}

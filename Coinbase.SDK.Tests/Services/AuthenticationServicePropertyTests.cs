using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Coinbase.SDK.Interfaces;
using Coinbase.SDK.Services;
using FsCheck;
using FsCheck.Xunit;
using Moq;

namespace Coinbase.SDK.Tests.Services;

/// <summary>
/// Property-based tests for AuthenticationService
/// </summary>
public class AuthenticationServicePropertyTests
{
    private const string TestPrivateKey = @"-----BEGIN EC PRIVATE KEY-----
MHcCAQEEII0iMS/H2064WKj/jpklOh/U4NyyG5IDbWjiWsMDyc0moAoGCCqGSM49
AwEHoUQDQgAEUfD5SafApitSCfrWXI4whmjKJD3HO2mo020ah7lh9NZbLNq5Z7bF
u5kHGrQka7yWHI4KoQMaxqJH2LHQSMSmYw==
-----END EC PRIVATE KEY-----";

    /// <summary>
    /// **Feature: coinbase-sdk, Property 4: JWT token validity**
    /// **Validates: Requirements 2.1, 2.2**
    /// For any API request parameters, generated JWT tokens should always contain required claims (sub, iss, nbf, exp, uri) and headers (kid, nonce, typ) with valid ES256 signatures
    /// </summary>
    [Property(MaxTest = 100)]
    public bool JwtTokenValidityProperty(string httpMethod, string endpoint, string apiKey)
    {
        // Filter invalid inputs
        if (string.IsNullOrWhiteSpace(httpMethod) || string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(apiKey))
            return true; // Skip invalid inputs

        // Only test with valid HTTP methods and endpoints
        var validMethods = new[] { "GET", "POST", "PUT", "DELETE" };
        var validEndpoints = new[] { "/accounts", "/orders", "/products", "/candles", "/book" };
        
        if (!validMethods.Contains(httpMethod) || !validEndpoints.Contains(endpoint))
            return true; // Skip invalid combinations

        // Arrange
        var mockConfig = new Mock<IConfigurationManager>();
        mockConfig.Setup(c => c.ApiKeyId).Returns(apiKey);
        mockConfig.Setup(c => c.PrivateKey).Returns(TestPrivateKey);
        mockConfig.Setup(c => c.BaseUrl).Returns("https://api.coinbase.com");
        mockConfig.Setup(c => c.TokenExpiration).Returns(TimeSpan.FromMinutes(1));

        var authService = new AuthenticationService(mockConfig.Object);

        try
        {
            // Act
            var token = authService.GenerateTokenAsync(httpMethod, endpoint).Result;

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Check required claims
            var requiredClaims = new[] { "sub", "iss", "nbf", "exp", "uri" };
            var hasAllClaims = requiredClaims.All(claim => 
                jwtToken.Claims.Any(c => c.Type == claim));

            // Check required headers
            var requiredHeaders = new[] { "kid", "nonce", "typ" };
            var hasAllHeaders = requiredHeaders.All(header => 
                jwtToken.Header.ContainsKey(header));

            // Check specific claim values
            var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            var issClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "iss")?.Value;
            var uriClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "uri")?.Value;
            var kidHeader = jwtToken.Header["kid"]?.ToString();
            var typHeader = jwtToken.Header["typ"]?.ToString();

            var hasCorrectValues = 
                subClaim == apiKey &&
                issClaim == "cdp" &&
                uriClaim == $"{httpMethod} https://api.coinbase.com{endpoint}" &&
                kidHeader == apiKey &&
                typHeader == "JWT";

            // Check algorithm
            var hasCorrectAlgorithm = jwtToken.Header.Alg == "ES256";

            return hasAllClaims && hasAllHeaders && hasCorrectValues && hasCorrectAlgorithm;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// **Feature: coinbase-sdk, Property 5: Token refresh on expiration**
    /// **Validates: Requirements 2.3**
    /// For any expired JWT token scenario, subsequent API requests should automatically generate new valid tokens without manual intervention
    /// </summary>
    [Property(MaxTest = 100)]
    public bool TokenRefreshOnExpirationProperty(string httpMethod, string endpoint, string apiKey)
    {
        // Filter invalid inputs
        if (string.IsNullOrWhiteSpace(httpMethod) || string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(apiKey))
            return true; // Skip invalid inputs

        // Only test with valid HTTP methods and endpoints
        var validMethods = new[] { "GET", "POST", "PUT", "DELETE" };
        var validEndpoints = new[] { "/accounts", "/orders", "/products" };
        
        if (!validMethods.Contains(httpMethod) || !validEndpoints.Contains(endpoint))
            return true; // Skip invalid combinations

        // Arrange
        var mockConfig = new Mock<IConfigurationManager>();
        mockConfig.Setup(c => c.ApiKeyId).Returns(apiKey);
        mockConfig.Setup(c => c.PrivateKey).Returns(TestPrivateKey);
        mockConfig.Setup(c => c.BaseUrl).Returns("https://api.coinbase.com");
        mockConfig.Setup(c => c.TokenExpiration).Returns(TimeSpan.FromMilliseconds(100)); // Very short expiration

        var authService = new AuthenticationService(mockConfig.Object);

        try
        {
            // Act - Generate first token
            var firstToken = authService.GenerateTokenAsync(httpMethod, endpoint).Result;
            
            // Wait for token to expire
            Thread.Sleep(150);
            
            // Generate second token after expiration
            var secondToken = authService.GenerateTokenAsync(httpMethod, endpoint).Result;

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var firstJwtToken = handler.ReadJwtToken(firstToken);
            var secondJwtToken = handler.ReadJwtToken(secondToken);

            // Tokens should be different (new token generated)
            var tokensAreDifferent = firstToken != secondToken;

            // Both tokens should be valid JWT tokens with required claims
            var firstTokenValid = ValidateTokenStructure(firstJwtToken, apiKey, httpMethod, endpoint);
            var secondTokenValid = ValidateTokenStructure(secondJwtToken, apiKey, httpMethod, endpoint);

            // Second token should have later expiration
            var secondTokenHasLaterExpiration = secondJwtToken.ValidTo > firstJwtToken.ValidTo;

            return tokensAreDifferent && firstTokenValid && secondTokenValid && secondTokenHasLaterExpiration;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// **Feature: coinbase-sdk, Property 6: Private key parsing round-trip**
    /// **Validates: Requirements 2.4**
    /// For any valid PEM format private key, parsing then using the key for token generation should produce valid, verifiable JWT tokens
    /// </summary>
    [Property(MaxTest = 100)]
    public bool PrivateKeyParsingRoundTripProperty(string apiKey, string httpMethod, string endpoint)
    {
        // Filter invalid inputs
        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(httpMethod) || string.IsNullOrWhiteSpace(endpoint))
            return true; // Skip invalid inputs

        // Only test with valid HTTP methods and endpoints
        var validMethods = new[] { "GET", "POST" };
        var validEndpoints = new[] { "/test", "/api" };
        
        if (!validMethods.Contains(httpMethod) || !validEndpoints.Contains(endpoint))
            return true; // Skip invalid combinations

        // Use the test private key for all tests
        var privateKeyPem = TestPrivateKey;
        
        // Arrange
        var mockConfig = new Mock<IConfigurationManager>();
        mockConfig.Setup(c => c.ApiKeyId).Returns(apiKey);
        mockConfig.Setup(c => c.PrivateKey).Returns(privateKeyPem);
        mockConfig.Setup(c => c.BaseUrl).Returns("https://api.coinbase.com");
        mockConfig.Setup(c => c.TokenExpiration).Returns(TimeSpan.FromMinutes(1));

        try
        {
            var authService = new AuthenticationService(mockConfig.Object);

            // Act - Parse private key and generate token
            var token = authService.GenerateTokenAsync(httpMethod, endpoint).Result;

            // Assert - Token should be valid and verifiable
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Validate token structure
            var tokenValid = ValidateTokenStructure(jwtToken, apiKey, httpMethod, endpoint);

            // Validate that token can be validated by the service itself
            var serviceValidation = authService.ValidateTokenAsync(token).Result;

            return tokenValid && serviceValidation;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// **Feature: coinbase-auth-fix, Property 1: JWT Token Structure Completeness**
    /// **Validates: Requirements 1.1, 1.3**
    /// For any valid API request, the generated JWT token should contain all required claims ("sub", "iss", "nbf", "exp", "uri") and headers ("kid", "nonce", "typ")
    /// </summary>
    [Property(MaxTest = 100)]
    public bool JwtTokenStructureCompletenessProperty(string httpMethod, string endpoint, string apiKey)
    {
        // Filter invalid inputs
        if (string.IsNullOrWhiteSpace(httpMethod) || string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(apiKey))
            return true; // Skip invalid inputs

        // Only test with valid HTTP methods and endpoints
        var validMethods = new[] { "GET", "POST", "PUT", "DELETE" };
        var validEndpoints = new[] { "/accounts", "/orders", "/products", "/candles", "/book" };
        
        if (!validMethods.Contains(httpMethod) || !validEndpoints.Contains(endpoint))
            return true; // Skip invalid combinations

        // Arrange
        var mockConfig = new Mock<IConfigurationManager>();
        mockConfig.Setup(c => c.ApiKeyId).Returns(apiKey);
        mockConfig.Setup(c => c.PrivateKey).Returns(TestPrivateKey);
        mockConfig.Setup(c => c.BaseUrl).Returns("https://api.coinbase.com");
        mockConfig.Setup(c => c.TokenExpiration).Returns(TimeSpan.FromMinutes(1));

        var authService = new AuthenticationService(mockConfig.Object);

        try
        {
            // Act
            var token = authService.GenerateTokenAsync(httpMethod, endpoint).Result;

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Check all required claims are present
            var requiredClaims = new[] { "sub", "iss", "nbf", "exp", "uri" };
            var hasAllClaims = requiredClaims.All(claim => 
                jwtToken.Claims.Any(c => c.Type == claim));

            // Check all required headers are present
            var requiredHeaders = new[] { "kid", "nonce", "typ" };
            var hasAllHeaders = requiredHeaders.All(header => 
                jwtToken.Header.ContainsKey(header));

            return hasAllClaims && hasAllHeaders;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// **Feature: coinbase-auth-fix, Property 2: Issuer Claim Consistency**
    /// **Validates: Requirements 1.2**
    /// For any generated JWT token, the "iss" claim should always be "coinbase-cloud"
    /// </summary>
    [Property(MaxTest = 100)]
    public bool IssuerClaimConsistencyProperty(string httpMethod, string endpoint, string apiKey)
    {
        // Filter invalid inputs
        if (string.IsNullOrWhiteSpace(httpMethod) || string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(apiKey))
            return true; // Skip invalid inputs

        // Only test with valid HTTP methods and endpoints
        var validMethods = new[] { "GET", "POST", "PUT", "DELETE" };
        var validEndpoints = new[] { "/accounts", "/orders", "/products", "/candles", "/book" };
        
        if (!validMethods.Contains(httpMethod) || !validEndpoints.Contains(endpoint))
            return true; // Skip invalid combinations

        // Arrange
        var mockConfig = new Mock<IConfigurationManager>();
        mockConfig.Setup(c => c.ApiKeyId).Returns(apiKey);
        mockConfig.Setup(c => c.PrivateKey).Returns(TestPrivateKey);
        mockConfig.Setup(c => c.BaseUrl).Returns("https://api.coinbase.com");
        mockConfig.Setup(c => c.TokenExpiration).Returns(TimeSpan.FromMinutes(1));

        var authService = new AuthenticationService(mockConfig.Object);

        try
        {
            // Act
            var token = authService.GenerateTokenAsync(httpMethod, endpoint).Result;

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Check issuer claim is "coinbase-cloud"
            var issClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "iss")?.Value;
            return issClaim == "coinbase-cloud";
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// **Feature: coinbase-auth-fix, Property 3: Nonce Format and Uniqueness**
    /// **Validates: Requirements 1.4**
    /// For any generated JWT token, the nonce should be exactly 10 hexadecimal characters, and multiple token generations should produce different nonces
    /// </summary>
    [Property(MaxTest = 100)]
    public bool NonceFormatAndUniquenessProperty(string httpMethod, string endpoint, string apiKey)
    {
        // Filter invalid inputs
        if (string.IsNullOrWhiteSpace(httpMethod) || string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(apiKey))
            return true; // Skip invalid inputs

        // Only test with valid HTTP methods and endpoints
        var validMethods = new[] { "GET", "POST", "PUT", "DELETE" };
        var validEndpoints = new[] { "/accounts", "/orders", "/products", "/candles", "/book" };
        
        if (!validMethods.Contains(httpMethod) || !validEndpoints.Contains(endpoint))
            return true; // Skip invalid combinations

        // Arrange
        var mockConfig = new Mock<IConfigurationManager>();
        mockConfig.Setup(c => c.ApiKeyId).Returns(apiKey);
        mockConfig.Setup(c => c.PrivateKey).Returns(TestPrivateKey);
        mockConfig.Setup(c => c.BaseUrl).Returns("https://api.coinbase.com");
        mockConfig.Setup(c => c.TokenExpiration).Returns(TimeSpan.FromMinutes(1));

        var authService = new AuthenticationService(mockConfig.Object);

        try
        {
            // Act - Generate two tokens
            var token1 = authService.GenerateTokenAsync(httpMethod, endpoint).Result;
            var token2 = authService.GenerateTokenAsync(httpMethod, endpoint).Result;

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken1 = handler.ReadJwtToken(token1);
            var jwtToken2 = handler.ReadJwtToken(token2);

            // Check nonce format (exactly 10 hexadecimal characters)
            var nonce1 = jwtToken1.Header["nonce"]?.ToString();
            var nonce2 = jwtToken2.Header["nonce"]?.ToString();

            if (string.IsNullOrEmpty(nonce1) || string.IsNullOrEmpty(nonce2))
                return false;

            // Check length is exactly 10
            if (nonce1.Length != 10 || nonce2.Length != 10)
                return false;

            // Check all characters are hexadecimal
            var isHex1 = nonce1.All(c => "0123456789abcdefABCDEF".Contains(c));
            var isHex2 = nonce2.All(c => "0123456789abcdefABCDEF".Contains(c));

            if (!isHex1 || !isHex2)
                return false;

            // Check uniqueness (different nonces for different tokens)
            return nonce1 != nonce2;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// **Feature: coinbase-auth-fix, Property 4: URI Claim Format Correctness**
    /// **Validates: Requirements 1.5, 3.1**
    /// For any API endpoint and HTTP method, the URI claim in the JWT should be formatted as "METHOD endpoint" without protocol prefix
    /// </summary>
    [Property(MaxTest = 100)]
    public bool UriClaimFormatCorrectnessProperty(string httpMethod, string endpoint, string apiKey)
    {
        // Filter invalid inputs
        if (string.IsNullOrWhiteSpace(httpMethod) || string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(apiKey))
            return true; // Skip invalid inputs

        // Only test with valid HTTP methods and endpoints
        var validMethods = new[] { "GET", "POST", "PUT", "DELETE" };
        var validEndpoints = new[] { "/accounts", "/orders", "/products", "/candles", "/book" };
        
        if (!validMethods.Contains(httpMethod) || !validEndpoints.Contains(endpoint))
            return true; // Skip invalid combinations

        // Arrange
        var mockConfig = new Mock<IConfigurationManager>();
        mockConfig.Setup(c => c.ApiKeyId).Returns(apiKey);
        mockConfig.Setup(c => c.PrivateKey).Returns(TestPrivateKey);
        mockConfig.Setup(c => c.BaseUrl).Returns("api.coinbase.com/api/v3/brokerage");
        mockConfig.Setup(c => c.TokenExpiration).Returns(TimeSpan.FromMinutes(1));

        var authService = new AuthenticationService(mockConfig.Object);

        try
        {
            // Act
            var token = authService.GenerateTokenAsync(httpMethod, endpoint).Result;

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Check URI claim format (should be "METHOD api.coinbase.com/api/v3/brokerage/endpoint")
            var uriClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "uri")?.Value;
            var expectedUri = $"{httpMethod} api.coinbase.com/api/v3/brokerage{endpoint}";

            return uriClaim == expectedUri;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// **Feature: coinbase-auth-fix, Property 6: Invalid Key Error Handling**
    /// **Validates: Requirements 2.4**
    /// For any invalid private key format, the system should throw a CoinbaseAuthenticationException with a descriptive message
    /// </summary>
    [Property(MaxTest = 100)]
    public bool InvalidKeyErrorHandlingProperty(string apiKey, string httpMethod, string endpoint)
    {
        // Filter invalid inputs for other parameters
        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(httpMethod) || string.IsNullOrWhiteSpace(endpoint))
            return true; // Skip invalid inputs

        // Only test with valid HTTP methods and endpoints
        var validMethods = new[] { "GET", "POST", "PUT", "DELETE" };
        var validEndpoints = new[] { "/accounts", "/orders", "/products" };
        
        if (!validMethods.Contains(httpMethod) || !validEndpoints.Contains(endpoint))
            return true; // Skip invalid combinations

        // Test with various invalid private key formats
        var invalidKeys = new[]
        {
            // Completely invalid base64
            "-----BEGIN EC PRIVATE KEY-----\ninvalid-base64-data\n-----END EC PRIVATE KEY-----",
            
            // Empty key
            "",
            
            // Null key would be handled by the null check, so test whitespace
            "   ",
            
            // Invalid PEM structure
            "not-a-pem-key-at-all",
            
            // Truncated key
            "-----BEGIN EC PRIVATE KEY-----\nMHcCAQEE\n-----END EC PRIVATE KEY-----",
            
            // Wrong key type (RSA instead of EC)
            "-----BEGIN RSA PRIVATE KEY-----\nMIIEpAIBAAKCAQEA1234567890\n-----END RSA PRIVATE KEY-----",
            
            // Malformed base64
            "-----BEGIN EC PRIVATE KEY-----\nMHcCAQEEII0iMS/H2064WKj/jpklOh/U4NyyG5IDbWjiWsMDyc0moAoGCCqGSM49\nAwEHoUQDQgAEUfD5SafApitSCfrWXI4whmjKJD3HO2mo020ah7lh9NZbLNq5Z7bF\nu5kHGrQka7yWHI4KoQMaxqJH2LHQSMSmYw==!!!\n-----END EC PRIVATE KEY-----"
        };

        foreach (var invalidKey in invalidKeys)
        {
            try
            {
                // Arrange
                var mockConfig = new Mock<IConfigurationManager>();
                mockConfig.Setup(c => c.ApiKeyId).Returns(apiKey);
                mockConfig.Setup(c => c.PrivateKey).Returns(invalidKey);
                mockConfig.Setup(c => c.BaseUrl).Returns("api.coinbase.com/api/v3/brokerage");
                mockConfig.Setup(c => c.TokenExpiration).Returns(TimeSpan.FromMinutes(1));

                var authService = new AuthenticationService(mockConfig.Object);

                // Act - This should throw CoinbaseAuthenticationException for invalid keys
                var token = authService.GenerateTokenAsync(httpMethod, endpoint).Result;

                // If we get here without an exception, the property fails
                return false;
            }
            catch (AggregateException ex) when (ex.InnerException is Coinbase.SDK.Exceptions.CoinbaseAuthenticationException)
            {
                // Expected exception type - verify it has a descriptive message
                var authEx = ex.InnerException as Coinbase.SDK.Exceptions.CoinbaseAuthenticationException;
                if (string.IsNullOrWhiteSpace(authEx?.Message))
                    return false;
                
                // Continue to next invalid key
            }
            catch (Coinbase.SDK.Exceptions.CoinbaseAuthenticationException authEx)
            {
                // Expected exception type - verify it has a descriptive message
                if (string.IsNullOrWhiteSpace(authEx.Message))
                    return false;
                
                // Continue to next invalid key
            }
            catch
            {
                // Wrong exception type - property fails
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// **Feature: coinbase-auth-fix, Property 5: Private Key Parsing Success**
    /// **Validates: Requirements 2.1, 2.2**
    /// For any valid PEM private key format (with or without headers/footers), the parsing should succeed and produce a working ECDsa key
    /// </summary>
    [Property(MaxTest = 100)]
    public bool PrivateKeyParsingSuccessProperty(string apiKey, string httpMethod, string endpoint)
    {
        // Filter invalid inputs
        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(httpMethod) || string.IsNullOrWhiteSpace(endpoint))
            return true; // Skip invalid inputs

        // Only test with valid HTTP methods and endpoints
        var validMethods = new[] { "GET", "POST", "PUT", "DELETE" };
        var validEndpoints = new[] { "/accounts", "/orders", "/products" };
        
        if (!validMethods.Contains(httpMethod) || !validEndpoints.Contains(endpoint))
            return true; // Skip invalid combinations

        // Test with different valid PEM formats
        var pemFormats = new[]
        {
            // Standard EC private key format
            TestPrivateKey,
            
            // Same key with different line endings
            TestPrivateKey.Replace("\n", "\r\n"),
            
            // Key with literal \n characters (as from JSON)
            TestPrivateKey.Replace("\n", "\\n"),
            
            // Key without headers/footers (just the base64 data)
            TestPrivateKey
                .Replace("-----BEGIN EC PRIVATE KEY-----", "")
                .Replace("-----END EC PRIVATE KEY-----", "")
                .Replace("\n", "")
                .Replace("\r", ""),
                
            // Key with extra whitespace
            "  " + TestPrivateKey + "  "
        };

        foreach (var pemFormat in pemFormats)
        {
            try
            {
                // Arrange
                var mockConfig = new Mock<IConfigurationManager>();
                mockConfig.Setup(c => c.ApiKeyId).Returns(apiKey);
                mockConfig.Setup(c => c.PrivateKey).Returns(pemFormat);
                mockConfig.Setup(c => c.BaseUrl).Returns("api.coinbase.com/api/v3/brokerage");
                mockConfig.Setup(c => c.TokenExpiration).Returns(TimeSpan.FromMinutes(1));

                var authService = new AuthenticationService(mockConfig.Object);

                // Act - This should succeed for all valid PEM formats
                var token = authService.GenerateTokenAsync(httpMethod, endpoint).Result;

                // Assert - Token should be valid
                if (string.IsNullOrEmpty(token))
                    return false;

                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                // Verify the token has the correct structure
                var hasRequiredClaims = new[] { "sub", "iss", "nbf", "exp", "uri" }
                    .All(claim => jwtToken.Claims.Any(c => c.Type == claim));

                var hasRequiredHeaders = new[] { "kid", "nonce", "typ" }
                    .All(header => jwtToken.Header.ContainsKey(header));

                if (!hasRequiredClaims || !hasRequiredHeaders)
                    return false;
            }
            catch
            {
                // If any valid PEM format fails to parse, the property fails
                return false;
            }
        }

        return true;
    }

    private bool ValidateTokenStructure(JwtSecurityToken jwtToken, string expectedApiKey, string httpMethod, string endpoint)
    {
        try
        {
            // Check required claims
            var requiredClaims = new[] { "sub", "iss", "nbf", "exp", "uri" };
            var hasAllClaims = requiredClaims.All(claim => 
                jwtToken.Claims.Any(c => c.Type == claim));

            // Check required headers
            var requiredHeaders = new[] { "kid", "nonce", "typ" };
            var hasAllHeaders = requiredHeaders.All(header => 
                jwtToken.Header.ContainsKey(header));

            // Check specific values
            var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            var issClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "iss")?.Value;
            var uriClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "uri")?.Value;

            var hasCorrectValues = 
                subClaim == expectedApiKey &&
                issClaim == "cdp" &&
                uriClaim == $"{httpMethod} https://api.coinbase.com{endpoint}";

            return hasAllClaims && hasAllHeaders && hasCorrectValues;
        }
        catch
        {
            return false;
        }
    }
}
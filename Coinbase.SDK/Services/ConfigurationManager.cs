using Coinbase.SDK.Exceptions;
using Coinbase.SDK.Interfaces;

namespace Coinbase.SDK.Services;

/// <summary>
/// Manages SDK configuration including credentials and endpoints
/// </summary>
public class ConfigurationManager : IConfigurationManager
{
    private const string DefaultBaseUrl = "https://api.coinbase.com";
    private static readonly TimeSpan DefaultTokenExpiration = TimeSpan.FromMinutes(1);

    /// <inheritdoc />
    public string ApiKeyId { get; }

    /// <inheritdoc />
    public string PrivateKey { get; }

    /// <inheritdoc />
    public string BaseUrl { get; }

    /// <inheritdoc />
    public TimeSpan TokenExpiration { get; }

    /// <summary>
    /// Initializes a new instance of the ConfigurationManager class
    /// </summary>
    /// <param name="apiKeyId">The API key ID</param>
    /// <param name="privateKey">The private key</param>
    /// <param name="baseUrl">The base URL (optional, defaults to production URL)</param>
    /// <param name="tokenExpiration">The token expiration time (optional, defaults to 1 minute)</param>
    public ConfigurationManager(string? apiKeyId = null, string? privateKey = null, string? baseUrl = null, TimeSpan? tokenExpiration = null)
    {
        // Load from environment variables if not provided
        ApiKeyId = apiKeyId ?? Environment.GetEnvironmentVariable("COINBASE_API_ID") ?? string.Empty;
        PrivateKey = privateKey ?? Environment.GetEnvironmentVariable("COINBASE_API_PRIVATE_KEY") ?? string.Empty;
        BaseUrl = baseUrl ?? Environment.GetEnvironmentVariable("COINBASE_BASE_URL") ?? DefaultBaseUrl;
        TokenExpiration = tokenExpiration ?? DefaultTokenExpiration;

        ValidateConfiguration();
    }

    /// <inheritdoc />
    public void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(ApiKeyId))
        {
            throw new CoinbaseConfigurationException("API Key ID is required. Set COINBASE_API_ID environment variable or provide it in constructor.");
        }

        if (string.IsNullOrWhiteSpace(PrivateKey))
        {
            throw new CoinbaseConfigurationException("Private Key is required. Set COINBASE_API_PRIVATE_KEY environment variable or provide it in constructor.");
        }

        if (string.IsNullOrWhiteSpace(BaseUrl))
        {
            throw new CoinbaseConfigurationException("Base URL cannot be empty.");
        }

        if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out _))
        {
            throw new CoinbaseConfigurationException($"Base URL '{BaseUrl}' is not a valid URI.");
        }

        if (TokenExpiration <= TimeSpan.Zero)
        {
            throw new CoinbaseConfigurationException("Token expiration must be greater than zero.");
        }
    }
}
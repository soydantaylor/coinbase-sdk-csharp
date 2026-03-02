namespace Coinbase.SDK.Configuration;

/// <summary>
/// Configuration options for the Coinbase SDK
/// </summary>
public class CoinbaseOptions
{
    /// <summary>
    /// The configuration section name for binding from configuration
    /// </summary>
    public const string SectionName = "Coinbase";

    /// <summary>
    /// Gets or sets the API key ID for authentication
    /// </summary>
    public string? ApiKeyId { get; set; }

    /// <summary>
    /// Gets or sets the private key for JWT token generation
    /// </summary>
    public string? PrivateKey { get; set; }

    /// <summary>
    /// Gets or sets the base URL for API endpoints
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// Gets or sets the token expiration time in minutes (default: 1 minute)
    /// </summary>
    public int TokenExpirationMinutes { get; set; } = 1;

    /// <summary>
    /// Gets the token expiration as a TimeSpan
    /// </summary>
    public TimeSpan TokenExpiration => TimeSpan.FromMinutes(TokenExpirationMinutes);
}
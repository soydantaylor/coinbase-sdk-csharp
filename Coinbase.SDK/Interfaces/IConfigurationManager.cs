using Coinbase.SDK.Exceptions;

namespace Coinbase.SDK.Interfaces;

/// <summary>
/// Manages SDK configuration including credentials and endpoints
/// </summary>
public interface IConfigurationManager
{
    /// <summary>
    /// Gets the API key ID for authentication
    /// </summary>
    string ApiKeyId { get; }

    /// <summary>
    /// Gets the private key for JWT token generation
    /// </summary>
    string PrivateKey { get; }

    /// <summary>
    /// Gets the base URL for API endpoints
    /// </summary>
    string BaseUrl { get; }

    /// <summary>
    /// Gets the token expiration time span
    /// </summary>
    TimeSpan TokenExpiration { get; }

    /// <summary>
    /// Validates the current configuration and throws exceptions if invalid
    /// </summary>
    /// <exception cref="CoinbaseConfigurationException">Thrown when configuration is invalid</exception>
    void ValidateConfiguration();
}
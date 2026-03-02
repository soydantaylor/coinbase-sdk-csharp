namespace Coinbase.SDK.Interfaces;

/// <summary>
/// Handles JWT token generation and validation for API authentication
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Generates a JWT token for the specified HTTP method and endpoint
    /// </summary>
    /// <param name="httpMethod">The HTTP method (GET, POST, etc.)</param>
    /// <param name="endpoint">The API endpoint path</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A valid JWT token string</returns>
    Task<string> GenerateTokenAsync(string httpMethod, string endpoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a JWT token
    /// </summary>
    /// <param name="token">The JWT token to validate</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>True if the token is valid, false otherwise</returns>
    Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
}
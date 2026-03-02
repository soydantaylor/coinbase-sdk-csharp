namespace Coinbase.SDK.Interfaces;

/// <summary>
/// Abstracts HTTP communication with the Coinbase API
/// </summary>
public interface IApiClient
{
    /// <summary>
    /// Performs a GET request to the specified endpoint
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response to</typeparam>
    /// <param name="endpoint">The API endpoint path</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>The deserialized response object</returns>
    Task<T> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a POST request to the specified endpoint
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response to</typeparam>
    /// <param name="endpoint">The API endpoint path</param>
    /// <param name="request">The request object to serialize and send</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>The deserialized response object</returns>
    Task<T> PostAsync<T>(string endpoint, object request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a PUT request to the specified endpoint
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response to</typeparam>
    /// <param name="endpoint">The API endpoint path</param>
    /// <param name="request">The request object to serialize and send</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>The deserialized response object</returns>
    Task<T> PutAsync<T>(string endpoint, object request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a DELETE request to the specified endpoint
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response to</typeparam>
    /// <param name="endpoint">The API endpoint path</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>The deserialized response object</returns>
    Task<T> DeleteAsync<T>(string endpoint, CancellationToken cancellationToken = default);
}
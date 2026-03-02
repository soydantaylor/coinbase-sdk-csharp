using System.Net;
using System.Text;
using Newtonsoft.Json;
using Coinbase.SDK.Exceptions;
using Coinbase.SDK.Interfaces;

namespace Coinbase.SDK.Services;

/// <summary>
/// Handles HTTP communication with the Coinbase API
/// </summary>
public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IAuthenticationService _authenticationService;
    private readonly IConfigurationManager _configurationManager;
    private readonly JsonSerializerSettings _jsonSettings;

    /// <summary>
    /// Initializes a new instance of the ApiClient class
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for requests</param>
    /// <param name="authenticationService">The authentication service for JWT tokens</param>
    /// <param name="configurationManager">The configuration manager</param>
    public ApiClient(HttpClient httpClient, IAuthenticationService authenticationService, IConfigurationManager configurationManager)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
        _configurationManager = configurationManager ?? throw new ArgumentNullException(nameof(configurationManager));

        _jsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc
        };

        // Configure HttpClient base address if not already set
        if (_httpClient.BaseAddress == null)
        {
            _httpClient.BaseAddress = new Uri(_configurationManager.BaseUrl);
        }
    }

    /// <inheritdoc />
    public async Task<T> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        return await SendRequestAsync<T>(HttpMethod.Get, endpoint, null, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<T> PostAsync<T>(string endpoint, object request, CancellationToken cancellationToken = default)
    {
        return await SendRequestAsync<T>(HttpMethod.Post, endpoint, request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<T> PutAsync<T>(string endpoint, object request, CancellationToken cancellationToken = default)
    {
        return await SendRequestAsync<T>(HttpMethod.Put, endpoint, request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<T> DeleteAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        return await SendRequestAsync<T>(HttpMethod.Delete, endpoint, null, cancellationToken);
    }

    /// <summary>
    /// Sends an HTTP request with retry logic and proper error handling
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response to</typeparam>
    /// <param name="method">The HTTP method</param>
    /// <param name="endpoint">The API endpoint path</param>
    /// <param name="requestBody">The request body object (null for GET/DELETE)</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>The deserialized response object</returns>
    private async Task<T> SendRequestAsync<T>(HttpMethod method, string endpoint, object? requestBody, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
            throw new ArgumentException("Endpoint cannot be null or empty", nameof(endpoint));

        // Ensure endpoint starts with /
        if (!endpoint.StartsWith("/"))
            endpoint = "/" + endpoint;

        const int maxRetries = 3;
        var retryDelays = new[] { TimeSpan.FromMilliseconds(500), TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2) };

        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                using var request = await CreateHttpRequestAsync(method, endpoint, requestBody, cancellationToken);
                using var response = await _httpClient.SendAsync(request, cancellationToken);
                Console.WriteLine($"{request.Method}: {request.RequestUri.PathAndQuery} - {(int)response.StatusCode} {response.ReasonPhrase}");
                
                return await ProcessResponseAsync<T>(response, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw; // Don't retry on cancellation
            }
            catch (CoinbaseApiException ex) when (ex.HttpStatusCode >= 400 && ex.HttpStatusCode < 500)
            {
                throw; // Don't retry on client errors (4xx)
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                // Retry on network errors and server errors (5xx)
                if (IsRetryableException(ex))
                {
                    await Task.Delay(retryDelays[attempt], cancellationToken);
                    continue;
                }
                throw;
            }
        }

        throw new CoinbaseNetworkException("Maximum retry attempts exceeded", new InvalidOperationException("This should not be reached"));
    }

    /// <summary>
    /// Creates an HTTP request message with proper authentication headers
    /// </summary>
    /// <param name="method">The HTTP method</param>
    /// <param name="endpoint">The API endpoint path</param>
    /// <param name="requestBody">The request body object</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>The configured HTTP request message</returns>
    private async Task<HttpRequestMessage> CreateHttpRequestAsync(HttpMethod method, string endpoint, object? requestBody, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(method, endpoint);

        // Add authentication header (strip query string — Coinbase signs path only)
        try
        {
            var authEndpoint = endpoint.Contains('?') ? endpoint.Substring(0, endpoint.IndexOf('?')) : endpoint;
            var token = await _authenticationService.GenerateTokenAsync(method.Method, authEndpoint, cancellationToken);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
        catch (Exception ex)
        {
            throw new CoinbaseAuthenticationException($"Failed to generate authentication token: {ex.Message}", ex);
        }

        // Add standard headers
        request.Headers.Add("Accept", "application/json");

        // Add request body for POST/PUT requests
        if (requestBody != null && (method == HttpMethod.Post || method == HttpMethod.Put))
        {
            var json = JsonConvert.SerializeObject(requestBody, _jsonSettings);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        return request;
    }

    /// <summary>
    /// Processes the HTTP response and handles errors
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response to</typeparam>
    /// <param name="response">The HTTP response message</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>The deserialized response object</returns>
    private async Task<T> ProcessResponseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            await HandleErrorResponseAsync(response, content);
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new CoinbaseApiException("Empty response received from API", (int)response.StatusCode);
        }

        try
        {
            var result = JsonConvert.DeserializeObject<T>(content, _jsonSettings);
            if (result == null)
            {
                throw new CoinbaseApiException("Failed to deserialize response: result is null", (int)response.StatusCode);
            }
            return result;
        }
        catch (JsonException ex)
        {
            throw new CoinbaseApiException($"Failed to deserialize response: {ex.Message}", (int)response.StatusCode, ex);
        }
    }

    /// <summary>
    /// Handles error responses from the API
    /// </summary>
    /// <param name="response">The HTTP response message</param>
    /// <param name="content">The response content</param>
    private async Task HandleErrorResponseAsync(HttpResponseMessage response, string content)
    {
        var statusCode = (int)response.StatusCode;
        var errorMessage = $"API request failed with status {statusCode} ({response.StatusCode})";

        // Try to parse error details from response
        string? apiErrorCode = null;
        string? apiErrorMessage = null;

        if (!string.IsNullOrWhiteSpace(content))
        {
            try
            {
                var errorResponse = JsonConvert.DeserializeObject<dynamic>(content);
                apiErrorMessage = errorResponse?.message?.ToString() ?? errorResponse?.error?.ToString();
                apiErrorCode = errorResponse?.error_code?.ToString() ?? errorResponse?.code?.ToString();
            }
            catch
            {
                // If we can't parse the error response, use the raw content
                apiErrorMessage = content;
            }
        }

        if (!string.IsNullOrWhiteSpace(apiErrorMessage))
        {
            errorMessage = apiErrorMessage;
        }

        // Throw appropriate exception based on status code
        switch (response.StatusCode)
        {
            case HttpStatusCode.Unauthorized:
            case HttpStatusCode.Forbidden:
                throw new CoinbaseAuthenticationException($"Authentication failed: {errorMessage}");
            
            case HttpStatusCode.TooManyRequests:
                throw new CoinbaseApiException($"Rate limit exceeded: {errorMessage}", statusCode, "RATE_LIMIT_EXCEEDED");
            
            case HttpStatusCode.BadRequest:
                throw new CoinbaseApiException($"Bad request: {errorMessage}", statusCode, apiErrorCode ?? "BAD_REQUEST");
            
            case HttpStatusCode.NotFound:
                throw new CoinbaseApiException($"Resource not found: {errorMessage}", statusCode, "NOT_FOUND");
            
            case HttpStatusCode.InternalServerError:
            case HttpStatusCode.BadGateway:
            case HttpStatusCode.ServiceUnavailable:
            case HttpStatusCode.GatewayTimeout:
                throw new CoinbaseApiException($"Server error: {errorMessage}", statusCode, "SERVER_ERROR");
            
            default:
                throw new CoinbaseApiException(errorMessage, statusCode, apiErrorCode);
        }
    }

    /// <summary>
    /// Determines if an exception is retryable
    /// </summary>
    /// <param name="exception">The exception to check</param>
    /// <returns>True if the exception is retryable, false otherwise</returns>
    private static bool IsRetryableException(Exception exception)
    {
        return exception switch
        {
            HttpRequestException => true,
            TaskCanceledException when !exception.Message.Contains("timeout") => false, // Actual cancellation
            TaskCanceledException => true, // Timeout
            CoinbaseNetworkException => true,
            CoinbaseApiException apiEx when apiEx.HttpStatusCode >= 500 => true,
            CoinbaseApiException apiEx when apiEx.HttpStatusCode == 429 => true, // Rate limiting
            _ => false
        };
    }
}
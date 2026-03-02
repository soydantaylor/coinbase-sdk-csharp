using System.Net;
using System.Text.Json;
using Coinbase.SDK.Exceptions;

namespace Coinbase.SDK.Utilities;

/// <summary>
/// Utility class for parsing error responses from the Coinbase API
/// </summary>
public static class ErrorResponseParser
{
    /// <summary>
    /// Parses an HTTP error response and creates the appropriate exception
    /// </summary>
    /// <param name="statusCode">The HTTP status code</param>
    /// <param name="responseContent">The response content</param>
    /// <returns>The appropriate CoinbaseException</returns>
    public static CoinbaseException ParseErrorResponse(HttpStatusCode statusCode, string? responseContent)
    {
        var errorMessage = "An error occurred while communicating with the Coinbase API";
        string? errorCode = null;

        // Try to parse the error response JSON
        if (!string.IsNullOrEmpty(responseContent))
        {
            try
            {
                var errorResponse = ParseErrorJson(responseContent);
                errorMessage = errorResponse.Message ?? errorMessage;
                errorCode = errorResponse.ErrorCode;
            }
            catch
            {
                // If JSON parsing fails, use the raw response content as the error message
                errorMessage = responseContent;
            }
        }

        // Map HTTP status codes to appropriate exception types
        return statusCode switch
        {
            HttpStatusCode.Unauthorized => new CoinbaseAuthenticationException($"Authentication failed: {errorMessage}"),
            HttpStatusCode.Forbidden => new CoinbaseAuthenticationException($"Access forbidden: {errorMessage}"),
            HttpStatusCode.BadRequest => new CoinbaseApiException(errorMessage, (int)statusCode, errorCode),
            HttpStatusCode.NotFound => new CoinbaseApiException($"Resource not found: {errorMessage}", (int)statusCode, errorCode),
            HttpStatusCode.TooManyRequests => new CoinbaseApiException($"Rate limit exceeded: {errorMessage}", (int)statusCode, errorCode),
            HttpStatusCode.InternalServerError => new CoinbaseApiException($"Internal server error: {errorMessage}", (int)statusCode, errorCode),
            HttpStatusCode.BadGateway => new CoinbaseApiException($"Bad gateway: {errorMessage}", (int)statusCode, errorCode),
            HttpStatusCode.ServiceUnavailable => new CoinbaseApiException($"Service unavailable: {errorMessage}", (int)statusCode, errorCode),
            HttpStatusCode.GatewayTimeout => new CoinbaseApiException($"Gateway timeout: {errorMessage}", (int)statusCode, errorCode),
            _ => new CoinbaseApiException($"HTTP {(int)statusCode}: {errorMessage}", (int)statusCode, errorCode)
        };
    }

    /// <summary>
    /// Creates a network exception from an underlying exception
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="innerException">The underlying exception</param>
    /// <returns>A CoinbaseNetworkException</returns>
    public static CoinbaseNetworkException CreateNetworkException(string message, Exception innerException)
    {
        return new CoinbaseNetworkException($"Network error: {message}", innerException);
    }

    /// <summary>
    /// Parses error JSON response
    /// </summary>
    /// <param name="jsonContent">The JSON content</param>
    /// <returns>Parsed error information</returns>
    private static ErrorResponse ParseErrorJson(string jsonContent)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // Try common error response formats
            var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(jsonContent, options);
            if (errorResponse != null)
            {
                return errorResponse;
            }

            // Try alternative format with "error" wrapper
            var wrappedError = JsonSerializer.Deserialize<WrappedErrorResponse>(jsonContent, options);
            if (wrappedError?.Error != null)
            {
                return wrappedError.Error;
            }

            // Try array format
            var errorArray = JsonSerializer.Deserialize<ErrorResponse[]>(jsonContent, options);
            if (errorArray?.Length > 0)
            {
                return errorArray[0];
            }
        }
        catch
        {
            // If all parsing attempts fail, return a generic error response
        }

        return new ErrorResponse
        {
            Message = jsonContent,
            ErrorCode = "PARSE_ERROR"
        };
    }

    /// <summary>
    /// Represents an error response from the API
    /// </summary>
    private class ErrorResponse
    {
        public string? Message { get; set; }
        public string? ErrorCode { get; set; }
        public string? Error { get; set; }
        public string? ErrorDescription { get; set; }
        public string? Details { get; set; }

        // Handle different message field names
        public string? GetMessage()
        {
            return Message ?? Error ?? ErrorDescription ?? Details;
        }
    }

    /// <summary>
    /// Represents a wrapped error response
    /// </summary>
    private class WrappedErrorResponse
    {
        public ErrorResponse? Error { get; set; }
    }
}
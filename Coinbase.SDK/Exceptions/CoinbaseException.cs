namespace Coinbase.SDK.Exceptions;

/// <summary>
/// Base exception class for all Coinbase SDK exceptions
/// </summary>
public abstract class CoinbaseException : Exception
{
    /// <summary>
    /// Gets the error code associated with this exception
    /// </summary>
    public string? ErrorCode { get; }

    /// <summary>
    /// Initializes a new instance of the CoinbaseException class
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="errorCode">The error code</param>
    protected CoinbaseException(string message, string? errorCode = null) : base(message)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Initializes a new instance of the CoinbaseException class
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="innerException">The inner exception</param>
    /// <param name="errorCode">The error code</param>
    protected CoinbaseException(string message, Exception innerException, string? errorCode = null) : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}

/// <summary>
/// Exception thrown when authentication fails
/// </summary>
public class CoinbaseAuthenticationException : CoinbaseException
{
    /// <summary>
    /// Initializes a new instance of the CoinbaseAuthenticationException class
    /// </summary>
    /// <param name="message">The error message</param>
    public CoinbaseAuthenticationException(string message) : base(message, "AUTH_ERROR")
    {
    }

    /// <summary>
    /// Initializes a new instance of the CoinbaseAuthenticationException class
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="innerException">The inner exception</param>
    public CoinbaseAuthenticationException(string message, Exception innerException) : base(message, innerException, "AUTH_ERROR")
    {
    }
}

/// <summary>
/// Exception thrown when API calls fail
/// </summary>
public class CoinbaseApiException : CoinbaseException
{
    /// <summary>
    /// Gets the HTTP status code associated with this exception
    /// </summary>
    public int HttpStatusCode { get; }

    /// <summary>
    /// Initializes a new instance of the CoinbaseApiException class
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="statusCode">The HTTP status code</param>
    /// <param name="errorCode">The error code</param>
    public CoinbaseApiException(string message, int statusCode, string? errorCode = null) : base(message, errorCode)
    {
        HttpStatusCode = statusCode;
    }

    /// <summary>
    /// Initializes a new instance of the CoinbaseApiException class
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="statusCode">The HTTP status code</param>
    /// <param name="innerException">The inner exception</param>
    /// <param name="errorCode">The error code</param>
    public CoinbaseApiException(string message, int statusCode, Exception innerException, string? errorCode = null) : base(message, innerException, errorCode)
    {
        HttpStatusCode = statusCode;
    }
}

/// <summary>
/// Exception thrown when network connectivity issues occur
/// </summary>
public class CoinbaseNetworkException : CoinbaseException
{
    /// <summary>
    /// Initializes a new instance of the CoinbaseNetworkException class
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="innerException">The inner exception</param>
    public CoinbaseNetworkException(string message, Exception innerException) : base(message, innerException, "NETWORK_ERROR")
    {
    }
}

/// <summary>
/// Exception thrown when configuration is invalid
/// </summary>
public class CoinbaseConfigurationException : CoinbaseException
{
    /// <summary>
    /// Initializes a new instance of the CoinbaseConfigurationException class
    /// </summary>
    /// <param name="message">The error message</param>
    public CoinbaseConfigurationException(string message) : base(message, "CONFIG_ERROR")
    {
    }

    /// <summary>
    /// Initializes a new instance of the CoinbaseConfigurationException class
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="innerException">The inner exception</param>
    public CoinbaseConfigurationException(string message, Exception innerException) : base(message, innerException, "CONFIG_ERROR")
    {
    }
}

/// <summary>
/// Exception thrown when transfer operations fail
/// </summary>
public class CoinbaseTransferException : CoinbaseException
{
    /// <summary>
    /// Initializes a new instance of the CoinbaseTransferException class
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="errorCode">The error code</param>
    public CoinbaseTransferException(string message, string? errorCode = null) : base(message, errorCode ?? "TRANSFER_ERROR")
    {
    }

    /// <summary>
    /// Initializes a new instance of the CoinbaseTransferException class
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="innerException">The inner exception</param>
    /// <param name="errorCode">The error code</param>
    public CoinbaseTransferException(string message, Exception innerException, string? errorCode = null) : base(message, innerException, errorCode ?? "TRANSFER_ERROR")
    {
    }
}

/// <summary>
/// Exception thrown when trading operations fail
/// </summary>
public class CoinbaseTradingException : CoinbaseException
{
    /// <summary>
    /// Initializes a new instance of the CoinbaseTradingException class
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="errorCode">The error code</param>
    public CoinbaseTradingException(string message, string? errorCode = null) : base(message, errorCode ?? "TRADING_ERROR")
    {
    }

    /// <summary>
    /// Initializes a new instance of the CoinbaseTradingException class
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="innerException">The inner exception</param>
    /// <param name="errorCode">The error code</param>
    public CoinbaseTradingException(string message, Exception innerException, string? errorCode = null) : base(message, innerException, errorCode ?? "TRADING_ERROR")
    {
    }
}

/// <summary>
/// Exception thrown when validation fails
/// </summary>
public class CoinbaseValidationException : CoinbaseException
{
    /// <summary>
    /// Initializes a new instance of the CoinbaseValidationException class
    /// </summary>
    /// <param name="message">The error message</param>
    public CoinbaseValidationException(string message) : base(message, "VALIDATION_ERROR")
    {
    }

    /// <summary>
    /// Initializes a new instance of the CoinbaseValidationException class
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="innerException">The inner exception</param>
    public CoinbaseValidationException(string message, Exception innerException) : base(message, innerException, "VALIDATION_ERROR")
    {
    }
}
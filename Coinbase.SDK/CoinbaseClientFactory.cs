using Coinbase.SDK.Interfaces;
using Coinbase.SDK.Services;
using Coinbase.SDK.Utilities;

namespace Coinbase.SDK;

/// <summary>
/// Factory class for creating CoinbaseClient instances without dependency injection
/// </summary>
public static class CoinbaseClientFactory
{
    /// <summary>
    /// Creates a new CoinbaseClient instance with the specified credentials
    /// </summary>
    /// <param name="apiKeyId">The API key ID</param>
    /// <param name="privateKey">The private key</param>
    /// <param name="baseUrl">Optional base URL (defaults to production)</param>
    /// <returns>A configured CoinbaseClient instance</returns>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null</exception>
    public static ICoinbaseClient Create(string apiKeyId, string privateKey, string? baseUrl = null)
    {
        if (string.IsNullOrWhiteSpace(apiKeyId))
            throw new ArgumentNullException(nameof(apiKeyId));
        if (string.IsNullOrWhiteSpace(privateKey))
            throw new ArgumentNullException(nameof(privateKey));

        var configurationManager = new ConfigurationManager(apiKeyId, privateKey, baseUrl);
        var authenticationService = new AuthenticationService(configurationManager);
        var httpClient = new HttpClient();
        var apiClient = new ApiClient(httpClient, authenticationService, configurationManager);

        return new CoinbaseClient(apiClient);
    }

    /// <summary>
    /// Creates a new CoinbaseClient instance using environment variables for credentials
    /// </summary>
    /// <param name="baseUrl">Optional base URL (defaults to production or environment variable)</param>
    /// <returns>A configured CoinbaseClient instance</returns>
    /// <exception cref="InvalidOperationException">Thrown when required environment variables are not set</exception>
    public static ICoinbaseClient CreateFromEnvironment(string? baseUrl = null)
    {
        var configurationManager = new ConfigurationManager(baseUrl: baseUrl);
        var authenticationService = new AuthenticationService(configurationManager);
        var httpClient = new HttpClient();
        var apiClient = new ApiClient(httpClient, authenticationService, configurationManager);

        return new CoinbaseClient(apiClient);
    }

    
    /// <summary>
    /// Creates a new CoinbaseClient instance from a Coinbase JSON credentials file
    /// </summary>
    /// <param name="credentialsFilePath">Path to the Coinbase JSON credentials file</param>
    /// <param name="baseUrl">Optional base URL (defaults to production)</param>
    /// <returns>A configured CoinbaseClient instance</returns>
    /// <exception cref="ArgumentException">Thrown when file path is invalid</exception>
    /// <exception cref="FileNotFoundException">Thrown when credentials file is not found</exception>
    /// <exception cref="InvalidOperationException">Thrown when credentials file is invalid</exception>
    public static async Task<ICoinbaseClient> CreateFromJsonFileAsync(string credentialsFilePath, string? baseUrl = null)
    {
        var (apiKeyId, privateKey) = await PrivateKeyHelper.LoadCredentialsFromFileAsync(credentialsFilePath);
        return Create(apiKeyId, privateKey, baseUrl);
    }

    /// <summary>
    /// Creates a new CoinbaseClient instance from Coinbase JSON credentials content
    /// </summary>
    /// <param name="jsonContent">The JSON content from Coinbase credentials</param>
    /// <param name="baseUrl">Optional base URL (defaults to production)</param>
    /// <returns>A configured CoinbaseClient instance</returns>
    /// <exception cref="ArgumentException">Thrown when JSON content is invalid</exception>
    /// <exception cref="InvalidOperationException">Thrown when required properties are missing</exception>
    public static ICoinbaseClient CreateFromJson(string jsonContent, string? baseUrl = null)
    {
        var apiKeyId = PrivateKeyHelper.ExtractApiKeyIdFromJson(jsonContent);
        var privateKey = PrivateKeyHelper.ExtractPrivateKeyFromJson(jsonContent);
        return Create(apiKeyId, privateKey, baseUrl);
    }

    /// <summary>
    /// Creates a new CoinbaseClient instance with custom service implementations
    /// </summary>
    /// <param name="configurationManager">The configuration manager</param>
    /// <param name="authenticationService">Optional custom authentication service</param>
    /// <param name="httpClient">Optional custom HTTP client</param>
    /// <returns>A configured CoinbaseClient instance</returns>
    /// <exception cref="ArgumentNullException">Thrown when configurationManager is null</exception>
    public static ICoinbaseClient Create(
        IConfigurationManager configurationManager,
        IAuthenticationService? authenticationService = null,
        HttpClient? httpClient = null)
    {
        if (configurationManager == null)
            throw new ArgumentNullException(nameof(configurationManager));

        authenticationService ??= new AuthenticationService(configurationManager);
        httpClient ??= new HttpClient();
        var apiClient = new ApiClient(httpClient, authenticationService, configurationManager);

        return new CoinbaseClient(apiClient);
    }
}
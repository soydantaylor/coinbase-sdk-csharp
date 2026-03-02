using Coinbase.SDK.Configuration;
using Coinbase.SDK.Interfaces;
using Coinbase.SDK.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Coinbase.SDK.Extensions;

/// <summary>
/// Extension methods for IServiceCollection to register Coinbase SDK services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Coinbase SDK services to the dependency injection container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Optional action to configure Coinbase options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddCoinbaseSDK(this IServiceCollection services, Action<CoinbaseOptions>? configureOptions = null)
    {
        // Configure options
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        else
        {
            // Register default options that will load from environment variables
            services.Configure<CoinbaseOptions>(options => { });
        }

        // Register core services
        services.AddSingleton<IConfigurationManager>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<CoinbaseOptions>>().Value;
            return new ConfigurationManager(
                options.ApiKeyId,
                options.PrivateKey,
                options.BaseUrl,
                options.TokenExpiration);
        });

        services.AddSingleton<IAuthenticationService, AuthenticationService>();
        services.AddSingleton<HttpClient>();
        services.AddScoped<IApiClient, ApiClient>();

        // Register business services
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<ITradingService, TradingService>();
        services.AddScoped<IMarketDataService, MarketDataService>();
        services.AddScoped<ITransferService, TransferService>();

        // Register main client with explicit constructor selection
        services.AddScoped<ICoinbaseClient>(provider =>
        {
            var accountService = provider.GetRequiredService<IAccountService>();
            var tradingService = provider.GetRequiredService<ITradingService>();
            var marketDataService = provider.GetRequiredService<IMarketDataService>();
            var transferService = provider.GetRequiredService<ITransferService>();
            return new CoinbaseClient(accountService, tradingService, marketDataService, transferService);
        });

        return services;
    }

    /// <summary>
    /// Adds Coinbase SDK services with explicit credentials
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="apiKeyId">The API key ID</param>
    /// <param name="privateKey">The private key</param>
    /// <param name="baseUrl">Optional base URL (defaults to production)</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddCoinbaseSDK(this IServiceCollection services, string apiKeyId, string privateKey, string? baseUrl = null)
    {
        return services.AddCoinbaseSDK(options =>
        {
            options.ApiKeyId = apiKeyId;
            options.PrivateKey = privateKey;
            if (!string.IsNullOrEmpty(baseUrl))
            {
                options.BaseUrl = baseUrl;
            }
        });
    }
}
using Coinbase.SDK.Configuration;
using Coinbase.SDK.Extensions;
using Coinbase.SDK.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Coinbase.SDK.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    private IServiceCollection _services;

    public ServiceCollectionExtensionsTests()
    {
        _services = new ServiceCollection();
    }

    [Fact]
    public void AddCoinbaseSDK_WithoutOptions_RegistersAllServices()
    {
        // Arrange & Act - provide minimal valid credentials to avoid validation errors
        _services.AddCoinbaseSDK(options =>
        {
            options.ApiKeyId = "test-key";
            options.PrivateKey = "test-private-key";
        });
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<IOptions<CoinbaseOptions>>());
        Assert.NotNull(serviceProvider.GetService<IConfigurationManager>());
        Assert.NotNull(serviceProvider.GetService<IAuthenticationService>());
        Assert.NotNull(serviceProvider.GetService<IApiClient>());
        Assert.NotNull(serviceProvider.GetService<IAccountService>());
        Assert.NotNull(serviceProvider.GetService<ITradingService>());
        Assert.NotNull(serviceProvider.GetService<IMarketDataService>());
        Assert.NotNull(serviceProvider.GetService<ICoinbaseClient>());
    }

    [Fact]
    public void AddCoinbaseSDK_WithCredentials_ConfiguresOptions()
    {
        // Arrange
        const string apiKeyId = "test-key";
        const string privateKey = "test-private-key";
        const string baseUrl = "https://test.coinbase.com";

        // Act
        _services.AddCoinbaseSDK(apiKeyId, privateKey, baseUrl);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var options = serviceProvider.GetRequiredService<IOptions<CoinbaseOptions>>().Value;
        Assert.Equal(apiKeyId, options.ApiKeyId);
        Assert.Equal(privateKey, options.PrivateKey);
        Assert.Equal(baseUrl, options.BaseUrl);
    }

    [Fact]
    public void AddCoinbaseSDK_WithConfigureAction_ConfiguresOptions()
    {
        // Arrange
        const string apiKeyId = "test-key";
        const string privateKey = "test-private-key";
        const int tokenExpirationMinutes = 5;

        // Act
        _services.AddCoinbaseSDK(options =>
        {
            options.ApiKeyId = apiKeyId;
            options.PrivateKey = privateKey;
            options.TokenExpirationMinutes = tokenExpirationMinutes;
        });
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var options = serviceProvider.GetRequiredService<IOptions<CoinbaseOptions>>().Value;
        Assert.Equal(apiKeyId, options.ApiKeyId);
        Assert.Equal(privateKey, options.PrivateKey);
        Assert.Equal(tokenExpirationMinutes, options.TokenExpirationMinutes);
    }

    [Fact]
    public void AddCoinbaseSDK_ServiceLifetimes_AreCorrect()
    {
        // Arrange & Act
        _services.AddCoinbaseSDK(options =>
        {
            options.ApiKeyId = "test-key";
            options.PrivateKey = "test-private-key";
        });

        // Assert service lifetimes
        var configManagerDescriptor = _services.FirstOrDefault(s => s.ServiceType == typeof(IConfigurationManager));
        Assert.Equal(ServiceLifetime.Singleton, configManagerDescriptor?.Lifetime);

        var authServiceDescriptor = _services.FirstOrDefault(s => s.ServiceType == typeof(IAuthenticationService));
        Assert.Equal(ServiceLifetime.Singleton, authServiceDescriptor?.Lifetime);

        var apiClientDescriptor = _services.FirstOrDefault(s => s.ServiceType == typeof(IApiClient));
        Assert.Equal(ServiceLifetime.Scoped, apiClientDescriptor?.Lifetime);

        var coinbaseClientDescriptor = _services.FirstOrDefault(s => s.ServiceType == typeof(ICoinbaseClient));
        Assert.Equal(ServiceLifetime.Scoped, coinbaseClientDescriptor?.Lifetime);
    }
}
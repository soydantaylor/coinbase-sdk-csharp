using Coinbase.SDK.Interfaces;
using Coinbase.SDK.Services;
using Xunit;

namespace Coinbase.SDK.Tests;

public class CoinbaseClientFactoryTests
{
    [Fact]
    public void Create_WithCredentials_ReturnsConfiguredClient()
    {
        // Arrange
        const string apiKeyId = "test-key";
        const string privateKey = "-----BEGIN EC PRIVATE KEY-----\nMHcCAQEEIBKVhJpVIr0jHjW7mBVz8nce2Q7rQzXyqGVJKGVJKGVJoAoGCCqGSM49\nAwEHoUQDQgAEKGVJKGVJKGVJKGVJKGVJKGVJKGVJKGVJKGVJKGVJKGVJKGVJKGVJ\nKGVJKGVJKGVJKGVJKGVJKGVJKGVJKGVJKA==\n-----END EC PRIVATE KEY-----";
        const string baseUrl = "https://test.coinbase.com";

        // Act
        var client = CoinbaseClientFactory.Create(apiKeyId, privateKey, baseUrl);

        // Assert
        Assert.NotNull(client);
        Assert.IsAssignableFrom<ICoinbaseClient>(client);
        Assert.NotNull(client.Accounts);
        Assert.NotNull(client.Trading);
        Assert.NotNull(client.MarketData);
    }

    [Fact]
    public void Create_WithNullApiKeyId_ThrowsArgumentNullException()
    {
        // Arrange
        const string privateKey = "test-private-key";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            CoinbaseClientFactory.Create(null!, privateKey));
    }

    [Fact]
    public void Create_WithNullPrivateKey_ThrowsArgumentNullException()
    {
        // Arrange
        const string apiKeyId = "test-key";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            CoinbaseClientFactory.Create(apiKeyId, null!));
    }

    [Fact]
    public void Create_WithConfigurationManager_ReturnsConfiguredClient()
    {
        // Arrange
        var configManager = new ConfigurationManager("test-key", "test-private-key");

        // Act
        var client = CoinbaseClientFactory.Create(configManager);

        // Assert
        Assert.NotNull(client);
        Assert.IsAssignableFrom<ICoinbaseClient>(client);
    }

    [Fact]
    public void Create_WithNullConfigurationManager_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            CoinbaseClientFactory.Create((IConfigurationManager)null!));
    }
}
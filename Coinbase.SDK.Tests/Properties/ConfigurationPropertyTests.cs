using Coinbase.SDK.Exceptions;
using Coinbase.SDK.Services;
using Xunit;
using FsCheck;
using FsCheck.Xunit;

namespace Coinbase.SDK.Tests.Properties;

/// <summary>
/// Property-based tests for configuration management
/// </summary>
public class ConfigurationPropertyTests : IDisposable
{
    public ConfigurationPropertyTests()
    {
        // Clear environment variables before each test to ensure clean state
        Environment.SetEnvironmentVariable("COINBASE_API_ID", null);
        Environment.SetEnvironmentVariable("COINBASE_API_PRIVATE_KEY", null);
        Environment.SetEnvironmentVariable("COINBASE_BASE_URL", null);
    }

    public void Dispose()
    {
        // Clean up environment variables after each test
        Environment.SetEnvironmentVariable("COINBASE_API_ID", null);
        Environment.SetEnvironmentVariable("COINBASE_API_PRIVATE_KEY", null);
        Environment.SetEnvironmentVariable("COINBASE_BASE_URL", null);
    }

    /// <summary>
    /// **Feature: coinbase-sdk, Property 1: Configuration initialization consistency**
    /// For any valid API key ID and private key combination, initializing the SDK should produce a properly configured client instance that can successfully authenticate
    /// **Validates: Requirements 1.1**
    /// </summary>
    [Fact]
    public void ConfigurationInitializationConsistency()
    {
        // Test with multiple valid combinations
        var testCases = new[]
        {
            ("test-api-key-1", "LS0tLS1CRUdJTiBFQyBQUklWQVRFIEtFWS0tLS0t"),
            ("api-key-123", "MHcCAQEEIFakePrivateKeyDataForTestingPurposes"),
            ("coinbase-key-abc", "dGVzdC1wcml2YXRlLWtleS1kYXRhLWZvci10ZXN0aW5n"),
            ("valid-key-xyz", "YW5vdGhlci10ZXN0LXByaXZhdGUta2V5LWRhdGE=")
        };

        foreach (var (apiKeyId, privateKey) in testCases)
        {
            // Act: Create configuration with valid credentials
            var config = new ConfigurationManager(apiKeyId, privateKey);

            // Assert: Configuration should be properly initialized
            Assert.Equal(apiKeyId, config.ApiKeyId);
            Assert.Equal(privateKey, config.PrivateKey);
            Assert.NotNull(config.BaseUrl);
            Assert.NotEmpty(config.BaseUrl);
            Assert.True(config.TokenExpiration > TimeSpan.Zero);
        }
    }

    /// <summary>
    /// Property test to verify that invalid credentials are properly rejected
    /// </summary>
    [Fact]
    public void InvalidCredentialsAreRejected()
    {
        var invalidCredentials = new[]
        {
            ("", "valid-key"),
            ("   ", "valid-key"),
            (null, "valid-key"),
            ("valid-key", ""),
            ("valid-key", "   "),
            ("valid-key", null),
            ("", ""),
            (null, null)
        };

        foreach (var (apiKeyId, privateKey) in invalidCredentials)
        {
            // Act & Assert: Try to create configuration with invalid credentials
            Assert.Throws<CoinbaseConfigurationException>(() => 
                new ConfigurationManager(apiKeyId, privateKey));
        }
    }

    /// <summary>
    /// **Feature: coinbase-sdk, Property 2: Environment variable override behavior**
    /// For any environment variable values (COINBASE_API_ID, COINBASE_API_PRIVATE_KEY, COINBASE_BASE_URL), the SDK should automatically load and use these values, overriding any defaults
    /// **Validates: Requirements 1.2, 1.5**
    /// </summary>
    [Property(MaxTest = 100)]
    public FsCheck.Property EnvironmentVariableOverrideBehavior()
    {
        var validUrls = new[] { 
            "https://api.coinbase.com", 
            "https://api.sandbox.coinbase.com", 
            "https://localhost:8080",
            "http://test.example.com",
            "https://api.test.com/v1"
        };

        var alphanumericGen = Gen.Choose(0, 61)
            .Select(i => i < 26 ? (char)('a' + i) : 
                        i < 52 ? (char)('A' + i - 26) : 
                        (char)('0' + i - 52));

        var validStringGen = Gen.NonEmptyListOf(alphanumericGen)
            .Select(chars => new string(chars.ToArray()))
            .Where(s => s.Length <= 100);

        var validPrivateKeyGen = Gen.NonEmptyListOf(alphanumericGen)
            .Select(chars => new string(chars.ToArray()))
            .Where(s => s.Length <= 200);

        return Prop.ForAll(
            validStringGen.ToArbitrary(),
            validPrivateKeyGen.ToArbitrary(),
            Gen.Elements(validUrls).ToArbitrary(),
            (apiKeyId, privateKey, baseUrl) =>
            {
                try
                {
                    // Arrange: Set environment variables
                    Environment.SetEnvironmentVariable("COINBASE_API_ID", apiKeyId);
                    Environment.SetEnvironmentVariable("COINBASE_API_PRIVATE_KEY", privateKey);
                    Environment.SetEnvironmentVariable("COINBASE_BASE_URL", baseUrl);

                    // Act: Create configuration without explicit parameters (should load from env)
                    var config = new ConfigurationManager();

                    // Assert: Configuration should load from environment variables and override defaults
                    var result = config.ApiKeyId == apiKeyId &&
                                config.PrivateKey == privateKey &&
                                config.BaseUrl == baseUrl &&
                                config.TokenExpiration > TimeSpan.Zero;

                    return result;
                }
                catch (Exception)
                {
                    // If any exception occurs during valid input, the property fails
                    return false;
                }
                finally
                {
                    // Cleanup
                    Environment.SetEnvironmentVariable("COINBASE_API_ID", null);
                    Environment.SetEnvironmentVariable("COINBASE_API_PRIVATE_KEY", null);
                    Environment.SetEnvironmentVariable("COINBASE_BASE_URL", null);
                }
            });
    }

    /// <summary>
    /// **Feature: coinbase-sdk, Property 3: Invalid credential rejection**
    /// For any invalid or malformed credential input, the SDK should throw a descriptive CoinbaseAuthenticationException without attempting API calls
    /// **Validates: Requirements 1.3**
    /// </summary>
    [Property(MaxTest = 100)]
    public FsCheck.Property InvalidCredentialRejection()
    {
        return Prop.ForAll(
            Arb.From<string>().Filter(s => string.IsNullOrWhiteSpace(s)),
            Arb.From<string>().Filter(s => !string.IsNullOrWhiteSpace(s) && s.Length > 0),
            (invalidApiKey, validPrivateKey) =>
            {
                try
                {
                    // Act: Try to create configuration with invalid API key
                    var config = new ConfigurationManager(invalidApiKey, validPrivateKey);
                    
                    // If no exception was thrown, the property fails
                    return false;
                }
                catch (CoinbaseConfigurationException)
                {
                    // Expected exception for invalid credentials
                    return true;
                }
                catch (Exception)
                {
                    // Unexpected exception type
                    return false;
                }
            })
            .And(Prop.ForAll(
                Arb.From<string>().Filter(s => !string.IsNullOrWhiteSpace(s) && s.Length > 0),
                Arb.From<string>().Filter(s => string.IsNullOrWhiteSpace(s)),
                (validApiKey, invalidPrivateKey) =>
                {
                    try
                    {
                        // Act: Try to create configuration with invalid private key
                        var config = new ConfigurationManager(validApiKey, invalidPrivateKey);
                        
                        // If no exception was thrown, the property fails
                        return false;
                    }
                    catch (CoinbaseConfigurationException)
                    {
                        // Expected exception for invalid credentials
                        return true;
                    }
                    catch (Exception)
                    {
                        // Unexpected exception type
                        return false;
                    }
                }));
    }
}
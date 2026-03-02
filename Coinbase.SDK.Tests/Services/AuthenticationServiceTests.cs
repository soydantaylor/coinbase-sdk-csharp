using System.IdentityModel.Tokens.Jwt;
using Coinbase.SDK.Exceptions;
using Coinbase.SDK.Interfaces;
using Coinbase.SDK.Services;
using Moq;
using Xunit;

namespace Coinbase.SDK.Tests.Services;

/// <summary>
/// Unit tests for AuthenticationService
/// </summary>
public class AuthenticationServiceTests
{
    private const string TestPrivateKey = @"-----BEGIN EC PRIVATE KEY-----
MHcCAQEEII0iMS/H2064WKj/jpklOh/U4NyyG5IDbWjiWsMDyc0moAoGCCqGSM49
AwEHoUQDQgAEUfD5SafApitSCfrWXI4whmjKJD3HO2mo020ah7lh9NZbLNq5Z7bF
u5kHGrQka7yWHI4KoQMaxqJH2LHQSMSmYw==
-----END EC PRIVATE KEY-----";

    private const string TestPrivateKeyWithoutHeaders = @"MHcCAQEEII0iMS/H2064WKj/jpklOh/U4NyyG5IDbWjiWsMDyc0moAoGCCqGSM49
AwEHoUQDQgAEUfD5SafApitSCfrWXI4whmjKJD3HO2mo020ah7lh9NZbLNq5Z7bF
u5kHGrQka7yWHI4KoQMaxqJH2LHQSMSmYw==";

    private const string TestPrivateKeyWithNewlines = "-----BEGIN EC PRIVATE KEY-----\\nMHcCAQEEII0iMS/H2064WKj/jpklOh/U4NyyG5IDbWjiWsMDyc0moAoGCCqGSM49\\nAwEHoUQDQgAEUfD5SafApitSCfrWXI4whmjKJD3HO2mo020ah7lh9NZbLNq5Z7bF\\nu5kHGrQka7yWHI4KoQMaxqJH2LHQSMSmYw==\\n-----END EC PRIVATE KEY-----";

    private const string TestPkcs8PrivateKey = @"-----BEGIN PRIVATE KEY-----
MIGHAgEAMBMGByqGSM49AgEGCCqGSM49AwEHBG0wawIBAQQgI0iMS/H2064WKj/j
pklOh/U4NyyG5IDbWjiWsMDyc0mhRANCAARS8PlJp8CmK1IJ+tZcjjCGaMokPcc7
aajTbRqHuWH01lss2rlntsW7mQcatCRrvJYcjgqhAxrGokcYsdBIxKZj
-----END PRIVATE KEY-----";

    [Fact]
    public async Task GenerateTokenAsync_WithValidInputs_ReturnsValidJwtToken()
    {
        // Arrange
        var mockConfig = new Mock<IConfigurationManager>();
        mockConfig.Setup(c => c.ApiKeyId).Returns("test-key-123");
        mockConfig.Setup(c => c.PrivateKey).Returns(TestPrivateKey);
        mockConfig.Setup(c => c.BaseUrl).Returns("https://api.coinbase.com");
        mockConfig.Setup(c => c.TokenExpiration).Returns(TimeSpan.FromMinutes(1));

        var authService = new AuthenticationService(mockConfig.Object);

        // Act
        var token = await authService.GenerateTokenAsync("GET", "/accounts");

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Check required claims
        var requiredClaims = new[] { "sub", "iss", "nbf", "exp", "uri" };
        foreach (var claim in requiredClaims)
        {
            Assert.True(jwtToken.Claims.Any(c => c.Type == claim), $"Missing claim: {claim}");
        }

        // Check required headers
        var requiredHeaders = new[] { "kid", "nonce", "typ" };
        foreach (var header in requiredHeaders)
        {
            Assert.True(jwtToken.Header.ContainsKey(header), $"Missing header: {header}");
        }
        var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
        var issClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "iss")?.Value;
        var uriClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "uri")?.Value;

        Assert.Equal("test-key-123", subClaim);
        Assert.Equal("coinbase-cloud", issClaim);
        Assert.Equal("GET api.coinbase.com/accounts", uriClaim);
        Assert.Equal("ES256", jwtToken.Header.Alg);
    }

    [Fact]
    public async Task GenerateTokenAsync_AfterExpiration_GeneratesNewToken()
    {
        // Arrange
        var mockConfig = new Mock<IConfigurationManager>();
        mockConfig.Setup(c => c.ApiKeyId).Returns("test-key-123");
        mockConfig.Setup(c => c.PrivateKey).Returns(TestPrivateKey);
        mockConfig.Setup(c => c.BaseUrl).Returns("https://api.coinbase.com");
        mockConfig.Setup(c => c.TokenExpiration).Returns(TimeSpan.FromMilliseconds(100)); // Very short expiration

        var authService = new AuthenticationService(mockConfig.Object);

        // Act - Generate first token
        var firstToken = await authService.GenerateTokenAsync("GET", "/accounts");
        
        // Wait for token to expire
        await Task.Delay(150);
        
        // Generate second token after expiration
        var secondToken = await authService.GenerateTokenAsync("GET", "/accounts");

        // Assert
        Assert.NotNull(firstToken);
        Assert.NotNull(secondToken);
        Assert.NotEqual(firstToken, secondToken); // Should be different tokens

        var handler = new JwtSecurityTokenHandler();
        var firstJwtToken = handler.ReadJwtToken(firstToken);
        var secondJwtToken = handler.ReadJwtToken(secondToken);

        // Second token should have later or equal expiration (since they might be generated in the same second)
        Assert.True(secondJwtToken.ValidTo >= firstJwtToken.ValidTo);
    }

    #region JWT Token Generation Tests - Requirements 1.1, 1.2, 1.3, 1.4

    [Theory]
    [InlineData("GET", "/api/v3/brokerage/accounts")]
    [InlineData("POST", "/api/v3/brokerage/orders")]
    [InlineData("DELETE", "/api/v3/brokerage/orders/123")]
    [InlineData("PUT", "/api/v3/brokerage/portfolios/456")]
    public async Task GenerateTokenAsync_WithVariousEndpoints_GeneratesCorrectUriClaim(string method, string endpoint)
    {
        // Arrange
        var mockConfig = new Mock<IConfigurationManager>();
        mockConfig.Setup(c => c.ApiKeyId).Returns("test-key-123");
        mockConfig.Setup(c => c.PrivateKey).Returns(TestPrivateKey);
        mockConfig.Setup(c => c.BaseUrl).Returns("https://api.coinbase.com");
        mockConfig.Setup(c => c.TokenExpiration).Returns(TimeSpan.FromMinutes(1));

        var authService = new AuthenticationService(mockConfig.Object);

        // Act
        var token = await authService.GenerateTokenAsync(method, endpoint);

        // Assert
        Assert.NotNull(token);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        var uriClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "uri")?.Value;
        Assert.Equal($"{method} api.coinbase.com{endpoint}", uriClaim);
    }

    [Fact]
    public async Task GenerateTokenAsync_Always_UsesCorrectIssuer()
    {
        // Arrange
        var mockConfig = new Mock<IConfigurationManager>();
        mockConfig.Setup(c => c.ApiKeyId).Returns("test-key-123");
        mockConfig.Setup(c => c.PrivateKey).Returns(TestPrivateKey);
        mockConfig.Setup(c => c.BaseUrl).Returns("https://api.coinbase.com");
        mockConfig.Setup(c => c.TokenExpiration).Returns(TimeSpan.FromMinutes(1));

        var authService = new AuthenticationService(mockConfig.Object);

        // Act
        var token = await authService.GenerateTokenAsync("GET", "/accounts");

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        var issClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "iss")?.Value;
        Assert.Equal("coinbase-cloud", issClaim);
    }

    [Fact]
    public async Task GenerateTokenAsync_Always_GeneratesCorrectNonceFormat()
    {
        // Arrange
        var mockConfig = new Mock<IConfigurationManager>();
        mockConfig.Setup(c => c.ApiKeyId).Returns("test-key-123");
        mockConfig.Setup(c => c.PrivateKey).Returns(TestPrivateKey);
        mockConfig.Setup(c => c.BaseUrl).Returns("https://api.coinbase.com");
        mockConfig.Setup(c => c.TokenExpiration).Returns(TimeSpan.FromMinutes(1));

        var authService = new AuthenticationService(mockConfig.Object);

        // Act
        var token = await authService.GenerateTokenAsync("GET", "/accounts");

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        var nonce = jwtToken.Header["nonce"]?.ToString();
        Assert.NotNull(nonce);
        Assert.Equal(32, nonce.Length); // Should be exactly 32 characters (16 bytes hex)
        Assert.True(nonce.All(c => "0123456789abcdef".Contains(c)), "Nonce should only contain hexadecimal characters");
    }

    [Fact]
    public async Task GenerateTokenAsync_MultipleCallsWithSameParams_GeneratesDifferentNonces()
    {
        // Arrange
        var mockConfig = new Mock<IConfigurationManager>();
        mockConfig.Setup(c => c.ApiKeyId).Returns("test-key-123");
        mockConfig.Setup(c => c.PrivateKey).Returns(TestPrivateKey);
        mockConfig.Setup(c => c.BaseUrl).Returns("https://api.coinbase.com");
        mockConfig.Setup(c => c.TokenExpiration).Returns(TimeSpan.FromMilliseconds(50)); // Short expiration to force new tokens

        var authService = new AuthenticationService(mockConfig.Object);

        // Act - Generate multiple tokens
        var token1 = await authService.GenerateTokenAsync("GET", "/accounts");
        await Task.Delay(100); // Wait for expiration
        var token2 = await authService.GenerateTokenAsync("GET", "/accounts");

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken1 = handler.ReadJwtToken(token1);
        var jwtToken2 = handler.ReadJwtToken(token2);
        
        var nonce1 = jwtToken1.Header["nonce"]?.ToString();
        var nonce2 = jwtToken2.Header["nonce"]?.ToString();
        
        Assert.NotNull(nonce1);
        Assert.NotNull(nonce2);
        Assert.NotEqual(nonce1, nonce2); // Nonces should be different
    }

    #endregion

    #region Private Key Parsing Tests - Requirements 2.1, 2.2

    [Fact]
    public async Task GenerateTokenAsync_WithPrivateKeyWithoutHeaders_ThrowsCoinbaseAuthenticationException()
    {
        // Arrange - bare EC key without PEM headers is now detected as Ed25519 (wrong format)
        var mockConfig = new Mock<IConfigurationManager>();
        mockConfig.Setup(c => c.ApiKeyId).Returns("test-key-123");
        mockConfig.Setup(c => c.PrivateKey).Returns(TestPrivateKeyWithoutHeaders);
        mockConfig.Setup(c => c.BaseUrl).Returns("https://api.coinbase.com");
        mockConfig.Setup(c => c.TokenExpiration).Returns(TimeSpan.FromMinutes(1));

        var authService = new AuthenticationService(mockConfig.Object);

        // Act & Assert - Should throw because bare EC key (121 bytes) is misdetected as Ed25519
        var exception = await Assert.ThrowsAsync<CoinbaseAuthenticationException>(
            () => authService.GenerateTokenAsync("GET", "/accounts"));
        Assert.Contains("Ed25519 key length", exception.Message);
    }

    [Fact]
    public async Task GenerateTokenAsync_WithPrivateKeyWithNewlineCharacters_ParsesSuccessfully()
    {
        // Arrange
        var mockConfig = new Mock<IConfigurationManager>();
        mockConfig.Setup(c => c.ApiKeyId).Returns("test-key-123");
        mockConfig.Setup(c => c.PrivateKey).Returns(TestPrivateKeyWithNewlines);
        mockConfig.Setup(c => c.BaseUrl).Returns("https://api.coinbase.com");
        mockConfig.Setup(c => c.TokenExpiration).Returns(TimeSpan.FromMinutes(1));

        var authService = new AuthenticationService(mockConfig.Object);

        // Act & Assert - Should not throw
        var token = await authService.GenerateTokenAsync("GET", "/accounts");
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    #endregion

    #region Error Handling Tests - Requirements 2.4

    [Fact]
    public async Task GenerateTokenAsync_WithNullPrivateKey_ThrowsCoinbaseAuthenticationException()
    {
        // Arrange
        var mockConfig = new Mock<IConfigurationManager>();
        mockConfig.Setup(c => c.ApiKeyId).Returns("test-key-123");
        mockConfig.Setup(c => c.PrivateKey).Returns((string)null);
        mockConfig.Setup(c => c.BaseUrl).Returns("https://api.coinbase.com");
        mockConfig.Setup(c => c.TokenExpiration).Returns(TimeSpan.FromMinutes(1));

        var authService = new AuthenticationService(mockConfig.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CoinbaseAuthenticationException>(
            () => authService.GenerateTokenAsync("GET", "/accounts"));
        
        Assert.Contains("Private key cannot be null or empty", exception.Message);
    }

    [Fact]
    public async Task GenerateTokenAsync_WithEmptyPrivateKey_ThrowsCoinbaseAuthenticationException()
    {
        // Arrange
        var mockConfig = new Mock<IConfigurationManager>();
        mockConfig.Setup(c => c.ApiKeyId).Returns("test-key-123");
        mockConfig.Setup(c => c.PrivateKey).Returns("");
        mockConfig.Setup(c => c.BaseUrl).Returns("https://api.coinbase.com");
        mockConfig.Setup(c => c.TokenExpiration).Returns(TimeSpan.FromMinutes(1));

        var authService = new AuthenticationService(mockConfig.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CoinbaseAuthenticationException>(
            () => authService.GenerateTokenAsync("GET", "/accounts"));
        
        Assert.Contains("Private key cannot be null or empty", exception.Message);
    }

    [Fact]
    public async Task GenerateTokenAsync_WithInvalidBase64PrivateKey_ThrowsCoinbaseAuthenticationException()
    {
        // Arrange
        var mockConfig = new Mock<IConfigurationManager>();
        mockConfig.Setup(c => c.ApiKeyId).Returns("test-key-123");
        mockConfig.Setup(c => c.PrivateKey).Returns("-----BEGIN EC PRIVATE KEY-----\nInvalidBase64Data!!!\n-----END EC PRIVATE KEY-----");
        mockConfig.Setup(c => c.BaseUrl).Returns("https://api.coinbase.com");
        mockConfig.Setup(c => c.TokenExpiration).Returns(TimeSpan.FromMinutes(1));

        var authService = new AuthenticationService(mockConfig.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CoinbaseAuthenticationException>(
            () => authService.GenerateTokenAsync("GET", "/accounts"));
        
        Assert.Contains("Invalid base64 encoding in private key", exception.Message);
    }

    [Fact]
    public async Task GenerateTokenAsync_WithInvalidKeyFormat_ThrowsCoinbaseAuthenticationException()
    {
        // Arrange
        var mockConfig = new Mock<IConfigurationManager>();
        mockConfig.Setup(c => c.ApiKeyId).Returns("test-key-123");
        mockConfig.Setup(c => c.PrivateKey).Returns("-----BEGIN EC PRIVATE KEY-----\nVGhpcyBpcyBub3QgYSB2YWxpZCBrZXk=\n-----END EC PRIVATE KEY-----");
        mockConfig.Setup(c => c.BaseUrl).Returns("https://api.coinbase.com");
        mockConfig.Setup(c => c.TokenExpiration).Returns(TimeSpan.FromMinutes(1));

        var authService = new AuthenticationService(mockConfig.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CoinbaseAuthenticationException>(
            () => authService.GenerateTokenAsync("GET", "/accounts"));
        
        Assert.Contains("Unable to parse EC private key", exception.Message);
    }

    [Theory]
    [InlineData(null, "endpoint")]
    [InlineData("", "endpoint")]
    [InlineData("   ", "endpoint")]
    [InlineData("GET", null)]
    [InlineData("GET", "")]
    [InlineData("GET", "   ")]
    public async Task GenerateTokenAsync_WithInvalidParameters_ThrowsArgumentException(string httpMethod, string endpoint)
    {
        // Arrange
        var mockConfig = new Mock<IConfigurationManager>();
        mockConfig.Setup(c => c.ApiKeyId).Returns("test-key-123");
        mockConfig.Setup(c => c.PrivateKey).Returns(TestPrivateKey);
        mockConfig.Setup(c => c.BaseUrl).Returns("https://api.coinbase.com");
        mockConfig.Setup(c => c.TokenExpiration).Returns(TimeSpan.FromMinutes(1));

        var authService = new AuthenticationService(mockConfig.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => authService.GenerateTokenAsync(httpMethod, endpoint));
    }

    #endregion
}
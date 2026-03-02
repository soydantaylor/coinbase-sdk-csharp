using Coinbase.SDK.Utilities;
using Xunit;

namespace Coinbase.SDK.Tests.Utilities;

public class PrivateKeyHelperTests
{
    [Fact]
    public void NormalizePrivateKey_WithLiteralNewlines_ReplacesWithActualNewlines()
    {
        // Arrange
        var privateKeyWithLiterals = "-----BEGIN PRIVATE KEY-----\\nMIGEAgEAMBAGByqGSM49AgEGBSuBBAAKBG0wawIBAQQg\\n-----END PRIVATE KEY-----";
        
        // Act
        var normalized = PrivateKeyHelper.NormalizePrivateKey(privateKeyWithLiterals);
        
        // Assert
        Assert.Contains("\n", normalized);
        Assert.DoesNotContain("\\n", normalized);
    }

    [Fact]
    public void NormalizePrivateKey_WithActualNewlines_LeavesUnchanged()
    {
        // Arrange
        var privateKeyWithNewlines = "-----BEGIN PRIVATE KEY-----\nMIGEAgEAMBAGByqGSM49AgEGBSuBBAAKBG0wawIBAQQg\n-----END PRIVATE KEY-----";
        
        // Act
        var normalized = PrivateKeyHelper.NormalizePrivateKey(privateKeyWithNewlines);
        
        // Assert
        Assert.Equal(privateKeyWithNewlines, normalized);
    }

    [Fact]
    public void ExtractPrivateKeyFromJson_WithValidJson_ReturnsNormalizedKey()
    {
        // Arrange
        var json = """
        {
            "name": "test-api-key",
            "privateKey": "-----BEGIN PRIVATE KEY-----\\nMIGEAgEAMBAGByqGSM49AgEGBSuBBAAKBG0wawIBAQQg\\n-----END PRIVATE KEY-----"
        }
        """;
        
        // Act
        var privateKey = PrivateKeyHelper.ExtractPrivateKeyFromJson(json);
        
        // Assert
        Assert.Contains("\n", privateKey);
        Assert.DoesNotContain("\\n", privateKey);
        Assert.StartsWith("-----BEGIN PRIVATE KEY-----\n", privateKey);
        Assert.EndsWith("\n-----END PRIVATE KEY-----", privateKey);
    }

    [Fact]
    public void ExtractApiKeyIdFromJson_WithValidJson_ReturnsApiKeyId()
    {
        // Arrange
        var json = """
        {
            "name": "test-api-key-id",
            "privateKey": "-----BEGIN PRIVATE KEY-----\\nMIGEAgEAMBAGByqGSM49AgEGBSuBBAAKBG0wawIBAQQg\\n-----END PRIVATE KEY-----"
        }
        """;
        
        // Act
        var apiKeyId = PrivateKeyHelper.ExtractApiKeyIdFromJson(json);
        
        // Assert
        Assert.Equal("test-api-key-id", apiKeyId);
    }

    [Fact]
    public void ExtractPrivateKeyFromJson_WithMissingPrivateKey_ThrowsException()
    {
        // Arrange
        var json = """
        {
            "name": "test-api-key"
        }
        """;
        
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            PrivateKeyHelper.ExtractPrivateKeyFromJson(json));
        Assert.Contains("Private key not found", exception.Message);
    }

    [Fact]
    public void ExtractApiKeyIdFromJson_WithMissingName_ThrowsException()
    {
        // Arrange
        var json = """
        {
            "privateKey": "-----BEGIN PRIVATE KEY-----\\ntest\\n-----END PRIVATE KEY-----"
        }
        """;
        
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            PrivateKeyHelper.ExtractApiKeyIdFromJson(json));
        Assert.Contains("API key ID not found", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void NormalizePrivateKey_WithInvalidInput_ThrowsArgumentException(string? input)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => PrivateKeyHelper.NormalizePrivateKey(input!));
    }

    [Fact]
    public void ExtractPrivateKeyFromJson_WithInvalidJson_ThrowsArgumentException()
    {
        // Arrange
        var invalidJson = "{ invalid json }";
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            PrivateKeyHelper.ExtractPrivateKeyFromJson(invalidJson));
    }
}
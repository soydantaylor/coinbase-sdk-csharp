using System.Text.Json;

namespace Coinbase.SDK.Utilities;

/// <summary>
/// Helper utility for handling Coinbase private keys in various formats
/// </summary>
public static class PrivateKeyHelper
{
    /// <summary>
    /// Normalizes a private key from Coinbase JSON format to proper PEM format
    /// </summary>
    /// <param name="privateKey">The private key string (may contain \n literals)</param>
    /// <returns>Normalized private key with proper newlines</returns>
    public static string NormalizePrivateKey(string privateKey)
    {
        if (string.IsNullOrWhiteSpace(privateKey))
            throw new ArgumentException("Private key cannot be null or empty", nameof(privateKey));

        // Handle Coinbase JSON format where \n are literal string characters
        return privateKey.Replace("\\n", "\n");
    }

    /// <summary>
    /// Extracts private key from Coinbase JSON credentials file
    /// </summary>
    /// <param name="jsonContent">The JSON content from Coinbase credentials file</param>
    /// <returns>The normalized private key</returns>
    public static string ExtractPrivateKeyFromJson(string jsonContent)
    {
        if (string.IsNullOrWhiteSpace(jsonContent))
            throw new ArgumentException("JSON content cannot be null or empty", nameof(jsonContent));

        try
        {
            using var document = JsonDocument.Parse(jsonContent);
            var root = document.RootElement;

            if (root.TryGetProperty("privateKey", out var privateKeyElement))
            {
                var privateKey = privateKeyElement.GetString();
                if (string.IsNullOrEmpty(privateKey))
                    throw new InvalidOperationException("Private key is empty in JSON");

                return NormalizePrivateKey(privateKey);
            }

            throw new InvalidOperationException("Private key not found in JSON. Expected 'privateKey' property.");
        }
        catch (JsonException ex)
        {
            throw new ArgumentException($"Invalid JSON format: {ex.Message}", nameof(jsonContent), ex);
        }
    }

    /// <summary>
    /// Extracts API key ID from Coinbase JSON credentials file
    /// </summary>
    /// <param name="jsonContent">The JSON content from Coinbase credentials file</param>
    /// <returns>The API key ID</returns>
    public static string ExtractApiKeyIdFromJson(string jsonContent)
    {
        if (string.IsNullOrWhiteSpace(jsonContent))
            throw new ArgumentException("JSON content cannot be null or empty", nameof(jsonContent));

        try
        {
            using var document = JsonDocument.Parse(jsonContent);
            var root = document.RootElement;

            // Support both legacy format ("name": "organizations/.../apiKeys/...")
            // and new CDP format ("id": "uuid")
            if (root.TryGetProperty("name", out var nameElement))
            {
                var apiKeyId = nameElement.GetString();
                if (!string.IsNullOrEmpty(apiKeyId))
                    return apiKeyId;
            }

            if (root.TryGetProperty("id", out var idElement))
            {
                var apiKeyId = idElement.GetString();
                if (!string.IsNullOrEmpty(apiKeyId))
                    return apiKeyId;
            }

            throw new InvalidOperationException("API key ID not found in JSON. Expected 'name' or 'id' property.");
        }
        catch (JsonException ex)
        {
            throw new ArgumentException($"Invalid JSON format: {ex.Message}", nameof(jsonContent), ex);
        }
    }

    /// <summary>
    /// Loads credentials from a Coinbase JSON file
    /// </summary>
    /// <param name="filePath">Path to the Coinbase JSON credentials file</param>
    /// <returns>A tuple containing (ApiKeyId, PrivateKey)</returns>
    public static async Task<(string ApiKeyId, string PrivateKey)> LoadCredentialsFromFileAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Credentials file not found: {filePath}");

        try
        {
            var jsonContent = await File.ReadAllTextAsync(filePath);
            var apiKeyId = ExtractApiKeyIdFromJson(jsonContent);
            var privateKey = ExtractPrivateKeyFromJson(jsonContent);

            return (apiKeyId, privateKey);
        }
        catch (Exception ex) when (!(ex is ArgumentException || ex is InvalidOperationException || ex is FileNotFoundException))
        {
            throw new InvalidOperationException($"Failed to load credentials from file: {ex.Message}", ex);
        }
    }
}
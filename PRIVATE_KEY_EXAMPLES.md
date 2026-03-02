# Private Key Handling Examples

This document shows how to use Coinbase private keys in different formats with the SDK.

## Problem

When you download API credentials from Coinbase, the private key comes in JSON format with `\n` as literal string characters instead of actual newlines:

```json
{
  "name": "your-api-key-id",
  "privateKey": "-----BEGIN PRIVATE KEY-----\nMIGEAgEAMBAGByqGSM49AgEGBSuBBAAKBG0wawIBAQQg...\n-----END PRIVATE KEY-----"
}
```

If you copy this directly to an environment variable, you'll get a Base64 parsing error.

## Solutions

### 1. Using Environment Variables (Automatic Handling)

The SDK now automatically handles the `\n` conversion:

```bash
# This works now - SDK converts \n to actual newlines
export COINBASE_API_PRIVATE_KEY="-----BEGIN PRIVATE KEY-----\nMIGEAgEAMBAGByqGSM49AgEGBSuBBAAKBG0wawIBAQQg...\n-----END PRIVATE KEY-----"

# Then use normally
var client = CoinbaseClientFactory.CreateFromEnvironment();
```

### 2. Using JSON File Directly

```csharp
// Load directly from Coinbase JSON file
var client = await CoinbaseClientFactory.CreateFromJsonFileAsync("coinbase_cloud_api_key.json");

// Or from JSON content
var jsonContent = File.ReadAllText("coinbase_cloud_api_key.json");
var client = CoinbaseClientFactory.CreateFromJson(jsonContent);
```

### 3. Manual Conversion

```csharp
using Coinbase.SDK.Utilities;

// If you need to convert manually
var privateKeyWithLiterals = "-----BEGIN PRIVATE KEY-----\\nMIGE...\\n-----END PRIVATE KEY-----";
var normalizedKey = PrivateKeyHelper.NormalizePrivateKey(privateKeyWithLiterals);

var client = CoinbaseClientFactory.Create("your-api-key-id", normalizedKey);
```

### 4. Extract from JSON Manually

```csharp
using Coinbase.SDK.Utilities;

var jsonContent = """
{
    "name": "your-api-key-id",
    "privateKey": "-----BEGIN PRIVATE KEY-----\\nMIGE...\\n-----END PRIVATE KEY-----"
}
""";

var apiKeyId = PrivateKeyHelper.ExtractApiKeyIdFromJson(jsonContent);
var privateKey = PrivateKeyHelper.ExtractPrivateKeyFromJson(jsonContent);

var client = CoinbaseClientFactory.Create(apiKeyId, privateKey);
```

## Complete Example

```csharp
using Coinbase.SDK;
using Coinbase.SDK.Models;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            // Method 1: From JSON file (recommended)
            var client = await CoinbaseClientFactory.CreateFromJsonFileAsync("coinbase_cloud_api_key.json");
            
            // Method 2: From environment variables (also works with \n literals now)
            // var client = CoinbaseClientFactory.CreateFromEnvironment();
            
            // Test the connection
            var accounts = await client.Accounts.GetAccountsAsync();
            Console.WriteLine($"Successfully connected! Found {accounts.Count()} accounts.");
            
            foreach (var account in accounts.Take(3))
            {
                Console.WriteLine($"- {account.Name}: {account.AvailableBalance.Value} {account.Currency}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
```

## Troubleshooting

If you still get Base64 errors:

1. **Check the private key format**: Make sure it starts with `-----BEGIN PRIVATE KEY-----` and ends with `-----END PRIVATE KEY-----`

2. **Verify JSON structure**: The JSON should have `name` and `privateKey` fields

3. **Use the helper methods**: The `PrivateKeyHelper` class can help debug format issues

4. **Test with a simple example**:
   ```csharp
   var testKey = "-----BEGIN PRIVATE KEY-----\\ntest\\n-----END PRIVATE KEY-----";
   var normalized = PrivateKeyHelper.NormalizePrivateKey(testKey);
   Console.WriteLine(normalized); // Should show actual newlines
   ```

The SDK now handles all common Coinbase private key formats automatically!
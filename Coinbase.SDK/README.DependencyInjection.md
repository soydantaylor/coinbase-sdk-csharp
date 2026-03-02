# Coinbase SDK Dependency Injection Guide

This document explains how to use the Coinbase SDK with dependency injection in .NET applications.

## Quick Start

### 1. Using Extension Methods

The simplest way to register the SDK services:

```csharp
using Coinbase.SDK.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Register SDK services with environment variables
builder.Services.AddCoinbaseSDK();

// Or with explicit credentials
builder.Services.AddCoinbaseSDK("your-api-key", "your-private-key");

// Or with configuration
builder.Services.AddCoinbaseSDK(options =>
{
    options.ApiKeyId = "your-api-key";
    options.PrivateKey = "your-private-key";
    options.BaseUrl = "https://api.coinbase.com"; // Optional
    options.TokenExpirationMinutes = 2; // Optional, default is 1
});

var host = builder.Build();
```

### 2. Using Configuration

You can bind SDK options from configuration:

```json
{
  "Coinbase": {
    "ApiKeyId": "your-api-key",
    "PrivateKey": "your-private-key",
    "BaseUrl": "https://api.coinbase.com",
    "TokenExpirationMinutes": 1
  }
}
```

```csharp
builder.Services.Configure<CoinbaseOptions>(
    builder.Configuration.GetSection(CoinbaseOptions.SectionName));
builder.Services.AddCoinbaseSDK();
```

### 3. Consuming Services

Inject the main client or individual services:

```csharp
public class TradingController : ControllerBase
{
    private readonly ICoinbaseClient _coinbaseClient;
    
    public TradingController(ICoinbaseClient coinbaseClient)
    {
        _coinbaseClient = coinbaseClient;
    }
    
    public async Task<IActionResult> GetAccounts()
    {
        var accounts = await _coinbaseClient.Accounts.GetAccountsAsync();
        return Ok(accounts);
    }
}

// Or inject individual services
public class AccountService
{
    private readonly IAccountService _accountService;
    
    public AccountService(IAccountService accountService)
    {
        _accountService = accountService;
    }
}
```

## Service Lifetimes

The SDK registers services with the following lifetimes:

- **Singleton**: `IConfigurationManager`, `IAuthenticationService`, `HttpClient`
- **Scoped**: `IApiClient`, `IAccountService`, `ITradingService`, `IMarketDataService`, `ICoinbaseClient`

This ensures efficient resource usage while maintaining thread safety.

## Factory Pattern (Without DI)

If you prefer not to use dependency injection:

```csharp
using Coinbase.SDK;

// Create with explicit credentials
var client = CoinbaseClientFactory.Create("api-key", "private-key");

// Create from environment variables
var client = CoinbaseClientFactory.CreateFromEnvironment();

// Create with custom services
var configManager = new ConfigurationManager("api-key", "private-key");
var client = CoinbaseClientFactory.Create(configManager);
```

## Environment Variables

The SDK automatically loads configuration from these environment variables:

- `COINBASE_API_ID`: Your API key ID
- `COINBASE_API_PRIVATE_KEY`: Your private key
- `COINBASE_BASE_URL`: Optional base URL override

## Advanced Configuration

For advanced scenarios, you can register services manually:

```csharp
services.Configure<CoinbaseOptions>(options =>
{
    options.ApiKeyId = "your-key";
    options.PrivateKey = "your-private-key";
    options.TokenExpirationMinutes = 5; // Longer expiration
});

services.AddSingleton<IConfigurationManager>(provider =>
{
    var options = provider.GetRequiredService<IOptions<CoinbaseOptions>>().Value;
    return new CustomConfigurationManager(options);
});

// Register other services as needed...
```
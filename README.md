# Coinbase Advanced Trade API SDK for .NET

[![NuGet Version](https://img.shields.io/nuget/v/Coinbase.SDK.svg)](https://www.nuget.org/packages/Coinbase.SDK/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Coinbase.SDK.svg)](https://www.nuget.org/packages/Coinbase.SDK/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A production-ready C# SDK for the Coinbase Advanced Trade API. This SDK provides comprehensive support for authentication, account management, trading operations, and market data access with full async/await support and dependency injection integration.

## Features

- **JWT Authentication**: Automatic ES256 JWT token generation and management
- **Account Management**: Retrieve and manage Coinbase accounts with filtering capabilities
- **Trading Operations**: Place market and limit orders, cancel orders, and retrieve order status
- **Transfer Operations**: ACH deposits and withdrawals, payment method management, transfer history
- **Market Data**: Access real-time and historical market data, order books, and candle data
- **Async/Await Support**: Full asynchronous operation support with cancellation tokens
- **Dependency Injection**: Built-in support for Microsoft.Extensions.DependencyInjection
- **Thread-Safe**: Safe for concurrent operations
- **Comprehensive Error Handling**: Detailed exception types with proper error context
- **Strongly Typed**: Full IntelliSense support with comprehensive XML documentation
- **Property-Based Testing**: Extensively tested with 19 correctness properties

## Installation

### From Private GitHub Package Registry

This package is published as a private package to GitHub Package Registry. To install it:

1. Create a `NuGet.Config` file in your project root:
   ```xml
   <?xml version="1.0" encoding="utf-8"?>
   <configuration>
     <packageSources>
       <add key="github" value="https://nuget.pkg.github.com/soydantaylor/index.json" />
       <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
     </packageSources>
     <packageSourceCredentials>
       <github>
         <add key="Username" value="soydantaylor" />
         <add key="ClearTextPassword" value="YOUR_GITHUB_TOKEN" />
       </github>
     </packageSourceCredentials>
   </configuration>
   ```

2. Install the package:
   ```bash
   dotnet add package Coinbase.SDK --source github
   ```

> **Note**: You need a GitHub Personal Access Token with `read:packages` permission to install this private package. See [GITHUB_PACKAGES.md](GITHUB_PACKAGES.md) for detailed setup instructions.

## Quick Start

### Using Environment Variables (Recommended)

Set your Coinbase API credentials as environment variables:

```bash
export COINBASE_API_ID="your-api-key-id"
export COINBASE_API_PRIVATE_KEY="your-private-key"
```

**Note**: If your private key comes from Coinbase's JSON format with `\n` characters, you can use it directly - the SDK will handle the conversion automatically.

Then create a client:

```csharp
using Coinbase.SDK;
using Coinbase.SDK.Models;

// Create client from environment variables
var client = CoinbaseClientFactory.CreateFromEnvironment();

// Get all accounts
var accounts = await client.Accounts.GetAccountsAsync();
foreach (var account in accounts)
{
    Console.WriteLine($"Account: {account.Name} - {account.Currency} - {account.AvailableBalance.Value}");
}

// Initiate an ACH deposit (example)
var paymentMethods = await client.Transfers.GetPaymentMethodsAsync();
var achMethod = paymentMethods.FirstOrDefault(pm => pm.Type == "ach_bank_account");
if (achMethod != null)
{
    var deposit = await client.Transfers.DepositAsync(new DepositRequest
    {
        Amount = 100m,
        Currency = "USD", 
        PaymentMethodId = achMethod.Id
    });
    Console.WriteLine($"Deposit initiated: {deposit.Id} - Status: {deposit.Status}");
}
```

### Using Coinbase JSON Credentials File

If you have a Coinbase JSON credentials file (downloaded from Coinbase Developer Platform):

```csharp
using Coinbase.SDK;

// Load from JSON file
var client = await CoinbaseClientFactory.CreateFromJsonFileAsync("path/to/coinbase_cloud_api_key.json");

// Or from JSON content
var jsonContent = File.ReadAllText("coinbase_cloud_api_key.json");
var client = CoinbaseClientFactory.CreateFromJson(jsonContent);
```

### Using Explicit Credentials

```csharp
using Coinbase.SDK;
using Coinbase.SDK.Models;

var client = CoinbaseClientFactory.Create("your-api-key-id", "your-private-key");

// Place a market order
var orderRequest = new MarketOrderRequest
{
    ProductId = "BTC-USD",
    Side = OrderSide.Buy,
    Size = 0.001m
};

var order = await client.Trading.PlaceMarketOrderAsync(orderRequest);
Console.WriteLine($"Order placed: {order.OrderId}");
```

### Handling Private Key Formats

The SDK automatically handles different private key formats:

1. **Environment Variable**: Can contain `\n` literal strings (from Coinbase JSON)
2. **Direct PEM**: Properly formatted PEM with actual newlines
3. **JSON Format**: Automatically extracts and normalizes from Coinbase JSON

```csharp
// All of these work automatically:

// From environment (with \n literals)
export COINBASE_API_PRIVATE_KEY="-----BEGIN PRIVATE KEY-----\nMIGE...your-key...\n-----END PRIVATE KEY-----"

// From JSON file (SDK handles extraction)
var client = await CoinbaseClientFactory.CreateFromJsonFileAsync("credentials.json");

// Direct PEM (with actual newlines)
var privateKey = @"-----BEGIN PRIVATE KEY-----
MIGEAgEAMBAGByqGSM49AgEGBSuBBAAKBG0wawIBAQQg...
-----END PRIVATE KEY-----";
var client = CoinbaseClientFactory.Create("api-key-id", privateKey);
```

### Using Dependency Injection

```csharp
using Coinbase.SDK.Extensions;
using Coinbase.SDK.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Register Coinbase SDK services
builder.Services.AddCoinbaseSDK(options =>
{
    options.ApiKeyId = "your-api-key-id";
    options.PrivateKey = "your-private-key";
});

var host = builder.Build();

// Use the client
var client = host.Services.GetRequiredService<ICoinbaseClient>();
var accounts = await client.Accounts.GetAccountsAsync();
```

## Complete Usage Examples

### Account Management

#### Get All Accounts
```csharp
using Coinbase.SDK;
using Coinbase.SDK.Models;

var client = CoinbaseClientFactory.CreateFromEnvironment();

// Get all accounts
var accounts = await client.Accounts.GetAccountsAsync();

foreach (var account in accounts)
{
    Console.WriteLine($"Account ID: {account.Uuid}");
    Console.WriteLine($"Name: {account.Name}");
    Console.WriteLine($"Currency: {account.Currency}");
    Console.WriteLine($"Available Balance: {account.AvailableBalance.Value} {account.AvailableBalance.Currency}");
    Console.WriteLine($"Hold: {account.Hold.Value} {account.Hold.Currency}");
    Console.WriteLine($"Active: {account.Active}");
    Console.WriteLine($"Type: {account.Type}");
    Console.WriteLine("---");
}
```

#### Get Specific Account
```csharp
// Get account by ID
var account = await client.Accounts.GetAccountAsync("account-uuid-here");
Console.WriteLine($"Found account: {account.Name} with balance {account.AvailableBalance.Value}");

// Get account by name
var btcAccount = await client.Accounts.GetAccountByNameAsync("BTC Wallet");
Console.WriteLine($"BTC Account balance: {btcAccount.AvailableBalance.Value}");
```

### Trading Operations

#### Place Market Orders
```csharp
using Coinbase.SDK;
using Coinbase.SDK.Models;

var client = CoinbaseClientFactory.CreateFromEnvironment();

// Buy Bitcoin with market order
var buyOrder = await client.Trading.PlaceMarketOrderAsync(new MarketOrderRequest
{
    ProductId = "BTC-USD",
    Side = OrderSide.Buy,
    Size = 0.001m // Buy 0.001 BTC
});

Console.WriteLine($"Market buy order placed: {buyOrder.OrderId}");
Console.WriteLine($"Status: {buyOrder.Status}");
Console.WriteLine($"Filled: {buyOrder.FilledSize} / {buyOrder.Size}");

// Sell Ethereum with market order
var sellOrder = await client.Trading.PlaceMarketOrderAsync(new MarketOrderRequest
{
    ProductId = "ETH-USD",
    Side = OrderSide.Sell,
    Size = 0.1m // Sell 0.1 ETH
});

Console.WriteLine($"Market sell order placed: {sellOrder.OrderId}");
```

#### Place Limit Orders
```csharp
// Place a limit buy order
var limitBuyOrder = await client.Trading.PlaceLimitOrderAsync(new LimitOrderRequest
{
    ProductId = "BTC-USD",
    Side = OrderSide.Buy,
    Size = 0.001m,
    Price = 45000m, // Buy at $45,000
    TimeInForce = TimeInForce.GTC // Good Till Cancelled
});

Console.WriteLine($"Limit buy order placed: {limitBuyOrder.OrderId} at ${limitBuyOrder.Price}");

// Place a limit sell order with different time in force
var limitSellOrder = await client.Trading.PlaceLimitOrderAsync(new LimitOrderRequest
{
    ProductId = "ETH-USD",
    Side = OrderSide.Sell,
    Size = 0.5m,
    Price = 3500m, // Sell at $3,500
    TimeInForce = TimeInForce.IOC // Immediate Or Cancel
});

Console.WriteLine($"Limit sell order placed: {limitSellOrder.OrderId}");
```

#### Order Management
```csharp
// Get specific order
var order = await client.Trading.GetOrderAsync("order-id-here");
Console.WriteLine($"Order {order.OrderId}:");
Console.WriteLine($"  Product: {order.ProductId}");
Console.WriteLine($"  Side: {order.Side}");
Console.WriteLine($"  Type: {order.Type}");
Console.WriteLine($"  Status: {order.Status}");
Console.WriteLine($"  Size: {order.Size}");
Console.WriteLine($"  Price: {order.Price}");
Console.WriteLine($"  Filled: {order.FilledSize} ({order.FilledValue:C})");
Console.WriteLine($"  Created: {order.CreatedTime}");

// Cancel an order
var cancelled = await client.Trading.CancelOrderAsync("order-id-here");
if (cancelled)
{
    Console.WriteLine("Order cancelled successfully");
}
```

#### Get Orders with Filtering
```csharp
// Get all orders
var allOrders = await client.Trading.GetOrdersAsync();
Console.WriteLine($"Total orders: {allOrders.Count()}");

// Get orders with filters
var btcOrders = await client.Trading.GetOrdersAsync(new OrderFilter
{
    ProductId = "BTC-USD",
    Status = OrderStatus.Open,
    StartDate = DateTime.UtcNow.AddDays(-7)
});

Console.WriteLine($"Open BTC orders from last 7 days: {btcOrders.Count()}");

// Get filled orders for a specific product
var filledEthOrders = await client.Trading.GetOrdersAsync(new OrderFilter
{
    ProductId = "ETH-USD",
    Status = OrderStatus.Filled,
    Side = OrderSide.Buy
});

Console.WriteLine($"Filled ETH buy orders: {filledEthOrders.Count()}");
```

### Market Data Operations

#### Get Products and Product Information
```csharp
using Coinbase.SDK;
using Coinbase.SDK.Models;

var client = CoinbaseClientFactory.CreateFromEnvironment();

// Get all available products
var products = await client.MarketData.GetProductsAsync();
Console.WriteLine($"Available products: {products.Count()}");

foreach (var product in products.Take(5)) // Show first 5
{
    Console.WriteLine($"Product: {product.Id} ({product.DisplayName})");
    Console.WriteLine($"  Base: {product.BaseCurrency}");
    Console.WriteLine($"  Quote: {product.QuoteCurrency}");
    Console.WriteLine($"  Min Size: {product.BaseMinSize}");
    Console.WriteLine($"  Max Size: {product.BaseMaxSize}");
    Console.WriteLine($"  Quote Increment: {product.QuoteIncrement}");
    Console.WriteLine($"  Status: {product.Status}");
    Console.WriteLine("---");
}

// Get specific product details
var btcProduct = await client.MarketData.GetProductAsync("BTC-USD");
Console.WriteLine($"BTC-USD Product Details:");
Console.WriteLine($"  Display Name: {btcProduct.DisplayName}");
Console.WriteLine($"  Min Order Size: {btcProduct.BaseMinSize} BTC");
Console.WriteLine($"  Max Order Size: {btcProduct.BaseMaxSize} BTC");
Console.WriteLine($"  Price Increment: ${btcProduct.QuoteIncrement}");
```

#### Get Order Book Data
```csharp
// Get Level 2 order book (default)
var orderBook = await client.MarketData.GetProductBookAsync("BTC-USD");
Console.WriteLine($"BTC-USD Order Book at {orderBook.Time}:");

Console.WriteLine("Top 5 Bids:");
foreach (var bid in orderBook.Bids.Take(5))
{
    Console.WriteLine($"  ${bid.Price:F2} - {bid.Size:F8} BTC ({bid.NumOrders} orders)");
}

Console.WriteLine("Top 5 Asks:");
foreach (var ask in orderBook.Asks.Take(5))
{
    Console.WriteLine($"  ${ask.Price:F2} - {ask.Size:F8} BTC ({ask.NumOrders} orders)");
}

// Get Level 1 order book (best bid/ask only)
var level1Book = await client.MarketData.GetProductBookAsync("ETH-USD", level: 1);
var bestBid = level1Book.Bids.FirstOrDefault();
var bestAsk = level1Book.Asks.FirstOrDefault();

if (bestBid != null && bestAsk != null)
{
    var spread = bestAsk.Price - bestBid.Price;
    Console.WriteLine($"ETH-USD Spread: ${spread:F2} (${bestBid.Price:F2} - ${bestAsk.Price:F2})");
}
```

#### Get Historical Candle Data
```csharp
// Get hourly candles for the last 24 hours
var hourlyCandles = await client.MarketData.GetCandlesAsync(
    "BTC-USD",
    DateTime.UtcNow.AddDays(-1),
    DateTime.UtcNow,
    Granularity.OneHour
);

Console.WriteLine("BTC-USD Hourly Candles (Last 24 hours):");
foreach (var candle in hourlyCandles.OrderBy(c => c.Start))
{
    Console.WriteLine($"{candle.Start:yyyy-MM-dd HH:mm}: " +
                     $"O=${candle.Open:F2} H=${candle.High:F2} " +
                     $"L=${candle.Low:F2} C=${candle.Close:F2} " +
                     $"V={candle.Volume:F4}");
}

// Get daily candles for the last month
var dailyCandles = await client.MarketData.GetCandlesAsync(
    "ETH-USD",
    DateTime.UtcNow.AddDays(-30),
    DateTime.UtcNow,
    Granularity.OneDay
);

Console.WriteLine($"\nETH-USD Daily Candles (Last 30 days): {dailyCandles.Count()} candles");

// Calculate some basic statistics
var prices = dailyCandles.Select(c => c.Close).ToList();
var avgPrice = prices.Average();
var maxPrice = prices.Max();
var minPrice = prices.Min();

Console.WriteLine($"Average Close: ${avgPrice:F2}");
Console.WriteLine($"Highest Close: ${maxPrice:F2}");
Console.WriteLine($"Lowest Close: ${minPrice:F2}");

// Get 5-minute candles for detailed analysis
var fiveMinCandles = await client.MarketData.GetCandlesAsync(
    "BTC-USD",
    DateTime.UtcNow.AddHours(-2),
    DateTime.UtcNow,
    Granularity.FiveMinutes
);

Console.WriteLine($"\nBTC-USD 5-minute candles (Last 2 hours): {fiveMinCandles.Count()} candles");
```

### Transfer Operations (ACH Deposits and Withdrawals)

#### Get Payment Methods
```csharp
using Coinbase.SDK;
using Coinbase.SDK.Models;

var client = CoinbaseClientFactory.CreateFromEnvironment();

// Get all available payment methods
var paymentMethods = await client.Transfers.GetPaymentMethodsAsync();

Console.WriteLine("Available Payment Methods:");
foreach (var method in paymentMethods)
{
    Console.WriteLine($"Payment Method ID: {method.Id}");
    Console.WriteLine($"  Name: {method.Name}");
    Console.WriteLine($"  Type: {method.Type}");
    Console.WriteLine($"  Currency: {method.Currency}");
    Console.WriteLine($"  Primary Buy: {method.PrimaryBuy}");
    Console.WriteLine($"  Primary Sell: {method.PrimarySell}");
    Console.WriteLine($"  Instant Buy: {method.InstantBuy}");
    Console.WriteLine($"  Instant Sell: {method.InstantSell}");
    
    if (method.Limits != null)
    {
        Console.WriteLine("  Limits:");
        foreach (var depositLimit in method.Limits.Deposit)
        {
            Console.WriteLine($"    Deposit ({depositLimit.PeriodInDays} days): " +
                            $"${depositLimit.Total.Value:F2} total, " +
                            $"${depositLimit.Remaining.Value:F2} remaining");
        }
    }
    Console.WriteLine("---");
}

// Get specific payment method details
var achMethod = paymentMethods.FirstOrDefault(pm => pm.Type == "ach_bank_account");
if (achMethod != null)
{
    var methodDetails = await client.Transfers.GetPaymentMethodAsync(achMethod.Id);
    Console.WriteLine($"ACH Method Details: {methodDetails.Name}");
}
```

#### Initiate ACH Deposit
```csharp
// Find your ACH bank account payment method
var paymentMethods = await client.Transfers.GetPaymentMethodsAsync();
var achPaymentMethod = paymentMethods.FirstOrDefault(pm => pm.Type == "ach_bank_account");

if (achPaymentMethod != null)
{
    // Initiate a $100 USD deposit from your bank account
    var depositRequest = new DepositRequest
    {
        Amount = 100.00m,
        Currency = "USD",
        PaymentMethodId = achPaymentMethod.Id,
        Commit = true // Set to false to create a pending deposit that needs to be committed later
    };

    var deposit = await client.Transfers.DepositAsync(depositRequest);
    
    Console.WriteLine($"Deposit initiated: {deposit.Id}");
    Console.WriteLine($"Amount: ${deposit.Amount.Value} {deposit.Amount.Currency}");
    Console.WriteLine($"Status: {deposit.Status}");
    Console.WriteLine($"Method: {deposit.Method}");
    Console.WriteLine($"Created: {deposit.CreatedAt}");
    
    if (deposit.Fee != null)
    {
        Console.WriteLine($"Fee: ${deposit.Fee.Value} {deposit.Fee.Currency}");
    }
    
    if (deposit.CompletedAt.HasValue)
    {
        Console.WriteLine($"Completed: {deposit.CompletedAt}");
    }
    else
    {
        Console.WriteLine("Deposit is processing...");
    }
}
```

#### Initiate Withdrawal to Bank Account
```csharp
// Withdraw funds back to your bank account
var withdrawalRequest = new WithdrawalRequest
{
    Amount = 50.00m,
    Currency = "USD",
    PaymentMethodId = achPaymentMethod.Id,
    Commit = true
};

var withdrawal = await client.Transfers.WithdrawAsync(withdrawalRequest);

Console.WriteLine($"Withdrawal initiated: {withdrawal.Id}");
Console.WriteLine($"Amount: ${withdrawal.Amount.Value} {withdrawal.Amount.Currency}");
Console.WriteLine($"Status: {withdrawal.Status}");
Console.WriteLine($"Method: {withdrawal.Method}");

// Note: ACH withdrawals typically take 1-3 business days to complete
if (withdrawal.Status == TransferStatus.Pending)
{
    Console.WriteLine("Withdrawal is pending - funds will be transferred to your bank account in 1-3 business days");
}
```

#### Monitor Transfer Status
```csharp
// Get specific transfer details
var transfer = await client.Transfers.GetTransferAsync("transfer-id-here");

Console.WriteLine($"Transfer {transfer.Id}:");
Console.WriteLine($"  Type: {transfer.Type}");
Console.WriteLine($"  Amount: ${transfer.Amount.Value} {transfer.Amount.Currency}");
Console.WriteLine($"  Status: {transfer.Status}");
Console.WriteLine($"  Method: {transfer.Method}");
Console.WriteLine($"  Created: {transfer.CreatedAt}");

if (transfer.CompletedAt.HasValue)
{
    Console.WriteLine($"  Completed: {transfer.CompletedAt}");
}

if (transfer.Details != null)
{
    Console.WriteLine($"  Details: {transfer.Details.Title}");
    if (!string.IsNullOrEmpty(transfer.Details.Subtitle))
    {
        Console.WriteLine($"  Subtitle: {transfer.Details.Subtitle}");
    }
}
```

#### Get Transfer History with Filtering
```csharp
// Get all transfers
var allTransfers = await client.Transfers.GetTransfersAsync();
Console.WriteLine($"Total transfers: {allTransfers.Count()}");

// Get only deposits from the last 30 days
var recentDeposits = await client.Transfers.GetTransfersAsync(new TransferFilter
{
    Type = TransferType.Deposit,
    StartDate = DateTime.UtcNow.AddDays(-30),
    EndDate = DateTime.UtcNow,
    Limit = 50
});

Console.WriteLine($"Recent deposits (last 30 days): {recentDeposits.Count()}");
foreach (var deposit in recentDeposits.OrderByDescending(t => t.CreatedAt))
{
    Console.WriteLine($"  {deposit.CreatedAt:yyyy-MM-dd}: ${deposit.Amount.Value:F2} - {deposit.Status}");
}

// Get only ACH transfers
var achTransfers = await client.Transfers.GetTransfersAsync(new TransferFilter
{
    Method = TransferMethod.AchBankAccount,
    Limit = 25
});

Console.WriteLine($"ACH transfers: {achTransfers.Count()}");

// Get completed withdrawals
var completedWithdrawals = await client.Transfers.GetTransfersAsync(new TransferFilter
{
    Type = TransferType.Withdraw,
    Status = TransferStatus.Completed
});

Console.WriteLine($"Completed withdrawals: {completedWithdrawals.Count()}");
```

#### Two-Step Transfer Process (Pending + Commit)
```csharp
// Step 1: Create a pending deposit (commit = false)
var pendingDepositRequest = new DepositRequest
{
    Amount = 250.00m,
    Currency = "USD",
    PaymentMethodId = achPaymentMethod.Id,
    Commit = false // Create as pending
};

var pendingDeposit = await client.Transfers.DepositAsync(pendingDepositRequest);
Console.WriteLine($"Pending deposit created: {pendingDeposit.Id}");
Console.WriteLine($"Status: {pendingDeposit.Status}"); // Should be Pending

// Step 2: Review and commit the deposit
Console.WriteLine("Review the deposit details...");
Console.WriteLine($"Amount: ${pendingDeposit.Amount.Value} {pendingDeposit.Amount.Currency}");
Console.WriteLine($"Payment Method: {pendingDeposit.PaymentMethodId}");

// Commit the deposit
var committedDeposit = await client.Transfers.CommitTransferAsync(pendingDeposit.Id);
Console.WriteLine($"Deposit committed: {committedDeposit.Id}");
Console.WriteLine($"New status: {committedDeposit.Status}"); // Should be Processing or Completed
```

#### Complete ACH Funding Workflow
```csharp
using Coinbase.SDK;
using Coinbase.SDK.Models;
using Coinbase.SDK.Exceptions;

public class ACHFundingWorkflow
{
    private readonly ICoinbaseClient _client;
    
    public ACHFundingWorkflow(ICoinbaseClient client)
    {
        _client = client;
    }
    
    public async Task<bool> FundAccountAsync(decimal amount)
    {
        try
        {
            // Step 1: Get available payment methods
            var paymentMethods = await _client.Transfers.GetPaymentMethodsAsync();
            var achMethod = paymentMethods.FirstOrDefault(pm => 
                pm.Type == "ach_bank_account" && pm.PrimaryBuy);
            
            if (achMethod == null)
            {
                Console.WriteLine("No ACH bank account found. Please add a bank account in Coinbase.");
                return false;
            }
            
            // Step 2: Check deposit limits
            if (achMethod.Limits?.Deposit?.Any() == true)
            {
                var dailyLimit = achMethod.Limits.Deposit.FirstOrDefault(l => l.PeriodInDays == 1);
                if (dailyLimit != null && amount > dailyLimit.Remaining.Value)
                {
                    Console.WriteLine($"Amount ${amount:F2} exceeds daily limit. " +
                                    $"Remaining: ${dailyLimit.Remaining.Value:F2}");
                    return false;
                }
            }
            
            // Step 3: Initiate deposit
            var depositRequest = new DepositRequest
            {
                Amount = amount,
                Currency = "USD",
                PaymentMethodId = achMethod.Id,
                Commit = true
            };
            
            var deposit = await _client.Transfers.DepositAsync(depositRequest);
            
            Console.WriteLine($"✅ ACH deposit initiated successfully!");
            Console.WriteLine($"Transfer ID: {deposit.Id}");
            Console.WriteLine($"Amount: ${deposit.Amount.Value:F2} {deposit.Amount.Currency}");
            Console.WriteLine($"Status: {deposit.Status}");
            Console.WriteLine($"Expected completion: 1-3 business days");
            
            // Step 4: Monitor status (optional)
            await MonitorTransferStatus(deposit.Id);
            
            return true;
        }
        catch (CoinbaseApiException ex)
        {
            Console.WriteLine($"❌ API Error: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            return false;
        }
    }
    
    private async Task MonitorTransferStatus(string transferId)
    {
        Console.WriteLine("\n📊 Monitoring transfer status...");
        
        for (int i = 0; i < 5; i++) // Check 5 times
        {
            await Task.Delay(TimeSpan.FromSeconds(10)); // Wait 10 seconds
            
            var transfer = await _client.Transfers.GetTransferAsync(transferId);
            Console.WriteLine($"Status check {i + 1}: {transfer.Status}");
            
            if (transfer.Status == TransferStatus.Completed)
            {
                Console.WriteLine("✅ Transfer completed!");
                break;
            }
            else if (transfer.Status == TransferStatus.Failed || transfer.Status == TransferStatus.Cancelled)
            {
                Console.WriteLine($"❌ Transfer {transfer.Status.ToString().ToLower()}");
                break;
            }
        }
    }
}

// Usage
var client = CoinbaseClientFactory.CreateFromEnvironment();
var workflow = new ACHFundingWorkflow(client);

// Fund account with $500
var success = await workflow.FundAccountAsync(500.00m);
if (success)
{
    Console.WriteLine("Account funding initiated successfully!");
}
```

## Advanced Configuration

### Using Configuration Files

#### appsettings.json
```json
{
  "Coinbase": {
    "ApiKeyId": "your-api-key-id",
    "PrivateKey": "your-private-key",
    "BaseUrl": "https://api.coinbase.com",
    "TokenExpirationMinutes": 1
  }
}
```

#### Program.cs with Configuration
```csharp
using Coinbase.SDK.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Add configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: false);

// Register Coinbase SDK with configuration binding
builder.Services.AddCoinbaseSDK(builder.Configuration.GetSection("Coinbase"));

var host = builder.Build();
var client = host.Services.GetRequiredService<ICoinbaseClient>();
```

### Custom Configuration Manager
```csharp
using Coinbase.SDK;
using Coinbase.SDK.Services;

// Create custom configuration
var configManager = new ConfigurationManager
{
    ApiKeyId = "your-api-key-id",
    PrivateKey = "your-private-key",
    BaseUrl = "https://api.coinbase.com"
};

var client = CoinbaseClientFactory.Create(configManager);
```

## Error Handling and Best Practices

### Comprehensive Error Handling
```csharp
using Coinbase.SDK;
using Coinbase.SDK.Exceptions;
using Coinbase.SDK.Models;

var client = CoinbaseClientFactory.CreateFromEnvironment();

try
{
    var order = await client.Trading.PlaceMarketOrderAsync(new MarketOrderRequest
    {
        ProductId = "BTC-USD",
        Side = OrderSide.Buy,
        Size = 0.001m
    });
    
    Console.WriteLine($"Order placed successfully: {order.OrderId}");
}
catch (CoinbaseAuthenticationException ex)
{
    Console.WriteLine($"Authentication failed: {ex.Message}");
    Console.WriteLine("Please check your API credentials");
}
catch (CoinbaseApiException ex)
{
    Console.WriteLine($"API error ({ex.HttpStatusCode}): {ex.Message}");
    Console.WriteLine($"Error code: {ex.ErrorCode}");
    
    // Handle specific HTTP status codes
    switch (ex.HttpStatusCode)
    {
        case 400:
            Console.WriteLine("Bad request - check your parameters");
            break;
        case 401:
            Console.WriteLine("Unauthorized - check your credentials");
            break;
        case 403:
            Console.WriteLine("Forbidden - insufficient permissions");
            break;
        case 429:
            Console.WriteLine("Rate limited - please wait before retrying");
            break;
        case 500:
            Console.WriteLine("Server error - please try again later");
            break;
    }
}
catch (CoinbaseNetworkException ex)
{
    Console.WriteLine($"Network error: {ex.Message}");
    Console.WriteLine("Please check your internet connection and try again");
}
catch (CoinbaseConfigurationException ex)
{
    Console.WriteLine($"Configuration error: {ex.Message}");
    Console.WriteLine("Please check your SDK configuration");
}
catch (OperationCanceledException)
{
    Console.WriteLine("Operation was cancelled");
}
catch (Exception ex)
{
    Console.WriteLine($"Unexpected error: {ex.Message}");
}
```

### Using Cancellation Tokens
```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

try
{
    var accounts = await client.Accounts.GetAccountsAsync(cts.Token);
    var orders = await client.Trading.GetOrdersAsync(cancellationToken: cts.Token);
    var products = await client.MarketData.GetProductsAsync(cts.Token);
    
    Console.WriteLine("All operations completed successfully");
}
catch (OperationCanceledException)
{
    Console.WriteLine("Operations timed out after 30 seconds");
}
```

### Retry Logic Example
```csharp
using Polly;
using Coinbase.SDK.Exceptions;

var retryPolicy = Policy
    .Handle<CoinbaseNetworkException>()
    .Or<CoinbaseApiException>(ex => ex.HttpStatusCode >= 500)
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
        onRetry: (outcome, timespan, retryCount, context) =>
        {
            Console.WriteLine($"Retry {retryCount} after {timespan} seconds");
        });

var accounts = await retryPolicy.ExecuteAsync(async () =>
{
    return await client.Accounts.GetAccountsAsync();
});
```

## Complete Trading Bot Example

```csharp
using Coinbase.SDK;
using Coinbase.SDK.Models;
using Coinbase.SDK.Exceptions;

public class SimpleTradingBot
{
    private readonly ICoinbaseClient _client;
    private readonly string _productId;
    
    public SimpleTradingBot(ICoinbaseClient client, string productId = "BTC-USD")
    {
        _client = client;
        _productId = productId;
    }
    
    public async Task RunAsync()
    {
        Console.WriteLine($"Starting trading bot for {_productId}");
        
        while (true)
        {
            try
            {
                await AnalyzeAndTrade();
                await Task.Delay(TimeSpan.FromMinutes(5)); // Check every 5 minutes
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in trading loop: {ex.Message}");
                await Task.Delay(TimeSpan.FromMinutes(1)); // Wait before retrying
            }
        }
    }
    
    private async Task AnalyzeAndTrade()
    {
        // Get recent candle data
        var candles = await _client.MarketData.GetCandlesAsync(
            _productId,
            DateTime.UtcNow.AddHours(-24),
            DateTime.UtcNow,
            Granularity.OneHour
        );
        
        var recentCandles = candles.OrderBy(c => c.Start).TakeLast(24).ToList();
        
        if (recentCandles.Count < 24)
        {
            Console.WriteLine("Not enough data for analysis");
            return;
        }
        
        // Simple moving average strategy
        var sma12 = recentCandles.TakeLast(12).Average(c => c.Close);
        var sma24 = recentCandles.Average(c => c.Close);
        var currentPrice = recentCandles.Last().Close;
        
        Console.WriteLine($"Current Price: ${currentPrice:F2}");
        Console.WriteLine($"SMA 12: ${sma12:F2}");
        Console.WriteLine($"SMA 24: ${sma24:F2}");
        
        // Get current orders
        var openOrders = await _client.Trading.GetOrdersAsync(new OrderFilter
        {
            ProductId = _productId,
            Status = OrderStatus.Open
        });
        
        // Simple strategy: Buy when SMA12 > SMA24, Sell when SMA12 < SMA24
        if (sma12 > sma24 && !openOrders.Any(o => o.Side == OrderSide.Buy))
        {
            await PlaceBuyOrder(currentPrice * 0.99m); // Buy 1% below current price
        }
        else if (sma12 < sma24 && !openOrders.Any(o => o.Side == OrderSide.Sell))
        {
            await PlaceSellOrder(currentPrice * 1.01m); // Sell 1% above current price
        }
    }
    
    private async Task PlaceBuyOrder(decimal price)
    {
        try
        {
            var order = await _client.Trading.PlaceLimitOrderAsync(new LimitOrderRequest
            {
                ProductId = _productId,
                Side = OrderSide.Buy,
                Size = 0.001m, // Small size for demo
                Price = price,
                TimeInForce = TimeInForce.GTC
            });
            
            Console.WriteLine($"Buy order placed: {order.OrderId} at ${price:F2}");
        }
        catch (CoinbaseApiException ex)
        {
            Console.WriteLine($"Failed to place buy order: {ex.Message}");
        }
    }
    
    private async Task PlaceSellOrder(decimal price)
    {
        try
        {
            var order = await _client.Trading.PlaceLimitOrderAsync(new LimitOrderRequest
            {
                ProductId = _productId,
                Side = OrderSide.Sell,
                Size = 0.001m, // Small size for demo
                Price = price,
                TimeInForce = TimeInForce.GTC
            });
            
            Console.WriteLine($"Sell order placed: {order.OrderId} at ${price:F2}");
        }
        catch (CoinbaseApiException ex)
        {
            Console.WriteLine($"Failed to place sell order: {ex.Message}");
        }
    }
}

// Usage
var client = CoinbaseClientFactory.CreateFromEnvironment();
var bot = new SimpleTradingBot(client, "BTC-USD");
await bot.RunAsync();
```

## Environment Variables

The SDK supports the following environment variables:

- `COINBASE_API_ID`: Your Coinbase API key ID (required)
- `COINBASE_API_PRIVATE_KEY`: Your Coinbase private key (required)
- `COINBASE_BASE_URL`: Optional base URL (defaults to `https://api.coinbase.com`)

## Supported Order Types and Enums

### OrderSide
- `Buy`: Buy order
- `Sell`: Sell order

### OrderType
- `Market`: Market order (executed immediately at current market price)
- `Limit`: Limit order (executed only at specified price or better)
- `Stop`: Stop order
- `StopLimit`: Stop limit order

### OrderStatus
- `Pending`: Order is pending
- `Open`: Order is open and active
- `Filled`: Order has been completely filled
- `Cancelled`: Order has been cancelled
- `Expired`: Order has expired

### TimeInForce
- `GTC`: Good Till Cancelled (default)
- `GTT`: Good Till Time
- `IOC`: Immediate Or Cancel
- `FOK`: Fill Or Kill

### Granularity (for candle data)
- `OneMinute`: 1-minute candles
- `FiveMinutes`: 5-minute candles
- `FifteenMinutes`: 15-minute candles
- `OneHour`: 1-hour candles
- `SixHours`: 6-hour candles
- `OneDay`: 1-day candles

### TransferType
- `Deposit`: Deposit transfer (adding funds to Coinbase)
- `Withdraw`: Withdrawal transfer (removing funds from Coinbase)

### TransferMethod
- `AchBankAccount`: ACH bank account transfer
- `Wire`: Wire transfer
- `CreditCard`: Credit card
- `DebitCard`: Debit card
- `PayPal`: PayPal account
- `BankAccount`: General bank account

### TransferStatus
- `Pending`: Transfer is pending
- `Completed`: Transfer is completed
- `Cancelled`: Transfer was cancelled
- `Failed`: Transfer failed
- `Processing`: Transfer is processing
- `WaitingForClearing`: Transfer is waiting for clearance
- `WaitingForSignature`: Transfer is waiting for signature

## Requirements

- .NET 6.0 or later
- Valid Coinbase Advanced Trade API credentials
- Internet connection for API access

## Dependencies

- `Newtonsoft.Json` (≥ 13.0.3): JSON serialization
- `System.IdentityModel.Tokens.Jwt` (≥ 8.1.2): JWT token handling
- `Microsoft.Extensions.DependencyInjection.Abstractions` (≥ 8.0.0): Dependency injection
- `Microsoft.Extensions.Options` (≥ 8.0.0): Options pattern support

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request. The project includes comprehensive property-based tests to ensure correctness.

## Support

For support, please:
1. Check the [Coinbase Developer Documentation](https://docs.cloud.coinbase.com/advanced-trade-api/docs)
2. Create an issue in this repository
3. Review the comprehensive test suite for usage examples

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for a detailed history of changes and updates.
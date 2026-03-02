using Coinbase.SDK;
using Coinbase.SDK.Models;
using Coinbase.SDK.Services;
using Coinbase.SDK.Exceptions;

var buyMode = args.Contains("--buy");

Console.WriteLine(buyMode
    ? "=== Bitcoin Purchase + Order History Test ==="
    : "=== Order History Test (read-only, no purchase) ===");
Console.WriteLine();

try
{
    // Step 1: Initialize client
    Console.WriteLine("1. Initializing Coinbase client...");
    var client = await CoinbaseClientFactory.CreateFromJsonFileAsync("/home/ubuntu/Development/RiderProjects/CoinbaseTest/.env");
    Console.WriteLine("   Client initialized successfully");
    Console.WriteLine();

    // Step 2: Verify API access
    Console.WriteLine("2. Verifying API access (listing accounts)...");
    var accounts0 = await client.Accounts.GetAccountsAsync();
    Console.WriteLine($"   Found {accounts0.Count()} accounts - auth works");
    Console.WriteLine();

    // Step 3: Buy (only if --buy flag passed)
    if (buyMode)
    {
        Console.WriteLine("3. Placing $20 BTC market buy order...");

        var bitcoinRequest = new BitcoinBuyRequest
        {
            ProductId = "BTC-USD"
        };
        bitcoinRequest.SetUsdAmount(20.00m);

        var rawResponse = await client.Trading.PlaceBitcoinBuyOrderAsync(bitcoinRequest);
        Console.WriteLine($"   Order placed: success={rawResponse.Success}, order_id={rawResponse.OrderId}");
        Console.WriteLine();
    }
    else
    {
        Console.WriteLine("3. Skipping buy (pass --buy to place an order)");
        Console.WriteLine();
    }

    // Step 4: Retrieve order history (the main test)
    Console.WriteLine("4. Retrieving BTC-USD order history...");

    var result = await client.Trading.GetOrdersAsync(new OrderFilter
    {
        ProductId = "BTC-USD",
        Side = OrderSide.Buy
    });

    Console.WriteLine($"   Found {result.Orders.Count()} orders (has_next={result.HasNext})");

    foreach (var order in result.Orders.OrderByDescending(o => o.CreatedTime).Take(5))
    {
        Console.WriteLine($"   ---");
        Console.WriteLine($"   Order ID:   {order.OrderId}");
        Console.WriteLine($"   Product:    {order.ProductId}");
        Console.WriteLine($"   Side:       {order.Side}");
        Console.WriteLine($"   Status:     {order.Status}");
        Console.WriteLine($"   Type:       {order.Type}");
        Console.WriteLine($"   Created:    {order.CreatedTime}");
        Console.WriteLine($"   Filled:     {order.FilledSize} @ avg ${order.AverageFilledPrice}");
        Console.WriteLine($"   Value:      ${order.FilledValue}");
        Console.WriteLine($"   Fees:       ${order.TotalFees}");
        Console.WriteLine($"   Completion: {order.CompletionPercentage}%");
    }

    Console.WriteLine();

    // Step 5: Test single order retrieval if we have any orders
    var firstOrder = result.Orders.FirstOrDefault();
    if (firstOrder != null)
    {
        Console.WriteLine($"5. Retrieving single order by ID: {firstOrder.OrderId}...");
        var singleOrder = await client.Trading.GetOrderAsync(firstOrder.OrderId);
        Console.WriteLine($"   Got order: {singleOrder.OrderId} - {singleOrder.Status} - {singleOrder.ProductId}");
        Console.WriteLine();
    }

    // Step 6: Test unfiltered order listing
    Console.WriteLine("6. Retrieving all recent orders (no filter)...");
    var allOrders = await client.Trading.GetOrdersAsync();
    Console.WriteLine($"   Found {allOrders.Orders.Count()} orders total across all products");
    Console.WriteLine();

    // Step 7: Check account balances
    Console.WriteLine("7. Checking account balances...");
    var accounts = await client.Accounts.GetAccountsAsync();
    var usdAccount = accounts.FirstOrDefault(a => a.Currency == "USD");
    var btcAccount = accounts.FirstOrDefault(a => a.Currency == "BTC");

    if (usdAccount != null)
        Console.WriteLine($"   USD: Available = ${usdAccount.AvailableBalance?.Value:F2}, Hold = ${usdAccount.Hold?.Value:F2}");
    if (btcAccount != null)
        Console.WriteLine($"   BTC: Available = {btcAccount.AvailableBalance?.Value:F8}, Hold = {btcAccount.Hold?.Value:F8}");

    Console.WriteLine();
    Console.WriteLine("All tests passed.");
}
catch (CoinbaseTradingException ex)
{
    Console.WriteLine($"Trading error: {ex.Message}");
    Console.WriteLine($"  Error Code: {ex.ErrorCode}");
}
catch (CoinbaseAuthenticationException ex)
{
    Console.WriteLine($"Authentication failed: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"  Type: {ex.GetType().Name}");
    if (ex.InnerException != null)
        Console.WriteLine($"  Inner: {ex.InnerException.Message}");
}

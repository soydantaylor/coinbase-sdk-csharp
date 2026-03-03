using Newtonsoft.Json;

namespace Coinbase.SDK.Models;

/// <summary>
/// Request parameters for placing a market order
/// </summary>
public class MarketOrderRequest
{
    /// <summary>
    /// The product identifier to trade
    /// </summary>
    [JsonProperty("product_id")]
    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// The order side (buy or sell)
    /// </summary>
    [JsonProperty("side")]
    public OrderSide Side { get; set; }

    /// <summary>
    /// The order size
    /// </summary>
    [JsonProperty("size")]
    public decimal Size { get; set; }
}

/// <summary>
/// Request parameters for placing a limit order
/// </summary>
public class LimitOrderRequest
{
    /// <summary>
    /// The product identifier to trade
    /// </summary>
    [JsonProperty("product_id")]
    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// The order side (buy or sell)
    /// </summary>
    [JsonProperty("side")]
    public OrderSide Side { get; set; }

    /// <summary>
    /// The order size
    /// </summary>
    [JsonProperty("size")]
    public decimal Size { get; set; }

    /// <summary>
    /// The limit price
    /// </summary>
    [JsonProperty("price")]
    public decimal Price { get; set; }

    /// <summary>
    /// Time in force for the order
    /// </summary>
    [JsonProperty("time_in_force")]
    public TimeInForce TimeInForce { get; set; } = TimeInForce.GTC;
}

/// <summary>
/// Request parameters for placing a Bitcoin buy order
/// </summary>
public class BitcoinBuyRequest
{
    /// <summary>
    /// Client-generated order ID
    /// </summary>
    [JsonProperty("client_order_id")]
    public string ClientOrderId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The product identifier (defaults to BTC-USD)
    /// </summary>
    [JsonProperty("product_id")]
    public string ProductId { get; set; } = "BTC-USD";

    /// <summary>
    /// The order side (must be BUY for buy orders)
    /// </summary>
    [JsonProperty("side")]
    public string Side { get; set; } = "BUY";

    /// <summary>
    /// Order configuration for market order
    /// </summary>
    [JsonProperty("order_configuration")]
    public OrderConfiguration OrderConfiguration { get; set; } = new OrderConfiguration();

    /// <summary>
    /// Sets the USD amount to spend on Bitcoin
    /// </summary>
    public void SetUsdAmount(decimal amount)
    {
        OrderConfiguration.MarketMarketIoc.QuoteSize = amount.ToString("F2");
    }

    /// <summary>
    /// Gets the USD amount
    /// </summary>
    [JsonIgnore]
    public decimal UsdAmount => decimal.TryParse(OrderConfiguration.MarketMarketIoc.QuoteSize, out var amount) ? amount : 0;
}

/// <summary>
/// Request parameters for previewing a Bitcoin buy order (no client_order_id needed)
/// </summary>
public class BitcoinBuyPreviewRequest
{
    /// <summary>
    /// The product identifier (defaults to BTC-USD)
    /// </summary>
    [JsonProperty("product_id")]
    public string ProductId { get; set; } = "BTC-USD";

    /// <summary>
    /// The order side (must be BUY for buy orders)
    /// </summary>
    [JsonProperty("side")]
    public string Side { get; set; } = "BUY";

    /// <summary>
    /// Order configuration for market order
    /// </summary>
    [JsonProperty("order_configuration")]
    public OrderConfiguration OrderConfiguration { get; set; } = new OrderConfiguration();

    /// <summary>
    /// Sets the USD amount to spend on Bitcoin
    /// </summary>
    public void SetUsdAmount(decimal amount)
    {
        OrderConfiguration.MarketMarketIoc.QuoteSize = amount.ToString("F2");
    }

    /// <summary>
    /// Gets the USD amount
    /// </summary>
    [JsonIgnore]
    public decimal UsdAmount => decimal.TryParse(OrderConfiguration.MarketMarketIoc.QuoteSize, out var amount) ? amount : 0;
}

/// <summary>
/// Response from Bitcoin buy order preview - matches Coinbase API preview structure
/// </summary>
public class BitcoinBuyPreviewResponse
{
    [JsonProperty("order_total")]
    public string OrderTotal { get; set; } = "0";

    [JsonProperty("commission_total")]
    public string CommissionTotal { get; set; } = "0";

    [JsonProperty("quote_size")]
    public string? QuoteSize { get; set; }

    [JsonProperty("base_size")]
    public string? BaseSize { get; set; }

    [JsonProperty("best_bid")]
    public string? BestBid { get; set; }

    [JsonProperty("best_ask")]
    public string? BestAsk { get; set; }

    [JsonProperty("is_max")]
    public bool IsMax { get; set; }

    [JsonProperty("slippage")]
    public string? Slippage { get; set; }

    [JsonProperty("errs")]
    public string[]? Errs { get; set; }

    [JsonProperty("warning")]
    public string[]? Warning { get; set; }

    [JsonProperty("preview_id")]
    public string? PreviewId { get; set; }

    [JsonIgnore]
    public decimal CommissionTotalDecimal => decimal.TryParse(CommissionTotal, out var v) ? v : 0m;

    [JsonIgnore]
    public decimal OrderTotalDecimal => decimal.TryParse(OrderTotal, out var v) ? v : 0m;
}

/// <summary>
/// Order configuration for market orders
/// </summary>
public class OrderConfiguration
{
    /// <summary>
    /// Market order configuration
    /// </summary>
    [JsonProperty("market_market_ioc")]
    public MarketMarketIoc MarketMarketIoc { get; set; } = new MarketMarketIoc();
}

/// <summary>
/// Market order immediate-or-cancel configuration
/// </summary>
public class MarketMarketIoc
{
    /// <summary>
    /// The quote size (USD amount) for the order
    /// </summary>
    [JsonProperty("quote_size")]
    public string QuoteSize { get; set; } = "0.00";
}

/// <summary>
/// Response from Bitcoin buy order placement - matches Coinbase API structure
/// </summary>
public class BitcoinBuyResponse
{
    /// <summary>
    /// Success indicator
    /// </summary>
    [JsonProperty("success")]
    public bool Success { get; set; }

    /// <summary>
    /// Failure reason if not successful
    /// </summary>
    [JsonProperty("failure_reason")]
    public string? FailureReason { get; set; }

    /// <summary>
    /// The order ID if successful
    /// </summary>
    [JsonProperty("order_id")]
    public string OrderId { get; set; } = string.Empty;

    /// <summary>
    /// Success response details
    /// </summary>
    [JsonProperty("success_response")]
    public OrderSuccessResponse? SuccessResponse { get; set; }

    /// <summary>
    /// Error response details
    /// </summary>
    [JsonProperty("error_response")]
    public OrderErrorResponse? ErrorResponse { get; set; }

    // Convenience properties for backward compatibility
    [JsonIgnore]
    public decimal UsdAmount => SuccessResponse?.OrderConfiguration?.MarketMarketIoc?.QuoteSize != null 
        ? decimal.TryParse(SuccessResponse.OrderConfiguration.MarketMarketIoc.QuoteSize, out var amount) ? amount : 0 
        : 0;

    [JsonIgnore]
    public decimal? BtcAmount => null; // Not available in initial response

    [JsonIgnore]
    public string Status => Success ? "PENDING" : "FAILED";

    [JsonIgnore]
    public DateTime CreatedAt => DateTime.UtcNow; // Approximate
}

/// <summary>
/// Success response details
/// </summary>
public class OrderSuccessResponse
{
    [JsonProperty("order_id")]
    public string OrderId { get; set; } = string.Empty;

    [JsonProperty("product_id")]
    public string ProductId { get; set; } = string.Empty;

    [JsonProperty("side")]
    public string Side { get; set; } = string.Empty;

    [JsonProperty("client_order_id")]
    public string ClientOrderId { get; set; } = string.Empty;

    [JsonProperty("order_configuration")]
    public OrderConfiguration? OrderConfiguration { get; set; }
}

/// <summary>
/// Error response details
/// </summary>
public class OrderErrorResponse
{
    [JsonProperty("error")]
    public string Error { get; set; } = string.Empty;

    [JsonProperty("message")]
    public string Message { get; set; } = string.Empty;

    [JsonProperty("error_details")]
    public string ErrorDetails { get; set; } = string.Empty;
}

/// <summary>
/// Filter criteria for querying orders via /api/v3/brokerage/orders/historical/batch
/// </summary>
public class OrderFilter
{
    /// <summary>
    /// Filter by product identifier (e.g. "BTC-USD")
    /// </summary>
    public string? ProductId { get; set; }

    /// <summary>
    /// Filter by order statuses (OPEN, FILLED, CANCELLED, EXPIRED, FAILED, PENDING, QUEUED)
    /// </summary>
    public List<OrderStatus>? Statuses { get; set; }

    /// <summary>
    /// Filter by order side (BUY or SELL)
    /// </summary>
    public OrderSide? Side { get; set; }

    /// <summary>
    /// Start date for filtering (inclusive, RFC3339)
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// End date for filtering (exclusive, RFC3339)
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Filter by order types (MARKET, LIMIT, STOP, STOP_LIMIT)
    /// </summary>
    public List<OrderType>? OrderTypes { get; set; }

    /// <summary>
    /// Max results per page
    /// </summary>
    public int? Limit { get; set; }

    /// <summary>
    /// Pagination cursor from a previous response
    /// </summary>
    public string? Cursor { get; set; }

    /// <summary>
    /// Product type filter (default SPOT)
    /// </summary>
    public string? ProductType { get; set; }

    /// <summary>
    /// Sort by field
    /// </summary>
    public string? SortBy { get; set; }
}
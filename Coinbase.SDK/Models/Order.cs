using Newtonsoft.Json;

namespace Coinbase.SDK.Models;

/// <summary>
/// Represents a trading order from the Coinbase Advanced Trade API v3.
/// Note: Coinbase returns most numeric values as strings.
/// </summary>
public class Order
{
    [JsonProperty("order_id")]
    public string OrderId { get; set; } = string.Empty;

    [JsonProperty("product_id")]
    public string ProductId { get; set; } = string.Empty;

    [JsonProperty("user_id")]
    public string? UserId { get; set; }

    [JsonProperty("side")]
    public OrderSide Side { get; set; }

    [JsonProperty("client_order_id")]
    public string? ClientOrderId { get; set; }

    [JsonProperty("status")]
    public OrderStatus Status { get; set; }

    [JsonProperty("order_type")]
    public OrderType Type { get; set; }

    [JsonProperty("time_in_force")]
    public string? TimeInForce { get; set; }

    [JsonProperty("created_time")]
    public DateTime CreatedTime { get; set; }

    [JsonProperty("last_update_time")]
    public DateTime? LastUpdateTime { get; set; }

    [JsonProperty("completion_percentage")]
    public string? CompletionPercentage { get; set; }

    [JsonProperty("filled_size")]
    public string? FilledSize { get; set; }

    [JsonProperty("average_filled_price")]
    public string? AverageFilledPrice { get; set; }

    [JsonProperty("filled_value")]
    public string? FilledValue { get; set; }

    [JsonProperty("number_of_fills")]
    public string? NumberOfFills { get; set; }

    [JsonProperty("total_fees")]
    public string? TotalFees { get; set; }

    [JsonProperty("total_value_after_fees")]
    public string? TotalValueAfterFees { get; set; }

    [JsonProperty("pending_cancel")]
    public bool PendingCancel { get; set; }

    [JsonProperty("size_in_quote")]
    public bool SizeInQuote { get; set; }

    [JsonProperty("size_inclusive_of_fees")]
    public bool SizeInclusiveOfFees { get; set; }

    [JsonProperty("settled")]
    public bool? Settled { get; set; }

    [JsonProperty("order_configuration")]
    public OrderConfiguration? OrderConfiguration { get; set; }

    [JsonProperty("order_placement_source")]
    public string? OrderPlacementSource { get; set; }

    [JsonProperty("reject_reason")]
    public string? RejectReason { get; set; }

    [JsonProperty("reject_message")]
    public string? RejectMessage { get; set; }

    [JsonProperty("cancel_message")]
    public string? CancelMessage { get; set; }

    [JsonProperty("outstanding_hold_amount")]
    public string? OutstandingHoldAmount { get; set; }

    [JsonProperty("is_liquidation")]
    public bool? IsLiquidation { get; set; }

    [JsonProperty("last_fill_time")]
    public DateTime? LastFillTime { get; set; }

    [JsonProperty("product_type")]
    public string? ProductType { get; set; }

    // Convenience helpers to parse string amounts to decimal
    [JsonIgnore]
    public decimal FilledSizeDecimal => decimal.TryParse(FilledSize, out var v) ? v : 0m;

    [JsonIgnore]
    public decimal FilledValueDecimal => decimal.TryParse(FilledValue, out var v) ? v : 0m;

    [JsonIgnore]
    public decimal AverageFilledPriceDecimal => decimal.TryParse(AverageFilledPrice, out var v) ? v : 0m;

    [JsonIgnore]
    public decimal TotalFeesDecimal => decimal.TryParse(TotalFees, out var v) ? v : 0m;
}
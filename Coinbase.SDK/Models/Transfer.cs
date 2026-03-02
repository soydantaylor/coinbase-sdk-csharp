using Newtonsoft.Json;

namespace Coinbase.SDK.Models;

/// <summary>
/// Represents a transfer operation (deposit/withdrawal)
/// </summary>
public class Transfer
{
    /// <summary>
    /// The unique transfer identifier
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The transfer type (deposit, withdrawal)
    /// </summary>
    [JsonProperty("type")]
    public TransferType Type { get; set; }

    /// <summary>
    /// The transfer method (ach_bank_account, wire, etc.)
    /// </summary>
    [JsonProperty("method")]
    public TransferMethod Method { get; set; }

    /// <summary>
    /// The amount being transferred
    /// </summary>
    [JsonProperty("amount")]
    public Money Amount { get; set; } = new Money();

    /// <summary>
    /// The current status of the transfer
    /// </summary>
    [JsonProperty("status")]
    public TransferStatus Status { get; set; }

    /// <summary>
    /// When the transfer was created
    /// </summary>
    [JsonProperty("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the transfer was completed (if applicable)
    /// </summary>
    [JsonProperty("completed_at")]
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// The payment method ID used for this transfer
    /// </summary>
    [JsonProperty("payment_method_id")]
    public string PaymentMethodId { get; set; } = string.Empty;

    /// <summary>
    /// The account ID associated with this transfer
    /// </summary>
    [JsonProperty("account_id")]
    public string AccountId { get; set; } = string.Empty;

    /// <summary>
    /// Additional details about the transfer
    /// </summary>
    [JsonProperty("details")]
    public TransferDetails? Details { get; set; }

    /// <summary>
    /// Fee information for the transfer
    /// </summary>
    [JsonProperty("fee")]
    public Money? Fee { get; set; }

    /// <summary>
    /// The subtotal amount (amount - fee)
    /// </summary>
    [JsonProperty("subtotal")]
    public Money? Subtotal { get; set; }
}

/// <summary>
/// Additional details about a transfer
/// </summary>
public class TransferDetails
{
    /// <summary>
    /// The title or description of the transfer
    /// </summary>
    [JsonProperty("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The subtitle or additional description
    /// </summary>
    [JsonProperty("subtitle")]
    public string Subtitle { get; set; } = string.Empty;

    /// <summary>
    /// Any additional information about the transfer
    /// </summary>
    [JsonProperty("health")]
    public string Health { get; set; } = string.Empty;
}

/// <summary>
/// Represents a payment method for transfers
/// </summary>
public class PaymentMethod
{
    /// <summary>
    /// The unique payment method identifier
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The type of payment method
    /// </summary>
    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// The name of the payment method
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The currency supported by this payment method
    /// </summary>
    [JsonProperty("currency")]
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Whether this payment method is the primary one
    /// </summary>
    [JsonProperty("primary_buy")]
    public bool PrimaryBuy { get; set; }

    /// <summary>
    /// Whether this payment method can be used for selling
    /// </summary>
    [JsonProperty("primary_sell")]
    public bool PrimarySell { get; set; }

    /// <summary>
    /// Whether instant buy is available
    /// </summary>
    [JsonProperty("instant_buy")]
    public bool InstantBuy { get; set; }

    /// <summary>
    /// Whether instant sell is available
    /// </summary>
    [JsonProperty("instant_sell")]
    public bool InstantSell { get; set; }

    /// <summary>
    /// The limits for this payment method
    /// </summary>
    [JsonProperty("limits")]
    public PaymentMethodLimits? Limits { get; set; }
}

/// <summary>
/// Limits for a payment method
/// </summary>
public class PaymentMethodLimits
{
    /// <summary>
    /// Buy limits
    /// </summary>
    [JsonProperty("buy")]
    public IEnumerable<PaymentMethodLimit> Buy { get; set; } = new List<PaymentMethodLimit>();

    /// <summary>
    /// Instant buy limits
    /// </summary>
    [JsonProperty("instant_buy")]
    public IEnumerable<PaymentMethodLimit> InstantBuy { get; set; } = new List<PaymentMethodLimit>();

    /// <summary>
    /// Sell limits
    /// </summary>
    [JsonProperty("sell")]
    public IEnumerable<PaymentMethodLimit> Sell { get; set; } = new List<PaymentMethodLimit>();

    /// <summary>
    /// Deposit limits
    /// </summary>
    [JsonProperty("deposit")]
    public IEnumerable<PaymentMethodLimit> Deposit { get; set; } = new List<PaymentMethodLimit>();
}

/// <summary>
/// Individual limit for a payment method
/// </summary>
public class PaymentMethodLimit
{
    /// <summary>
    /// The period type (day, month, year, etc.)
    /// </summary>
    [JsonProperty("period_in_days")]
    public int PeriodInDays { get; set; }

    /// <summary>
    /// The total limit amount
    /// </summary>
    [JsonProperty("total")]
    public Money Total { get; set; } = new Money();

    /// <summary>
    /// The remaining amount available
    /// </summary>
    [JsonProperty("remaining")]
    public Money Remaining { get; set; } = new Money();
}

/// <summary>
/// Response model for payment methods API endpoint
/// </summary>
public class PaymentMethodsResponse
{
    /// <summary>
    /// Collection of payment methods returned from the API
    /// </summary>
    [JsonProperty("payment_methods")]
    public IEnumerable<PaymentMethod> PaymentMethods { get; set; } = Enumerable.Empty<PaymentMethod>();
}
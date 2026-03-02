using Newtonsoft.Json;

namespace Coinbase.SDK.Models;

/// <summary>
/// Request parameters for initiating a deposit
/// </summary>
public class DepositRequest
{
    /// <summary>
    /// The amount to deposit
    /// </summary>
    [JsonProperty("amount")]
    public decimal Amount { get; set; }

    /// <summary>
    /// The currency to deposit
    /// </summary>
    [JsonProperty("currency")]
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// The payment method ID to use for the deposit
    /// </summary>
    [JsonProperty("payment_method")]
    public string PaymentMethodId { get; set; } = string.Empty;

    /// <summary>
    /// Whether to commit the deposit immediately
    /// </summary>
    [JsonProperty("commit")]
    public bool Commit { get; set; } = true;
}

/// <summary>
/// Request parameters for initiating a withdrawal
/// </summary>
public class WithdrawalRequest
{
    /// <summary>
    /// The amount to withdraw
    /// </summary>
    [JsonProperty("amount")]
    public decimal Amount { get; set; }

    /// <summary>
    /// The currency to withdraw
    /// </summary>
    [JsonProperty("currency")]
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// The payment method ID to use for the withdrawal
    /// </summary>
    [JsonProperty("payment_method")]
    public string PaymentMethodId { get; set; } = string.Empty;

    /// <summary>
    /// Whether to commit the withdrawal immediately
    /// </summary>
    [JsonProperty("commit")]
    public bool Commit { get; set; } = true;
}

/// <summary>
/// Request parameters for initiating an ACH deposit
/// </summary>
public class AchDepositRequest
{
    /// <summary>
    /// The amount to deposit as a string
    /// </summary>
    [JsonProperty("amount")]
    public string Amount { get; set; } = string.Empty;

    /// <summary>
    /// The currency to deposit
    /// </summary>
    [JsonProperty("currency")]
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// The payment method ID to use for the ACH deposit
    /// </summary>
    [JsonProperty("payment_method")]
    public string PaymentMethodId { get; set; } = string.Empty;

    /// <summary>
    /// Whether to commit the deposit immediately
    /// </summary>
    [JsonProperty("commit")]
    public bool Commit { get; set; } = false;
}

/// <summary>
/// Response from an ACH deposit request - matches actual Coinbase API response structure
/// </summary>
public class AchDepositResponse
{
    /// <summary>
    /// The transfer object containing all transfer details
    /// </summary>
    [JsonProperty("transfer")]
    public ApiTransferDetails Transfer { get; set; } = new ApiTransferDetails();
}

/// <summary>
/// Transfer details from the API response
/// </summary>
public class ApiTransferDetails
{
    /// <summary>
    /// User entered amount
    /// </summary>
    [JsonProperty("user_entered_amount")]
    public Money? UserEnteredAmount { get; set; }

    /// <summary>
    /// The amount being transferred
    /// </summary>
    [JsonProperty("amount")]
    public Money? Amount { get; set; }

    /// <summary>
    /// The total amount
    /// </summary>
    [JsonProperty("total")]
    public Money? Total { get; set; }

    /// <summary>
    /// The subtotal amount
    /// </summary>
    [JsonProperty("subtotal")]
    public Money? Subtotal { get; set; }

    /// <summary>
    /// Idempotency key
    /// </summary>
    [JsonProperty("idem")]
    public string Idem { get; set; } = string.Empty;

    /// <summary>
    /// Whether the transfer is committed
    /// </summary>
    [JsonProperty("committed")]
    public bool Committed { get; set; }

    /// <summary>
    /// The transfer ID
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is an instant transfer
    /// </summary>
    [JsonProperty("instant")]
    public bool Instant { get; set; }

    /// <summary>
    /// Source information
    /// </summary>
    [JsonProperty("source")]
    public object? Source { get; set; }

    /// <summary>
    /// Target information
    /// </summary>
    [JsonProperty("target")]
    public object? Target { get; set; }

    /// <summary>
    /// When the transfer will be paid out
    /// </summary>
    [JsonProperty("payout_at")]
    public DateTime? PayoutAt { get; set; }

    /// <summary>
    /// The transfer status
    /// </summary>
    [JsonProperty("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// User reference
    /// </summary>
    [JsonProperty("user_reference")]
    public string UserReference { get; set; } = string.Empty;

    /// <summary>
    /// The transfer type
    /// </summary>
    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// When the transfer was created
    /// </summary>
    [JsonProperty("created_at")]
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// When the transfer was updated
    /// </summary>
    [JsonProperty("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// User warnings
    /// </summary>
    [JsonProperty("user_warnings")]
    public List<object> UserWarnings { get; set; } = new List<object>();

    /// <summary>
    /// Transfer fees
    /// </summary>
    [JsonProperty("fees")]
    public List<object> Fees { get; set; } = new List<object>();

    /// <summary>
    /// Total fee amount
    /// </summary>
    [JsonProperty("total_fee")]
    public Money? TotalFee { get; set; }

    /// <summary>
    /// Cancellation reason if the transfer was cancelled
    /// </summary>
    [JsonProperty("cancellation_reason")]
    public CancellationReason? CancellationReason { get; set; }

    /// <summary>
    /// Number of hold days
    /// </summary>
    [JsonProperty("hold_days")]
    public int HoldDays { get; set; }

    /// <summary>
    /// Next step information
    /// </summary>
    [JsonProperty("nextStep")]
    public object? NextStep { get; set; }

    /// <summary>
    /// Checkout URL if applicable
    /// </summary>
    [JsonProperty("checkout_url")]
    public string CheckoutUrl { get; set; } = string.Empty;

    /// <summary>
    /// Whether the transfer requires a completion step
    /// </summary>
    [JsonProperty("requires_completion_step")]
    public bool RequiresCompletionStep { get; set; }

    /// <summary>
    /// Transfer settings
    /// </summary>
    [JsonProperty("transfer_settings")]
    public object? TransferSettings { get; set; }
}

/// <summary>
/// Cancellation reason information
/// </summary>
public class CancellationReason
{
    /// <summary>
    /// The error message
    /// </summary>
    [JsonProperty("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The error code
    /// </summary>
    [JsonProperty("code")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// The error code (duplicate field in API response)
    /// </summary>
    [JsonProperty("error_code")]
    public string ErrorCode { get; set; } = string.Empty;
}

/// <summary>
/// Payment method information in the response
/// </summary>
public class PaymentMethodInfo
{
    /// <summary>
    /// The payment method ID
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The resource type
    /// </summary>
    [JsonProperty("resource")]
    public string Resource { get; set; } = string.Empty;

    /// <summary>
    /// The resource path
    /// </summary>
    [JsonProperty("resource_path")]
    public string ResourcePath { get; set; } = string.Empty;
}

/// <summary>
/// Transaction information in the response
/// </summary>
public class TransactionInfo
{
    /// <summary>
    /// The transaction ID
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The resource type
    /// </summary>
    [JsonProperty("resource")]
    public string Resource { get; set; } = string.Empty;

    /// <summary>
    /// The resource path
    /// </summary>
    [JsonProperty("resource_path")]
    public string ResourcePath { get; set; } = string.Empty;
}

/// <summary>
/// Filter criteria for querying transfers
/// </summary>
public class TransferFilter
{
    /// <summary>
    /// Filter by transfer type
    /// </summary>
    public TransferType? Type { get; set; }

    /// <summary>
    /// Filter by transfer status
    /// </summary>
    public TransferStatus? Status { get; set; }

    /// <summary>
    /// Filter by transfer method
    /// </summary>
    public TransferMethod? Method { get; set; }

    /// <summary>
    /// Start date for filtering
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// End date for filtering
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Maximum number of results to return
    /// </summary>
    public int? Limit { get; set; }

    /// <summary>
    /// Cursor for pagination
    /// </summary>
    public string? Cursor { get; set; }
}
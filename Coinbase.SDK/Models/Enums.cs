using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Coinbase.SDK.Models;

/// <summary>
/// Order side enumeration — Coinbase API uses UPPERCASE values
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum OrderSide
{
    [EnumMember(Value = "BUY")]
    Buy,

    [EnumMember(Value = "SELL")]
    Sell
}

/// <summary>
/// Order type enumeration — Coinbase API uses UPPERCASE values
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum OrderType
{
    [EnumMember(Value = "MARKET")]
    Market,

    [EnumMember(Value = "LIMIT")]
    Limit,

    [EnumMember(Value = "STOP")]
    Stop,

    [EnumMember(Value = "STOP_LIMIT")]
    StopLimit,

    [EnumMember(Value = "BRACKET")]
    Bracket,

    [EnumMember(Value = "UNKNOWN_ORDER_TYPE")]
    Unknown
}

/// <summary>
/// Order status enumeration — Coinbase API uses UPPERCASE values
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum OrderStatus
{
    [EnumMember(Value = "PENDING")]
    Pending,

    [EnumMember(Value = "OPEN")]
    Open,

    [EnumMember(Value = "FILLED")]
    Filled,

    [EnumMember(Value = "CANCELLED")]
    Cancelled,

    [EnumMember(Value = "EXPIRED")]
    Expired,

    [EnumMember(Value = "FAILED")]
    Failed,

    [EnumMember(Value = "QUEUED")]
    Queued,

    [EnumMember(Value = "CANCEL_QUEUED")]
    CancelQueued,

    [EnumMember(Value = "UNKNOWN_ORDER_STATUS")]
    Unknown
}

/// <summary>
/// Time in force enumeration
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum TimeInForce
{
    /// <summary>
    /// Good Till Cancelled
    /// </summary>
    GTC,

    /// <summary>
    /// Good Till Time
    /// </summary>
    GTT,

    /// <summary>
    /// Immediate Or Cancel
    /// </summary>
    IOC,

    /// <summary>
    /// Fill Or Kill
    /// </summary>
    FOK
}

/// <summary>
/// Granularity for candle data
/// </summary>
public enum Granularity
{
    /// <summary>
    /// One minute
    /// </summary>
    OneMinute = 60,

    /// <summary>
    /// Five minutes
    /// </summary>
    FiveMinutes = 300,

    /// <summary>
    /// Fifteen minutes
    /// </summary>
    FifteenMinutes = 900,

    /// <summary>
    /// One hour
    /// </summary>
    OneHour = 3600,

    /// <summary>
    /// Six hours
    /// </summary>
    SixHours = 21600,

    /// <summary>
    /// One day
    /// </summary>
    OneDay = 86400
}

/// <summary>
/// Transfer type enumeration
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum TransferType
{
    /// <summary>
    /// Deposit transfer
    /// </summary>
    Deposit,

    /// <summary>
    /// Withdrawal transfer
    /// </summary>
    Withdraw
}

/// <summary>
/// Transfer method enumeration
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum TransferMethod
{
    /// <summary>
    /// ACH bank account transfer
    /// </summary>
    [JsonProperty("ach_bank_account")]
    AchBankAccount,

    /// <summary>
    /// Wire transfer
    /// </summary>
    [JsonProperty("wire")]
    Wire,

    /// <summary>
    /// Credit card
    /// </summary>
    [JsonProperty("credit_card")]
    CreditCard,

    /// <summary>
    /// Debit card
    /// </summary>
    [JsonProperty("debit_card")]
    DebitCard,

    /// <summary>
    /// PayPal
    /// </summary>
    [JsonProperty("paypal_account")]
    PayPal,

    /// <summary>
    /// Bank account
    /// </summary>
    [JsonProperty("bank_account")]
    BankAccount
}

/// <summary>
/// Transfer status enumeration
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum TransferStatus
{
    /// <summary>
    /// Transfer is pending
    /// </summary>
    Pending,

    /// <summary>
    /// Transfer is completed
    /// </summary>
    Completed,

    /// <summary>
    /// Transfer was cancelled
    /// </summary>
    Cancelled,

    /// <summary>
    /// Transfer failed
    /// </summary>
    Failed,

    /// <summary>
    /// Transfer is processing
    /// </summary>
    Processing,

    /// <summary>
    /// Transfer is waiting for clearance
    /// </summary>
    [JsonProperty("waiting_for_clearing")]
    WaitingForClearing,

    /// <summary>
    /// Transfer is waiting for signature
    /// </summary>
    [JsonProperty("waiting_for_signature")]
    WaitingForSignature
}
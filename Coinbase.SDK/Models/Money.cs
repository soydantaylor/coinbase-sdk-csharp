using Newtonsoft.Json;

namespace Coinbase.SDK.Models;

/// <summary>
/// Represents a monetary value with currency
/// </summary>
public class Money
{
    /// <summary>
    /// The monetary value
    /// </summary>
    [JsonProperty("value")]
    public decimal Value { get; set; }

    /// <summary>
    /// The currency code
    /// </summary>
    [JsonProperty("currency")]
    public string Currency { get; set; } = string.Empty;
}
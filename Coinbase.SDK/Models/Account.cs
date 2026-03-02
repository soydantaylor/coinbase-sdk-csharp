using Newtonsoft.Json;

namespace Coinbase.SDK.Models;

/// <summary>
/// Represents a Coinbase account
/// </summary>
public class Account
{
    /// <summary>
    /// The unique account identifier
    /// </summary>
    [JsonProperty("uuid")]
    public string Uuid { get; set; } = string.Empty;

    /// <summary>
    /// The account name
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The account currency
    /// </summary>
    [JsonProperty("currency")]
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// The available balance in the account
    /// </summary>
    [JsonProperty("available_balance")]
    public Money AvailableBalance { get; set; } = new();

    /// <summary>
    /// Whether this is the default account
    /// </summary>
    [JsonProperty("default")]
    public bool IsDefault { get; set; }

    /// <summary>
    /// Whether the account is active
    /// </summary>
    [JsonProperty("active")]
    public bool Active { get; set; }

    /// <summary>
    /// When the account was created
    /// </summary>
    [JsonProperty("created_at")]
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// When the account was last updated
    /// </summary>
    [JsonProperty("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// When the account was deleted (if applicable)
    /// </summary>
    [JsonProperty("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// The account type
    /// </summary>
    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Whether the account is ready for use
    /// </summary>
    [JsonProperty("ready")]
    public bool Ready { get; set; }

    /// <summary>
    /// The amount on hold in the account
    /// </summary>
    [JsonProperty("hold")]
    public Money Hold { get; set; } = new();

    /// <summary>
    /// The retail portfolio ID associated with this account
    /// </summary>
    [JsonProperty("retail_portfolio_id")]
    public string RetailPortfolioId { get; set; } = string.Empty;

    /// <summary>
    /// The platform this account belongs to
    /// </summary>
    [JsonProperty("platform")]
    public string Platform { get; set; } = string.Empty;
}
using Newtonsoft.Json;

namespace Coinbase.SDK.Models;

/// <summary>
/// Represents a product order book
/// </summary>
public class ProductBook
{
    /// <summary>
    /// The product identifier
    /// </summary>
    [JsonProperty("product_id")]
    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// Buy orders in the book
    /// </summary>
    [JsonProperty("bids")]
    public IEnumerable<BookEntry> Bids { get; set; } = new List<BookEntry>();

    /// <summary>
    /// Sell orders in the book
    /// </summary>
    [JsonProperty("asks")]
    public IEnumerable<BookEntry> Asks { get; set; } = new List<BookEntry>();

    /// <summary>
    /// The timestamp of the book data
    /// </summary>
    [JsonProperty("time")]
    public DateTime Time { get; set; }
}

/// <summary>
/// Represents an entry in the order book
/// </summary>
public class BookEntry
{
    /// <summary>
    /// The price level
    /// </summary>
    [JsonProperty("price")]
    public decimal Price { get; set; }

    /// <summary>
    /// The size at this price level
    /// </summary>
    [JsonProperty("size")]
    public decimal Size { get; set; }

    /// <summary>
    /// The number of orders at this price level
    /// </summary>
    [JsonProperty("num_orders")]
    public int NumOrders { get; set; }
}

/// <summary>
/// Represents a trading product
/// </summary>
public class Product
{
    /// <summary>
    /// The product identifier
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The base currency
    /// </summary>
    [JsonProperty("base_currency")]
    public string BaseCurrency { get; set; } = string.Empty;

    /// <summary>
    /// The quote currency
    /// </summary>
    [JsonProperty("quote_currency")]
    public string QuoteCurrency { get; set; } = string.Empty;

    /// <summary>
    /// The display name
    /// </summary>
    [JsonProperty("display_name")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// The minimum order size
    /// </summary>
    [JsonProperty("base_min_size")]
    public decimal BaseMinSize { get; set; }

    /// <summary>
    /// The maximum order size
    /// </summary>
    [JsonProperty("base_max_size")]
    public decimal BaseMaxSize { get; set; }

    /// <summary>
    /// The price increment
    /// </summary>
    [JsonProperty("quote_increment")]
    public decimal QuoteIncrement { get; set; }

    /// <summary>
    /// Whether the product is active
    /// </summary>
    [JsonProperty("status")]
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Represents a price candle
/// </summary>
public class Candle
{
    /// <summary>
    /// The timestamp for this candle
    /// </summary>
    [JsonProperty("start")]
    public DateTime Start { get; set; }

    /// <summary>
    /// The lowest price during this period
    /// </summary>
    [JsonProperty("low")]
    public decimal Low { get; set; }

    /// <summary>
    /// The highest price during this period
    /// </summary>
    [JsonProperty("high")]
    public decimal High { get; set; }

    /// <summary>
    /// The opening price
    /// </summary>
    [JsonProperty("open")]
    public decimal Open { get; set; }

    /// <summary>
    /// The closing price
    /// </summary>
    [JsonProperty("close")]
    public decimal Close { get; set; }

    /// <summary>
    /// The volume during this period
    /// </summary>
    [JsonProperty("volume")]
    public decimal Volume { get; set; }
}
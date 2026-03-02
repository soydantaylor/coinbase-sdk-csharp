using Coinbase.SDK.Exceptions;
using Coinbase.SDK.Interfaces;
using Coinbase.SDK.Models;

namespace Coinbase.SDK.Services;

/// <summary>
/// Provides market data access
/// </summary>
public class MarketDataService : IMarketDataService
{
    private readonly IApiClient _apiClient;

    /// <summary>
    /// Initializes a new instance of the MarketDataService class
    /// </summary>
    /// <param name="apiClient">The API client for making HTTP requests</param>
    public MarketDataService(IApiClient apiClient)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
    }

    /// <summary>
    /// Retrieves the order book for a specific product
    /// </summary>
    /// <param name="productId">The product identifier (e.g., "BTC-USD")</param>
    /// <param name="level">The level of detail (1, 2, or 3)</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>The product order book</returns>
    public async Task<ProductBook> GetProductBookAsync(string productId, int level = 2, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("Product ID cannot be null or empty", nameof(productId));

        if (level < 1 || level > 3)
            throw new ArgumentOutOfRangeException(nameof(level), "Level must be between 1 and 3");

        try
        {
            var endpoint = $"/api/v3/brokerage/product_book?product_id={productId}&level={level}";
            var response = await _apiClient.GetAsync<ProductBookResponse>(endpoint, cancellationToken);
            
            if (response?.ProductBook == null)
                throw new CoinbaseApiException("Market data unavailable for the requested product", 404);

            return response.ProductBook;
        }
        catch (CoinbaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new CoinbaseApiException($"Failed to retrieve product book for {productId}", 500, ex.Message);
        }
    }

    /// <summary>
    /// Retrieves all available products
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A collection of all available products</returns>
    public async Task<IEnumerable<Product>> GetProductsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = "/api/v3/brokerage/products";
            var response = await _apiClient.GetAsync<ProductsResponse>(endpoint, cancellationToken);
            
            if (response?.Products == null)
                throw new CoinbaseApiException("Market data unavailable - no products found", 404);

            return response.Products;
        }
        catch (CoinbaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new CoinbaseApiException("Failed to retrieve products", 500, ex.Message);
        }
    }

    /// <summary>
    /// Retrieves a specific product by ID
    /// </summary>
    /// <param name="productId">The product identifier</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>The product details</returns>
    public async Task<Product> GetProductAsync(string productId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("Product ID cannot be null or empty", nameof(productId));

        try
        {
            var endpoint = $"/api/v3/brokerage/products/{productId}";
            var response = await _apiClient.GetAsync<ProductResponse>(endpoint, cancellationToken);
            
            if (response?.Product == null)
                throw new CoinbaseApiException($"Market data unavailable for product {productId}", 404);

            return response.Product;
        }
        catch (CoinbaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new CoinbaseApiException($"Failed to retrieve product {productId}", 500, ex.Message);
        }
    }

    /// <summary>
    /// Retrieves historical candle data for a product
    /// </summary>
    /// <param name="productId">The product identifier</param>
    /// <param name="start">The start date for historical data</param>
    /// <param name="end">The end date for historical data</param>
    /// <param name="granularity">The time granularity for candles</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A collection of candle data</returns>
    public async Task<IEnumerable<Candle>> GetCandlesAsync(string productId, DateTime start, DateTime end, Granularity granularity, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("Product ID cannot be null or empty", nameof(productId));

        // Date range validation
        if (end <= start)
            throw new ArgumentException("End date must be after start date", nameof(end));

        if (start > DateTime.UtcNow)
            throw new ArgumentException("Start date cannot be in the future", nameof(start));

        if (end > DateTime.UtcNow)
            throw new ArgumentException("End date cannot be in the future", nameof(end));

        try
        {
            var startUnix = ((DateTimeOffset)start).ToUnixTimeSeconds();
            var endUnix = ((DateTimeOffset)end).ToUnixTimeSeconds();
            var granularitySeconds = (int)granularity;

            var endpoint = $"/api/v3/brokerage/products/{productId}/candles?start={startUnix}&end={endUnix}&granularity={granularitySeconds}";
            var response = await _apiClient.GetAsync<CandlesResponse>(endpoint, cancellationToken);
            
            if (response?.Candles == null)
                throw new CoinbaseApiException($"Market data unavailable for product {productId} in the specified date range", 404);

            return response.Candles;
        }
        catch (CoinbaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new CoinbaseApiException($"Failed to retrieve candles for {productId}", 500, ex.Message);
        }
    }
}

/// <summary>
/// Response wrapper for product book API calls
/// </summary>
internal class ProductBookResponse
{
    [Newtonsoft.Json.JsonProperty("pricebook")]
    public ProductBook? ProductBook { get; set; }
}

/// <summary>
/// Response wrapper for products API calls
/// </summary>
internal class ProductsResponse
{
    [Newtonsoft.Json.JsonProperty("products")]
    public IEnumerable<Product>? Products { get; set; }
}

/// <summary>
/// Response wrapper for single product API calls
/// </summary>
internal class ProductResponse
{
    [Newtonsoft.Json.JsonProperty("product")]
    public Product? Product { get; set; }
}

/// <summary>
/// Response wrapper for candles API calls
/// </summary>
internal class CandlesResponse
{
    [Newtonsoft.Json.JsonProperty("candles")]
    public IEnumerable<Candle>? Candles { get; set; }
}
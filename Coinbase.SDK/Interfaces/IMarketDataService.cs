using Coinbase.SDK.Models;

namespace Coinbase.SDK.Interfaces;

/// <summary>
/// Provides market data access
/// </summary>
public interface IMarketDataService
{
    /// <summary>
    /// Retrieves the order book for a specific product
    /// </summary>
    /// <param name="productId">The product identifier (e.g., "BTC-USD")</param>
    /// <param name="level">The level of detail (1, 2, or 3)</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>The product order book</returns>
    Task<ProductBook> GetProductBookAsync(string productId, int level = 2, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all available products
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A collection of all available products</returns>
    Task<IEnumerable<Product>> GetProductsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a specific product by ID
    /// </summary>
    /// <param name="productId">The product identifier</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>The product details</returns>
    Task<Product> GetProductAsync(string productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves historical candle data for a product
    /// </summary>
    /// <param name="productId">The product identifier</param>
    /// <param name="start">The start date for historical data</param>
    /// <param name="end">The end date for historical data</param>
    /// <param name="granularity">The time granularity for candles</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A collection of candle data</returns>
    Task<IEnumerable<Candle>> GetCandlesAsync(string productId, DateTime start, DateTime end, Granularity granularity, CancellationToken cancellationToken = default);
}
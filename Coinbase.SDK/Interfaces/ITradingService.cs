using Coinbase.SDK.Models;
using Coinbase.SDK.Services;

namespace Coinbase.SDK.Interfaces;

/// <summary>
/// Handles trading operations
/// </summary>
public interface ITradingService
{
    /// <summary>
    /// Places a market order
    /// </summary>
    /// <param name="request">The market order request parameters</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>The created order details</returns>
    Task<Order> PlaceMarketOrderAsync(MarketOrderRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Places a limit order
    /// </summary>
    /// <param name="request">The limit order request parameters</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>The created order details</returns>
    Task<Order> PlaceLimitOrderAsync(LimitOrderRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a specific order by ID
    /// </summary>
    /// <param name="orderId">The unique order identifier</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>The order details</returns>
    Task<Order> GetOrderAsync(string orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels an existing order
    /// </summary>
    /// <param name="orderId">The unique order identifier to cancel</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>True if the order was successfully cancelled</returns>
    Task<bool> CancelOrderAsync(string orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Places a Bitcoin buy order using USD balance
    /// </summary>
    /// <param name="request">The Bitcoin buy order request parameters</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>The Bitcoin buy order response details</returns>
    Task<BitcoinBuyResponse> PlaceBitcoinBuyOrderAsync(BitcoinBuyRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves orders with optional filtering via /api/v3/brokerage/orders/historical/batch
    /// </summary>
    /// <param name="filter">Optional filter criteria for orders</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Orders result with pagination info</returns>
    Task<OrdersResult> GetOrdersAsync(OrderFilter? filter = null, CancellationToken cancellationToken = default);
}
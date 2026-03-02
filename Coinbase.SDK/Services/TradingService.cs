using System.Runtime.Serialization;
using Coinbase.SDK.Exceptions;
using Coinbase.SDK.Interfaces;
using Coinbase.SDK.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Coinbase.SDK.Services;

/// <summary>
/// Handles trading operations via Coinbase Advanced Trade API v3
/// </summary>
public class TradingService : ITradingService
{
    private readonly IApiClient _apiClient;

    public TradingService(IApiClient apiClient)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
    }

    /// <inheritdoc />
    public async Task<Order> PlaceMarketOrderAsync(MarketOrderRequest request, CancellationToken cancellationToken = default)
    {
        ValidateMarketOrderRequest(request);

        try
        {
            var response = await _apiClient.PostAsync<OrderResponse>("/api/v3/brokerage/orders", request, cancellationToken);

            if (response?.Order == null)
                throw new CoinbaseApiException("Failed to place market order: Invalid response from API", 500, "INVALID_RESPONSE");

            return response.Order;
        }
        catch (Exception ex) when (ex is not CoinbaseException)
        {
            throw new CoinbaseApiException($"Failed to place market order: {ex.Message}", 0, ex);
        }
    }

    /// <inheritdoc />
    public async Task<Order> PlaceLimitOrderAsync(LimitOrderRequest request, CancellationToken cancellationToken = default)
    {
        ValidateLimitOrderRequest(request);

        try
        {
            var response = await _apiClient.PostAsync<OrderResponse>("/api/v3/brokerage/orders", request, cancellationToken);

            if (response?.Order == null)
                throw new CoinbaseApiException("Failed to place limit order: Invalid response from API", 500, "INVALID_RESPONSE");

            return response.Order;
        }
        catch (Exception ex) when (ex is not CoinbaseException)
        {
            throw new CoinbaseApiException($"Failed to place limit order: {ex.Message}", 0, ex);
        }
    }

    /// <inheritdoc />
    public async Task<Order> GetOrderAsync(string orderId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(orderId))
            throw new ArgumentException("Order ID cannot be null or empty", nameof(orderId));

        try
        {
            var response = await _apiClient.GetAsync<OrderResponse>(
                $"/api/v3/brokerage/orders/historical/{orderId}", cancellationToken);

            if (response?.Order == null)
                throw new CoinbaseApiException($"Order with ID '{orderId}' not found", 404, "NOT_FOUND");

            return response.Order;
        }
        catch (Exception ex) when (ex is not CoinbaseException)
        {
            throw new CoinbaseApiException($"Failed to retrieve order '{orderId}': {ex.Message}", 0, ex);
        }
    }

    /// <inheritdoc />
    public async Task<bool> CancelOrderAsync(string orderId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(orderId))
            throw new ArgumentException("Order ID cannot be null or empty", nameof(orderId));

        try
        {
            // Coinbase v3 cancel is POST /orders/batch_cancel with order_ids array
            var request = new CancelOrdersRequest { OrderIds = new[] { orderId } };
            var response = await _apiClient.PostAsync<CancelOrdersResponse>(
                "/api/v3/brokerage/orders/batch_cancel", request, cancellationToken);
            return response?.Results?.Any(r => r.Success) ?? false;
        }
        catch (Exception ex) when (ex is not CoinbaseException)
        {
            throw new CoinbaseApiException($"Failed to cancel order '{orderId}': {ex.Message}", 0, ex);
        }
    }

    /// <inheritdoc />
    public async Task<BitcoinBuyResponse> PlaceBitcoinBuyOrderAsync(BitcoinBuyRequest request, CancellationToken cancellationToken = default)
    {
        ValidateBitcoinBuyRequest(request);

        try
        {
            var response = await _apiClient.PostAsync<BitcoinBuyResponse>("/api/v3/brokerage/orders", request, cancellationToken);

            if (response == null)
                throw new CoinbaseTradingException("Failed to place Bitcoin buy order: Invalid response from API", "INVALID_RESPONSE");

            return response;
        }
        catch (CoinbaseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new CoinbaseTradingException($"Failed to place Bitcoin buy order: {ex.Message}", ex, "API_ERROR");
        }
    }

    /// <inheritdoc />
    public async Task<OrdersResult> GetOrdersAsync(OrderFilter? filter = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = "/api/v3/brokerage/orders/historical/batch";
            var queryParams = BuildQueryParameters(filter);

            if (!string.IsNullOrEmpty(queryParams))
                endpoint += "?" + queryParams;

            var response = await _apiClient.GetAsync<OrdersResponse>(endpoint, cancellationToken);

            return new OrdersResult
            {
                Orders = response?.Orders ?? Enumerable.Empty<Order>(),
                HasNext = response?.HasNext ?? false,
                Cursor = response?.Cursor
            };
        }
        catch (Exception ex) when (ex is not CoinbaseException)
        {
            throw new CoinbaseApiException($"Failed to retrieve orders: {ex.Message}", 0, ex);
        }
    }

    private static void ValidateMarketOrderRequest(MarketOrderRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.ProductId))
            throw new ArgumentException("Product ID cannot be null or empty", nameof(request.ProductId));
        if (request.Size <= 0)
            throw new ArgumentException("Order size must be greater than zero", nameof(request.Size));
    }

    private static void ValidateLimitOrderRequest(LimitOrderRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.ProductId))
            throw new ArgumentException("Product ID cannot be null or empty", nameof(request.ProductId));
        if (request.Size <= 0)
            throw new ArgumentException("Order size must be greater than zero", nameof(request.Size));
        if (request.Price <= 0)
            throw new ArgumentException("Order price must be greater than zero", nameof(request.Price));
    }

    private static void ValidateBitcoinBuyRequest(BitcoinBuyRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
        if (request.UsdAmount <= 0)
            throw new CoinbaseValidationException("USD amount must be greater than zero");
        if (string.IsNullOrWhiteSpace(request.ProductId))
            throw new CoinbaseValidationException("Product ID cannot be null or empty");
        if (request.Side != "BUY")
            throw new CoinbaseValidationException("Bitcoin buy orders must have BUY side");
    }

    /// <summary>
    /// Builds query parameters matching Coinbase v3 API parameter names
    /// </summary>
    private static string BuildQueryParameters(OrderFilter? filter)
    {
        if (filter == null)
            return string.Empty;

        var parameters = new List<string>();

        if (!string.IsNullOrWhiteSpace(filter.ProductId))
            parameters.Add($"product_ids={Uri.EscapeDataString(filter.ProductId)}");

        if (filter.Statuses != null)
        {
            foreach (var status in filter.Statuses)
            {
                var memberAttr = typeof(OrderStatus).GetField(status.ToString())
                    ?.GetCustomAttributes(typeof(EnumMemberAttribute), false)
                    .FirstOrDefault() as EnumMemberAttribute;
                var value = memberAttr?.Value ?? status.ToString().ToUpperInvariant();
                parameters.Add($"order_status={Uri.EscapeDataString(value)}");
            }
        }

        if (filter.Side.HasValue)
        {
            var memberAttr = typeof(OrderSide).GetField(filter.Side.Value.ToString())
                ?.GetCustomAttributes(typeof(EnumMemberAttribute), false)
                .FirstOrDefault() as EnumMemberAttribute;
            var value = memberAttr?.Value ?? filter.Side.Value.ToString().ToUpperInvariant();
            parameters.Add($"order_side={Uri.EscapeDataString(value)}");
        }

        if (filter.OrderTypes != null)
        {
            foreach (var orderType in filter.OrderTypes)
            {
                var memberAttr = typeof(OrderType).GetField(orderType.ToString())
                    ?.GetCustomAttributes(typeof(EnumMemberAttribute), false)
                    .FirstOrDefault() as EnumMemberAttribute;
                var value = memberAttr?.Value ?? orderType.ToString().ToUpperInvariant();
                parameters.Add($"order_types={Uri.EscapeDataString(value)}");
            }
        }

        if (filter.StartDate.HasValue)
            parameters.Add($"start_date={filter.StartDate.Value:yyyy-MM-ddTHH:mm:ssZ}");

        if (filter.EndDate.HasValue)
            parameters.Add($"end_date={filter.EndDate.Value:yyyy-MM-ddTHH:mm:ssZ}");

        if (filter.Limit.HasValue)
            parameters.Add($"limit={filter.Limit.Value}");

        if (!string.IsNullOrWhiteSpace(filter.Cursor))
            parameters.Add($"cursor={Uri.EscapeDataString(filter.Cursor)}");

        if (!string.IsNullOrWhiteSpace(filter.ProductType))
            parameters.Add($"product_type={Uri.EscapeDataString(filter.ProductType)}");

        if (!string.IsNullOrWhiteSpace(filter.SortBy))
            parameters.Add($"sort_by={Uri.EscapeDataString(filter.SortBy)}");

        return string.Join("&", parameters);
    }
}

/// <summary>
/// Response model for GET /api/v3/brokerage/orders/historical/{order_id}
/// </summary>
public class OrderResponse
{
    [JsonProperty("order")]
    public Order? Order { get; set; }
}

/// <summary>
/// Response model for GET /api/v3/brokerage/orders/historical/batch
/// </summary>
public class OrdersResponse
{
    [JsonProperty("orders")]
    public IEnumerable<Order> Orders { get; set; } = Enumerable.Empty<Order>();

    [JsonProperty("has_next")]
    public bool HasNext { get; set; }

    [JsonProperty("cursor")]
    public string? Cursor { get; set; }
}

/// <summary>
/// Result wrapper for order listing with pagination info
/// </summary>
public class OrdersResult
{
    public IEnumerable<Order> Orders { get; set; } = Enumerable.Empty<Order>();
    public bool HasNext { get; set; }
    public string? Cursor { get; set; }
}

/// <summary>
/// Request model for POST /api/v3/brokerage/orders/batch_cancel
/// </summary>
public class CancelOrdersRequest
{
    [JsonProperty("order_ids")]
    public string[] OrderIds { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Response model for batch cancel
/// </summary>
public class CancelOrdersResponse
{
    [JsonProperty("results")]
    public CancelOrderResult[]? Results { get; set; }
}

public class CancelOrderResult
{
    [JsonProperty("success")]
    public bool Success { get; set; }

    [JsonProperty("failure_reason")]
    public string? FailureReason { get; set; }

    [JsonProperty("order_id")]
    public string? OrderId { get; set; }
}

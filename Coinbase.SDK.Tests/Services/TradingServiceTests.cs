using Coinbase.SDK.Exceptions;
using Coinbase.SDK.Interfaces;
using Coinbase.SDK.Models;
using Coinbase.SDK.Services;
using Moq;
using Xunit;

namespace Coinbase.SDK.Tests.Services;

/// <summary>
/// Unit tests for TradingService
/// </summary>
public class TradingServiceTests
{
    private readonly Mock<IApiClient> _mockApiClient;
    private readonly TradingService _tradingService;

    public TradingServiceTests()
    {
        _mockApiClient = new Mock<IApiClient>();
        _tradingService = new TradingService(_mockApiClient.Object);
    }

    [Fact]
    public async Task PlaceMarketOrderAsync_WithValidRequest_ReturnsOrder()
    {
        // Arrange
        var request = new MarketOrderRequest
        {
            ProductId = "BTC-USD",
            Side = OrderSide.Buy,
            Size = 0.01m
        };

        var expectedOrder = new Order
        {
            OrderId = "test-order-id",
            ProductId = "BTC-USD",
            Side = OrderSide.Buy,
            Type = OrderType.Market,
            FilledSize = "0.01",
            Status = OrderStatus.Pending,
            CreatedTime = DateTime.UtcNow
        };

        var response = new OrderResponse { Order = expectedOrder };
        _mockApiClient.Setup(x => x.PostAsync<OrderResponse>("/api/v3/brokerage/orders", request, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(response);

        // Act
        var result = await _tradingService.PlaceMarketOrderAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedOrder.OrderId, result.OrderId);
        Assert.Equal(expectedOrder.ProductId, result.ProductId);
        Assert.Equal(expectedOrder.Side, result.Side);
        Assert.Equal(expectedOrder.FilledSize, result.FilledSize);
    }

    [Fact]
    public async Task PlaceLimitOrderAsync_WithValidRequest_ReturnsOrder()
    {
        // Arrange
        var request = new LimitOrderRequest
        {
            ProductId = "BTC-USD",
            Side = OrderSide.Buy,
            Size = 0.01m,
            Price = 50000m
        };

        var expectedOrder = new Order
        {
            OrderId = "test-order-id",
            ProductId = "BTC-USD",
            Side = OrderSide.Buy,
            Type = OrderType.Limit,
            FilledSize = "0.01",
            AverageFilledPrice = "50000",
            Status = OrderStatus.Open,
            CreatedTime = DateTime.UtcNow
        };

        var response = new OrderResponse { Order = expectedOrder };
        _mockApiClient.Setup(x => x.PostAsync<OrderResponse>("/api/v3/brokerage/orders", request, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(response);

        // Act
        var result = await _tradingService.PlaceLimitOrderAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedOrder.OrderId, result.OrderId);
        Assert.Equal(expectedOrder.ProductId, result.ProductId);
        Assert.Equal(expectedOrder.Side, result.Side);
        Assert.Equal(expectedOrder.FilledSize, result.FilledSize);
        Assert.Equal(expectedOrder.AverageFilledPrice, result.AverageFilledPrice);
    }

    [Fact]
    public async Task GetOrderAsync_WithValidOrderId_ReturnsOrder()
    {
        // Arrange
        var orderId = "test-order-id";
        var expectedOrder = new Order
        {
            OrderId = orderId,
            ProductId = "BTC-USD",
            Side = OrderSide.Buy,
            Type = OrderType.Limit,
            FilledSize = "0.01",
            AverageFilledPrice = "50000",
            Status = OrderStatus.Open,
            CreatedTime = DateTime.UtcNow
        };

        var response = new OrderResponse { Order = expectedOrder };
        _mockApiClient.Setup(x => x.GetAsync<OrderResponse>($"/api/v3/brokerage/orders/historical/{orderId}", It.IsAny<CancellationToken>()))
                     .ReturnsAsync(response);

        // Act
        var result = await _tradingService.GetOrderAsync(orderId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedOrder.OrderId, result.OrderId);
        Assert.Equal(expectedOrder.ProductId, result.ProductId);
    }

    [Fact]
    public async Task CancelOrderAsync_WithValidOrderId_ReturnsTrue()
    {
        // Arrange
        var orderId = "test-order-id";
        var response = new CancelOrdersResponse
        {
            Results = new[] { new CancelOrderResult { Success = true, OrderId = orderId } }
        };
        _mockApiClient.Setup(x => x.PostAsync<CancelOrdersResponse>(
                         "/api/v3/brokerage/orders/batch_cancel",
                         It.IsAny<CancelOrdersRequest>(),
                         It.IsAny<CancellationToken>()))
                     .ReturnsAsync(response);

        // Act
        var result = await _tradingService.CancelOrderAsync(orderId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task GetOrdersAsync_WithoutFilter_ReturnsOrders()
    {
        // Arrange
        var expectedOrders = new List<Order>
        {
            new Order { OrderId = "order-1", ProductId = "BTC-USD", Side = OrderSide.Buy },
            new Order { OrderId = "order-2", ProductId = "ETH-USD", Side = OrderSide.Sell }
        };

        var response = new OrdersResponse { Orders = expectedOrders };
        _mockApiClient.Setup(x => x.GetAsync<OrdersResponse>("/api/v3/brokerage/orders/historical/batch", It.IsAny<CancellationToken>()))
                     .ReturnsAsync(response);

        // Act
        var result = await _tradingService.GetOrdersAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Orders.Count());
        Assert.Contains(result.Orders, o => o.OrderId == "order-1");
        Assert.Contains(result.Orders, o => o.OrderId == "order-2");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void PlaceMarketOrderAsync_WithInvalidProductId_ThrowsArgumentException(string productId)
    {
        // Arrange
        var request = new MarketOrderRequest
        {
            ProductId = productId,
            Side = OrderSide.Buy,
            Size = 0.01m
        };

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => _tradingService.PlaceMarketOrderAsync(request));
    }

    [Fact]
    public void PlaceMarketOrderAsync_WithZeroSize_ThrowsArgumentException()
    {
        // Arrange
        var request = new MarketOrderRequest
        {
            ProductId = "BTC-USD",
            Side = OrderSide.Buy,
            Size = 0m
        };

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => _tradingService.PlaceMarketOrderAsync(request));
    }

    [Fact]
    public void PlaceLimitOrderAsync_WithZeroPrice_ThrowsArgumentException()
    {
        // Arrange
        var request = new LimitOrderRequest
        {
            ProductId = "BTC-USD",
            Side = OrderSide.Buy,
            Size = 0.01m,
            Price = 0m
        };

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => _tradingService.PlaceLimitOrderAsync(request));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetOrderAsync_WithInvalidOrderId_ThrowsArgumentException(string orderId)
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => _tradingService.GetOrderAsync(orderId));
    }

    [Fact]
    public async Task PlaceBitcoinBuyOrderAsync_WithValidRequest_ReturnsBitcoinBuyResponse()
    {
        // Arrange
        var request = new BitcoinBuyRequest
        {
            ProductId = "BTC-USD"
        };
        request.SetUsdAmount(100m);

        var expectedResponse = new BitcoinBuyResponse
        {
            Success = true,
            OrderId = "test-bitcoin-order-id",
            SuccessResponse = new OrderSuccessResponse
            {
                OrderId = "test-bitcoin-order-id",
                ProductId = "BTC-USD",
                Side = "BUY"
            }
        };

        _mockApiClient.Setup(x => x.PostAsync<BitcoinBuyResponse>("/api/v3/brokerage/orders", request, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(expectedResponse);

        // Act
        var result = await _tradingService.PlaceBitcoinBuyOrderAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test-bitcoin-order-id", result.OrderId);
        Assert.True(result.Success);
        Assert.Equal("PENDING", result.Status);
    }

    [Fact]
    public void PlaceBitcoinBuyOrderAsync_WithZeroAmount_ThrowsCoinbaseValidationException()
    {
        // Arrange
        var request = new BitcoinBuyRequest
        {
            ProductId = "BTC-USD"
        };
        // UsdAmount defaults to 0 (QuoteSize is "0.00")

        // Act & Assert
        Assert.ThrowsAsync<CoinbaseValidationException>(() => _tradingService.PlaceBitcoinBuyOrderAsync(request));
    }

    [Fact]
    public void PlaceBitcoinBuyOrderAsync_WithNegativeAmount_ThrowsCoinbaseValidationException()
    {
        // Arrange
        var request = new BitcoinBuyRequest
        {
            ProductId = "BTC-USD"
        };
        request.SetUsdAmount(-10m);

        // Act & Assert
        Assert.ThrowsAsync<CoinbaseValidationException>(() => _tradingService.PlaceBitcoinBuyOrderAsync(request));
    }

    [Fact]
    public void PlaceBitcoinBuyOrderAsync_WithSellSide_ThrowsCoinbaseValidationException()
    {
        // Arrange
        var request = new BitcoinBuyRequest
        {
            ProductId = "BTC-USD",
            Side = "SELL" // Invalid for Bitcoin buy order
        };
        request.SetUsdAmount(100m);

        // Act & Assert
        Assert.ThrowsAsync<CoinbaseValidationException>(() => _tradingService.PlaceBitcoinBuyOrderAsync(request));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void PlaceBitcoinBuyOrderAsync_WithInvalidProductId_ThrowsCoinbaseValidationException(string productId)
    {
        // Arrange
        var request = new BitcoinBuyRequest
        {
            ProductId = productId
        };
        request.SetUsdAmount(100m);

        // Act & Assert
        Assert.ThrowsAsync<CoinbaseValidationException>(() => _tradingService.PlaceBitcoinBuyOrderAsync(request));
    }

    [Fact]
    public async Task PlaceBitcoinBuyOrderAsync_WithApiException_ThrowsCoinbaseTradingException()
    {
        // Arrange
        var request = new BitcoinBuyRequest
        {
            ProductId = "BTC-USD"
        };
        request.SetUsdAmount(100m);

        _mockApiClient.Setup(x => x.PostAsync<BitcoinBuyResponse>("/api/v3/brokerage/orders", request, It.IsAny<CancellationToken>()))
                     .ThrowsAsync(new HttpRequestException("Network error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CoinbaseTradingException>(() => _tradingService.PlaceBitcoinBuyOrderAsync(request));
        Assert.Contains("Failed to place Bitcoin buy order", exception.Message);
        Assert.Contains("Network error", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CancelOrderAsync_WithInvalidOrderId_ThrowsArgumentException(string orderId)
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => _tradingService.CancelOrderAsync(orderId));
    }

    [Fact]
    public async Task PreviewBitcoinBuyAsync_WithValidRequest_ReturnsPreviewResponse()
    {
        // Arrange
        var request = new BitcoinBuyPreviewRequest
        {
            ProductId = "BTC-USD"
        };
        request.SetUsdAmount(100m);

        var expectedResponse = new BitcoinBuyPreviewResponse
        {
            OrderTotal = "100.60",
            CommissionTotal = "0.60",
            QuoteSize = "100.00",
            BaseSize = "0.00105",
            PreviewId = "preview-123"
        };

        _mockApiClient.Setup(x => x.PostAsync<BitcoinBuyPreviewResponse>("/api/v3/brokerage/orders/preview", request, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(expectedResponse);

        // Act
        var result = await _tradingService.PreviewBitcoinBuyAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0.60m, result.CommissionTotalDecimal);
        Assert.Equal(100.60m, result.OrderTotalDecimal);
        Assert.Equal("preview-123", result.PreviewId);
    }

    [Fact]
    public void PreviewBitcoinBuyAsync_WithZeroAmount_ThrowsCoinbaseValidationException()
    {
        // Arrange
        var request = new BitcoinBuyPreviewRequest
        {
            ProductId = "BTC-USD"
        };

        // Act & Assert
        Assert.ThrowsAsync<CoinbaseValidationException>(() => _tradingService.PreviewBitcoinBuyAsync(request));
    }

    [Fact]
    public void PreviewBitcoinBuyAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentNullException>(() => _tradingService.PreviewBitcoinBuyAsync(null!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void PreviewBitcoinBuyAsync_WithInvalidProductId_ThrowsCoinbaseValidationException(string productId)
    {
        // Arrange
        var request = new BitcoinBuyPreviewRequest
        {
            ProductId = productId
        };
        request.SetUsdAmount(100m);

        // Act & Assert
        Assert.ThrowsAsync<CoinbaseValidationException>(() => _tradingService.PreviewBitcoinBuyAsync(request));
    }

    [Fact]
    public void PreviewBitcoinBuyAsync_WithSellSide_ThrowsCoinbaseValidationException()
    {
        // Arrange
        var request = new BitcoinBuyPreviewRequest
        {
            ProductId = "BTC-USD",
            Side = "SELL"
        };
        request.SetUsdAmount(100m);

        // Act & Assert
        Assert.ThrowsAsync<CoinbaseValidationException>(() => _tradingService.PreviewBitcoinBuyAsync(request));
    }

    [Fact]
    public async Task PreviewBitcoinBuyAsync_WithApiException_ThrowsCoinbaseTradingException()
    {
        // Arrange
        var request = new BitcoinBuyPreviewRequest
        {
            ProductId = "BTC-USD"
        };
        request.SetUsdAmount(100m);

        _mockApiClient.Setup(x => x.PostAsync<BitcoinBuyPreviewResponse>("/api/v3/brokerage/orders/preview", request, It.IsAny<CancellationToken>()))
                     .ThrowsAsync(new HttpRequestException("Network error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CoinbaseTradingException>(() => _tradingService.PreviewBitcoinBuyAsync(request));
        Assert.Contains("Failed to preview Bitcoin buy order", exception.Message);
        Assert.Contains("Network error", exception.Message);
    }
}
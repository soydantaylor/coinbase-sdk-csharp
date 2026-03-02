using Coinbase.SDK.Interfaces;
using Coinbase.SDK.Models;
using Coinbase.SDK.Services;
using Coinbase.SDK.Exceptions;
using Moq;
using Xunit;

namespace Coinbase.SDK.Tests.Services;

public class TransferServiceTests
{
    private readonly Mock<IApiClient> _mockApiClient;
    private readonly TransferService _transferService;

    public TransferServiceTests()
    {
        _mockApiClient = new Mock<IApiClient>();
        _transferService = new TransferService(_mockApiClient.Object);
    }

    [Fact]
    public async Task DepositAsync_WithValidRequest_ReturnsTransfer()
    {
        // Arrange
        var request = new DepositRequest
        {
            Amount = 100m,
            Currency = "USD",
            PaymentMethodId = "payment-method-id"
        };

        var expectedTransfer = new Transfer
        {
            Id = "transfer-id",
            Type = TransferType.Deposit,
            Amount = new Money { Value = 100m, Currency = "USD" },
            Status = TransferStatus.Pending
        };

        _mockApiClient.Setup(x => x.PostAsync<Transfer>("/v2/accounts/primary/deposits", request, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(expectedTransfer);

        // Act
        var result = await _transferService.DepositAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("transfer-id", result.Id);
        Assert.Equal(TransferType.Deposit, result.Type);
        Assert.Equal(100m, result.Amount.Value);
        Assert.Equal("USD", result.Amount.Currency);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task DepositAsync_WithInvalidPaymentMethodId_ThrowsArgumentException(string paymentMethodId)
    {
        // Arrange
        var request = new DepositRequest
        {
            Amount = 100m,
            Currency = "USD",
            PaymentMethodId = paymentMethodId
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _transferService.DepositAsync(request));
    }

    [Fact]
    public async Task DepositAsync_WithZeroAmount_ThrowsArgumentException()
    {
        // Arrange
        var request = new DepositRequest
        {
            Amount = 0m,
            Currency = "USD",
            PaymentMethodId = "payment-method-id"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _transferService.DepositAsync(request));
    }

    [Fact]
    public async Task WithdrawAsync_WithValidRequest_ReturnsTransfer()
    {
        // Arrange
        var request = new WithdrawalRequest
        {
            Amount = 50m,
            Currency = "USD",
            PaymentMethodId = "payment-method-id"
        };

        var expectedTransfer = new Transfer
        {
            Id = "transfer-id",
            Type = TransferType.Withdraw,
            Amount = new Money { Value = 50m, Currency = "USD" },
            Status = TransferStatus.Pending
        };

        _mockApiClient.Setup(x => x.PostAsync<Transfer>("/v2/accounts/primary/withdrawals", request, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(expectedTransfer);

        // Act
        var result = await _transferService.WithdrawAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("transfer-id", result.Id);
        Assert.Equal(TransferType.Withdraw, result.Type);
        Assert.Equal(50m, result.Amount.Value);
    }

    [Fact]
    public async Task GetTransferAsync_WithValidId_ReturnsTransfer()
    {
        // Arrange
        var transferId = "transfer-id";
        var expectedTransfer = new Transfer
        {
            Id = transferId,
            Type = TransferType.Deposit,
            Status = TransferStatus.Completed
        };

        _mockApiClient.Setup(x => x.GetAsync<Transfer>($"/v2/accounts/primary/transactions/{transferId}", It.IsAny<CancellationToken>()))
                     .ReturnsAsync(expectedTransfer);

        // Act
        var result = await _transferService.GetTransferAsync(transferId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(transferId, result.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetTransferAsync_WithInvalidId_ThrowsArgumentException(string transferId)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _transferService.GetTransferAsync(transferId));
    }

    [Fact]
    public async Task GetPaymentMethodsAsync_ReturnsPaymentMethods()
    {
        // Arrange
        var expectedPaymentMethods = new List<PaymentMethod>
        {
            new PaymentMethod { Id = "pm1", Type = "ach_bank_account", Name = "Bank Account" },
            new PaymentMethod { Id = "pm2", Type = "credit_card", Name = "Credit Card" }
        };

        var response = new PaymentMethodListResponse { Data = expectedPaymentMethods };
        _mockApiClient.Setup(x => x.GetAsync<PaymentMethodListResponse>("/api/v3/brokerage/payment_methods", It.IsAny<CancellationToken>()))
                     .ReturnsAsync(response);

        // Act
        var result = await _transferService.GetPaymentMethodsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, pm => pm.Id == "pm1");
        Assert.Contains(result, pm => pm.Id == "pm2");
    }

    [Fact]
    public async Task CommitTransferAsync_WithValidId_ReturnsTransfer()
    {
        // Arrange
        var transferId = "transfer-id";
        var expectedTransfer = new Transfer
        {
            Id = transferId,
            Status = TransferStatus.Processing
        };

        _mockApiClient.Setup(x => x.PostAsync<Transfer>($"/v2/accounts/primary/deposits/{transferId}/commit", It.IsAny<object>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(expectedTransfer);

        // Act
        var result = await _transferService.CommitTransferAsync(transferId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(transferId, result.Id);
        Assert.Equal(TransferStatus.Processing, result.Status);
    }

    [Fact]
    public async Task InitiateAchDepositAsync_WithValidRequest_ReturnsAchDepositResponse()
    {
        // Arrange
        var request = new AchDepositRequest
        {
            Amount = "100.00",
            Currency = "USD",
            PaymentMethodId = "payment-method-id"
        };

        var expectedResponse = new AchDepositResponse
        {
            Transfer = new ApiTransferDetails
            {
                Id = "ach-transaction-id",
                Amount = new Money { Value = 100m, Currency = "USD" },
                Status = "pending",
                CreatedAt = DateTime.UtcNow,
                Type = "deposit",
                Committed = false
            }
        };

        _mockApiClient.Setup(x => x.PostAsync<AchDepositResponse>("/v2/accounts/primary/deposits", request, It.IsAny<CancellationToken>()))
                     .ReturnsAsync(expectedResponse);

        // Act
        var result = await _transferService.InitiateAchDepositAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Transfer);
        Assert.Equal("ach-transaction-id", result.Transfer.Id);
        Assert.Equal(100m, result.Transfer.Amount?.Value);
        Assert.Equal("USD", result.Transfer.Amount?.Currency);
        Assert.Equal("pending", result.Transfer.Status);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task InitiateAchDepositAsync_WithInvalidPaymentMethodId_ThrowsCoinbaseValidationException(string paymentMethodId)
    {
        // Arrange
        var request = new AchDepositRequest
        {
            Amount = "100.00",
            Currency = "USD",
            PaymentMethodId = paymentMethodId
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CoinbaseValidationException>(() => _transferService.InitiateAchDepositAsync(request));
        Assert.Contains("Payment method ID is required", exception.Message);
    }

    [Fact]
    public async Task InitiateAchDepositAsync_WithZeroAmount_ThrowsCoinbaseValidationException()
    {
        // Arrange
        var request = new AchDepositRequest
        {
            Amount = "0.00",
            Currency = "USD",
            PaymentMethodId = "payment-method-id"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CoinbaseValidationException>(() => _transferService.InitiateAchDepositAsync(request));
        Assert.Contains("Amount must be a valid number greater than zero", exception.Message);
    }

    [Fact]
    public async Task InitiateAchDepositAsync_WithNegativeAmount_ThrowsCoinbaseValidationException()
    {
        // Arrange
        var request = new AchDepositRequest
        {
            Amount = "-50.00",
            Currency = "USD",
            PaymentMethodId = "payment-method-id"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CoinbaseValidationException>(() => _transferService.InitiateAchDepositAsync(request));
        Assert.Contains("Amount must be a valid number greater than zero", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task InitiateAchDepositAsync_WithInvalidCurrency_ThrowsCoinbaseValidationException(string currency)
    {
        // Arrange
        var request = new AchDepositRequest
        {
            Amount = "100.00",
            Currency = currency,
            PaymentMethodId = "payment-method-id"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CoinbaseValidationException>(() => _transferService.InitiateAchDepositAsync(request));
        Assert.Contains("Currency is required", exception.Message);
    }

    [Fact]
    public async Task InitiateAchDepositAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _transferService.InitiateAchDepositAsync(null));
    }

    [Fact]
    public async Task InitiateAchDepositAsync_WhenApiThrowsCoinbaseApiException_ThrowsCoinbaseTransferException()
    {
        // Arrange
        var request = new AchDepositRequest
        {
            Amount = "100.00",
            Currency = "USD",
            PaymentMethodId = "payment-method-id"
        };

        var apiException = new CoinbaseApiException("Insufficient funds", 400, "INSUFFICIENT_FUNDS");
        _mockApiClient.Setup(x => x.PostAsync<AchDepositResponse>("/v2/accounts/primary/deposits", request, It.IsAny<CancellationToken>()))
                     .ThrowsAsync(apiException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CoinbaseTransferException>(() => _transferService.InitiateAchDepositAsync(request));
        Assert.Contains("ACH deposit API call failed", exception.Message);
        Assert.Contains("Insufficient funds", exception.Message);
        Assert.Equal("INSUFFICIENT_FUNDS", exception.ErrorCode);
        Assert.Equal(apiException, exception.InnerException);
    }

    [Fact]
    public async Task InitiateAchDepositAsync_WhenApiThrowsCoinbaseNetworkException_ThrowsCoinbaseTransferException()
    {
        // Arrange
        var request = new AchDepositRequest
        {
            Amount = "100.00",
            Currency = "USD",
            PaymentMethodId = "payment-method-id"
        };

        var networkException = new CoinbaseNetworkException("Network timeout", new TimeoutException());
        _mockApiClient.Setup(x => x.PostAsync<AchDepositResponse>("/v2/accounts/primary/deposits", request, It.IsAny<CancellationToken>()))
                     .ThrowsAsync(networkException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CoinbaseTransferException>(() => _transferService.InitiateAchDepositAsync(request));
        Assert.Contains("Network error during ACH deposit", exception.Message);
        Assert.Contains("Network timeout", exception.Message);
        Assert.Equal(networkException, exception.InnerException);
    }
}
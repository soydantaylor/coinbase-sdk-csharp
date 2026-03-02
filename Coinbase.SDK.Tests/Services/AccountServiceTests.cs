using Coinbase.SDK.Exceptions;
using Coinbase.SDK.Interfaces;
using Coinbase.SDK.Models;
using Coinbase.SDK.Services;
using Moq;
using Xunit;

namespace Coinbase.SDK.Tests.Services;

/// <summary>
/// Unit tests for AccountService
/// </summary>
public class AccountServiceTests
{
    private readonly Mock<IApiClient> _mockApiClient;
    private readonly AccountService _accountService;

    public AccountServiceTests()
    {
        _mockApiClient = new Mock<IApiClient>();
        _accountService = new AccountService(_mockApiClient.Object);
    }

    [Fact]
    public async Task GetAccountsAsync_ReturnsAccounts_WhenApiCallSucceeds()
    {
        // Arrange
        var expectedAccounts = new List<Account>
        {
            new Account { Uuid = "account1", Name = "BTC Account", Currency = "BTC" },
            new Account { Uuid = "account2", Name = "USD Account", Currency = "USD" }
        };
        
        var response = new AccountsResponse { Accounts = expectedAccounts };
        _mockApiClient.Setup(x => x.GetAsync<AccountsResponse>("/api/v3/brokerage/accounts", It.IsAny<CancellationToken>()))
                     .ReturnsAsync(response);

        // Act
        var result = await _accountService.GetAccountsAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Equal("account1", result.First().Uuid);
        Assert.Equal("BTC Account", result.First().Name);
    }

    [Fact]
    public async Task GetAccountAsync_ReturnsAccount_WhenAccountExists()
    {
        // Arrange
        var expectedAccount = new Account { Uuid = "account1", Name = "BTC Account", Currency = "BTC" };
        var response = new AccountResponse { Account = expectedAccount };
        
        _mockApiClient.Setup(x => x.GetAsync<AccountResponse>("/api/v3/brokerage/accounts/account1", It.IsAny<CancellationToken>()))
                     .ReturnsAsync(response);

        // Act
        var result = await _accountService.GetAccountAsync("account1");

        // Assert
        Assert.Equal("account1", result.Uuid);
        Assert.Equal("BTC Account", result.Name);
    }

    [Fact]
    public async Task GetAccountAsync_ThrowsArgumentException_WhenAccountIdIsEmpty()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _accountService.GetAccountAsync(""));
        await Assert.ThrowsAsync<ArgumentException>(() => _accountService.GetAccountAsync(null!));
    }

    [Fact]
    public async Task GetAccountByNameAsync_ReturnsAccount_WhenAccountExists()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account { Uuid = "account1", Name = "BTC Account", Currency = "BTC" },
            new Account { Uuid = "account2", Name = "USD Account", Currency = "USD" }
        };
        
        var response = new AccountsResponse { Accounts = accounts };
        _mockApiClient.Setup(x => x.GetAsync<AccountsResponse>("/api/v3/brokerage/accounts", It.IsAny<CancellationToken>()))
                     .ReturnsAsync(response);

        // Act
        var result = await _accountService.GetAccountByNameAsync("BTC Account");

        // Assert
        Assert.Equal("account1", result.Uuid);
        Assert.Equal("BTC Account", result.Name);
    }

    [Fact]
    public async Task GetAccountByNameAsync_ThrowsException_WhenAccountNotFound()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account { Uuid = "account1", Name = "BTC Account", Currency = "BTC" }
        };
        
        var response = new AccountsResponse { Accounts = accounts };
        _mockApiClient.Setup(x => x.GetAsync<AccountsResponse>("/api/v3/brokerage/accounts", It.IsAny<CancellationToken>()))
                     .ReturnsAsync(response);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CoinbaseApiException>(() => _accountService.GetAccountByNameAsync("NonExistent Account"));
        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public void FilterAccounts_FiltersCorrectly_ByCurrency()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account { Uuid = "account1", Currency = "BTC", Type = "crypto", Active = true },
            new Account { Uuid = "account2", Currency = "USD", Type = "fiat", Active = true },
            new Account { Uuid = "account3", Currency = "BTC", Type = "crypto", Active = false }
        };

        // Act
        var result = AccountService.FilterAccounts(accounts, currency: "BTC");

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, account => Assert.Equal("BTC", account.Currency));
    }

    [Fact]
    public void FilterAccounts_FiltersCorrectly_ByActiveStatus()
    {
        // Arrange
        var accounts = new List<Account>
        {
            new Account { Uuid = "account1", Currency = "BTC", Type = "crypto", Active = true },
            new Account { Uuid = "account2", Currency = "USD", Type = "fiat", Active = true },
            new Account { Uuid = "account3", Currency = "BTC", Type = "crypto", Active = false }
        };

        // Act
        var result = AccountService.FilterAccounts(accounts, activeOnly: true);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, account => Assert.True(account.Active));
    }

    [Fact]
    public async Task GetPaymentMethodsAsync_ReturnsPaymentMethods_WhenApiCallSucceeds()
    {
        // Arrange
        var expectedPaymentMethods = new List<PaymentMethod>
        {
            new PaymentMethod { Id = "pm1", Type = "ach_bank_account", Name = "Bank Account", Currency = "USD" },
            new PaymentMethod { Id = "pm2", Type = "debit_card", Name = "Debit Card", Currency = "USD" }
        };
        
        var response = new PaymentMethodsResponse { PaymentMethods = expectedPaymentMethods };
        _mockApiClient.Setup(x => x.GetAsync<PaymentMethodsResponse>("/api/v3/brokerage/payment_methods", It.IsAny<CancellationToken>()))
                     .ReturnsAsync(response);

        // Act
        var result = await _accountService.GetPaymentMethodsAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Equal("pm1", result.First().Id);
        Assert.Equal("ach_bank_account", result.First().Type);
    }

    [Fact]
    public async Task GetPaymentMethodsAsync_ReturnsEmptyCollection_WhenResponseIsNull()
    {
        // Arrange
        _mockApiClient.Setup(x => x.GetAsync<PaymentMethodsResponse>("/api/v3/brokerage/payment_methods", It.IsAny<CancellationToken>()))
                     .ReturnsAsync((PaymentMethodsResponse?)null);

        // Act
        var result = await _accountService.GetPaymentMethodsAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetPaymentMethodAsync_ReturnsPaymentMethod_WhenPaymentMethodExists()
    {
        // Arrange
        var paymentMethods = new List<PaymentMethod>
        {
            new PaymentMethod { Id = "pm1", Type = "ach_bank_account", Name = "Bank Account", Currency = "USD" },
            new PaymentMethod { Id = "pm2", Type = "debit_card", Name = "Debit Card", Currency = "USD" }
        };
        
        var response = new PaymentMethodsResponse { PaymentMethods = paymentMethods };
        _mockApiClient.Setup(x => x.GetAsync<PaymentMethodsResponse>("/api/v3/brokerage/payment_methods", It.IsAny<CancellationToken>()))
                     .ReturnsAsync(response);

        // Act
        var result = await _accountService.GetPaymentMethodAsync("pm1");

        // Assert
        Assert.Equal("pm1", result.Id);
        Assert.Equal("ach_bank_account", result.Type);
    }

    [Fact]
    public async Task GetPaymentMethodAsync_ThrowsArgumentException_WhenPaymentMethodIdIsEmpty()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _accountService.GetPaymentMethodAsync(""));
        await Assert.ThrowsAsync<ArgumentException>(() => _accountService.GetPaymentMethodAsync(null!));
    }

    [Fact]
    public async Task GetPaymentMethodAsync_ThrowsException_WhenPaymentMethodNotFound()
    {
        // Arrange
        var paymentMethods = new List<PaymentMethod>
        {
            new PaymentMethod { Id = "pm1", Type = "ach_bank_account", Name = "Bank Account", Currency = "USD" }
        };
        
        var response = new PaymentMethodsResponse { PaymentMethods = paymentMethods };
        _mockApiClient.Setup(x => x.GetAsync<PaymentMethodsResponse>("/api/v3/brokerage/payment_methods", It.IsAny<CancellationToken>()))
                     .ReturnsAsync(response);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CoinbaseApiException>(() => _accountService.GetPaymentMethodAsync("nonexistent"));
        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public async Task GetPaymentMethodsAsync_ThrowsCoinbaseApiException_WhenApiCallFails()
    {
        // Arrange
        _mockApiClient.Setup(x => x.GetAsync<PaymentMethodsResponse>("/api/v3/brokerage/payment_methods", It.IsAny<CancellationToken>()))
                     .ThrowsAsync(new HttpRequestException("Network error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CoinbaseApiException>(() => _accountService.GetPaymentMethodsAsync());
        Assert.Contains("Failed to retrieve payment methods", exception.Message);
    }

    [Fact]
    public async Task GetPaymentMethodAsync_ThrowsCoinbaseApiException_WhenApiCallFails()
    {
        // Arrange
        _mockApiClient.Setup(x => x.GetAsync<PaymentMethodsResponse>("/api/v3/brokerage/payment_methods", It.IsAny<CancellationToken>()))
                     .ThrowsAsync(new HttpRequestException("Network error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CoinbaseApiException>(() => _accountService.GetPaymentMethodAsync("pm1"));
        Assert.Contains("Failed to retrieve payment method", exception.Message);
    }
}
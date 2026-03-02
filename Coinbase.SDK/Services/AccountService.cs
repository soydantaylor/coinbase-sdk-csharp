using Coinbase.SDK.Exceptions;
using Coinbase.SDK.Interfaces;
using Coinbase.SDK.Models;

namespace Coinbase.SDK.Services;

/// <summary>
/// Manages account-related operations
/// </summary>
public class AccountService : IAccountService
{
    private readonly IApiClient _apiClient;

    /// <summary>
    /// Initializes a new instance of the AccountService class
    /// </summary>
    /// <param name="apiClient">The API client for making HTTP requests</param>
    public AccountService(IApiClient apiClient)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Account>> GetAccountsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _apiClient.GetAsync<AccountsResponse>("/api/v3/brokerage/accounts", cancellationToken);
            return response?.Accounts ?? Enumerable.Empty<Account>();
        }
        catch (Exception ex) when (!(ex is CoinbaseException))
        {
            throw new CoinbaseApiException($"Failed to retrieve accounts: {ex.Message}", 0, ex);
        }
    }

    /// <inheritdoc />
    public async Task<Account> GetAccountAsync(string accountId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accountId))
            throw new ArgumentException("Account ID cannot be null or empty", nameof(accountId));

        try
        {
            var response = await _apiClient.GetAsync<AccountResponse>($"/api/v3/brokerage/accounts/{accountId}", cancellationToken);
            
            if (response?.Account == null)
                throw new CoinbaseApiException($"Account with ID '{accountId}' not found", 404, "NOT_FOUND");
            
            return response.Account;
        }
        catch (Exception ex) when (!(ex is CoinbaseException))
        {
            throw new CoinbaseApiException($"Failed to retrieve account '{accountId}': {ex.Message}", 0, ex);
        }
    }

    /// <inheritdoc />
    public async Task<Account> GetAccountByNameAsync(string accountName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accountName))
            throw new ArgumentException("Account name cannot be null or empty", nameof(accountName));

        try
        {
            var accounts = await GetAccountsAsync(cancellationToken);
            var matchingAccount = accounts.FirstOrDefault(a => 
                string.Equals(a.Name, accountName, StringComparison.OrdinalIgnoreCase));

            if (matchingAccount == null)
                throw new CoinbaseApiException($"Account with name '{accountName}' not found", 404, "NOT_FOUND");

            return matchingAccount;
        }
        catch (Exception ex) when (!(ex is CoinbaseException))
        {
            throw new CoinbaseApiException($"Failed to retrieve account by name '{accountName}': {ex.Message}", 0, ex);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PaymentMethod>> GetPaymentMethodsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _apiClient.GetAsync<PaymentMethodsResponse>("/api/v3/brokerage/payment_methods", cancellationToken);
            return response?.PaymentMethods ?? Enumerable.Empty<PaymentMethod>();
        }
        catch (Exception ex) when (!(ex is CoinbaseException))
        {
            throw new CoinbaseApiException($"Failed to retrieve payment methods: {ex.Message}", 0, ex);
        }
    }

    /// <inheritdoc />
    public async Task<PaymentMethod> GetPaymentMethodAsync(string paymentMethodId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(paymentMethodId))
            throw new ArgumentException("Payment method ID cannot be null or empty", nameof(paymentMethodId));

        try
        {
            var paymentMethods = await GetPaymentMethodsAsync(cancellationToken);
            var matchingPaymentMethod = paymentMethods.FirstOrDefault(pm => 
                string.Equals(pm.Id, paymentMethodId, StringComparison.OrdinalIgnoreCase));

            if (matchingPaymentMethod == null)
                throw new CoinbaseApiException($"Payment method with ID '{paymentMethodId}' not found", 404, "NOT_FOUND");

            return matchingPaymentMethod;
        }
        catch (Exception ex) when (!(ex is CoinbaseException))
        {
            throw new CoinbaseApiException($"Failed to retrieve payment method '{paymentMethodId}': {ex.Message}", 0, ex);
        }
    }

    /// <summary>
    /// Filters accounts based on specified criteria
    /// </summary>
    /// <param name="accounts">The accounts to filter</param>
    /// <param name="currency">Optional currency filter</param>
    /// <param name="type">Optional account type filter</param>
    /// <param name="activeOnly">Whether to include only active accounts</param>
    /// <returns>Filtered accounts</returns>
    public static IEnumerable<Account> FilterAccounts(IEnumerable<Account> accounts, 
        string? currency = null, 
        string? type = null, 
        bool? activeOnly = null)
    {
        if (accounts == null)
            return Enumerable.Empty<Account>();

        var filtered = accounts.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(currency))
        {
            filtered = filtered.Where(a => 
                string.Equals(a.Currency, currency, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            filtered = filtered.Where(a => 
                string.Equals(a.Type, type, StringComparison.OrdinalIgnoreCase));
        }

        if (activeOnly.HasValue)
        {
            filtered = filtered.Where(a => a.Active == activeOnly.Value);
        }

        return filtered;
    }
}

/// <summary>
/// Response model for accounts API endpoint
/// </summary>
public class AccountsResponse
{
    /// <summary>
    /// Collection of accounts returned from the API
    /// </summary>
    public IEnumerable<Account> Accounts { get; set; } = Enumerable.Empty<Account>();
}

/// <summary>
/// Response model for single account API endpoint
/// </summary>
public class AccountResponse
{
    /// <summary>
    /// The account returned from the API
    /// </summary>
    public Account? Account { get; set; }
}
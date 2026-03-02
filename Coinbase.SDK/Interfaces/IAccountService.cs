using Coinbase.SDK.Models;

namespace Coinbase.SDK.Interfaces;

/// <summary>
/// Manages account-related operations
/// </summary>
public interface IAccountService
{
    /// <summary>
    /// Retrieves all accounts for the authenticated user
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A collection of all accounts</returns>
    Task<IEnumerable<Account>> GetAccountsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a specific account by ID
    /// </summary>
    /// <param name="accountId">The unique account identifier</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>The account with the specified ID</returns>
    Task<Account> GetAccountAsync(string accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an account by name
    /// </summary>
    /// <param name="accountName">The account name to search for</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>The account with the specified name</returns>
    Task<Account> GetAccountByNameAsync(string accountName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all payment methods for the authenticated user
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A collection of all payment methods</returns>
    Task<IEnumerable<PaymentMethod>> GetPaymentMethodsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a specific payment method by ID
    /// </summary>
    /// <param name="paymentMethodId">The unique payment method identifier</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>The payment method with the specified ID</returns>
    Task<PaymentMethod> GetPaymentMethodAsync(string paymentMethodId, CancellationToken cancellationToken = default);
}
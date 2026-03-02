using Coinbase.SDK.Models;

namespace Coinbase.SDK.Interfaces;

/// <summary>
/// Handles transfer operations (deposits and withdrawals)
/// </summary>
public interface ITransferService
{
    /// <summary>
    /// Initiates a deposit from a payment method to Coinbase
    /// </summary>
    /// <param name="request">The deposit request parameters</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>The created transfer details</returns>
    Task<Transfer> DepositAsync(DepositRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Initiates a withdrawal from Coinbase to a payment method
    /// </summary>
    /// <param name="request">The withdrawal request parameters</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>The created transfer details</returns>
    Task<Transfer> WithdrawAsync(WithdrawalRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a specific transfer by ID
    /// </summary>
    /// <param name="transferId">The unique transfer identifier</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>The transfer details</returns>
    Task<Transfer> GetTransferAsync(string transferId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves transfers with optional filtering
    /// </summary>
    /// <param name="filter">Optional filter criteria for transfers</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A collection of transfers matching the filter criteria</returns>
    Task<IEnumerable<Transfer>> GetTransfersAsync(TransferFilter? filter = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all available payment methods
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A collection of available payment methods</returns>
    Task<IEnumerable<PaymentMethod>> GetPaymentMethodsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a specific payment method by ID
    /// </summary>
    /// <param name="paymentMethodId">The unique payment method identifier</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>The payment method details</returns>
    Task<PaymentMethod> GetPaymentMethodAsync(string paymentMethodId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits a pending transfer
    /// </summary>
    /// <param name="transferId">The unique transfer identifier</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>The updated transfer details</returns>
    Task<Transfer> CommitTransferAsync(string transferId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Initiates an ACH deposit from a linked bank account
    /// </summary>
    /// <param name="request">The ACH deposit request parameters</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>The ACH deposit response details</returns>
    Task<AchDepositResponse> InitiateAchDepositAsync(AchDepositRequest request, CancellationToken cancellationToken = default);
}
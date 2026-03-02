using Coinbase.SDK.Interfaces;
using Coinbase.SDK.Models;
using Coinbase.SDK.Exceptions;
using System.Text;

namespace Coinbase.SDK.Services;

/// <summary>
/// Implementation of transfer operations
/// </summary>
public class TransferService : ITransferService
{
    private readonly IApiClient _apiClient;

    /// <summary>
    /// Initializes a new instance of the TransferService
    /// </summary>
    /// <param name="apiClient">The API client for making HTTP requests</param>
    public TransferService(IApiClient apiClient)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
    }

    /// <inheritdoc />
    public async Task<Transfer> DepositAsync(DepositRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (request.Amount <= 0)
            throw new ArgumentException("Amount must be greater than zero", nameof(request));

        if (string.IsNullOrWhiteSpace(request.PaymentMethodId))
            throw new ArgumentException("Payment method ID is required", nameof(request));

        if (string.IsNullOrWhiteSpace(request.Currency))
            throw new ArgumentException("Currency is required", nameof(request));

        return await _apiClient.PostAsync<Transfer>("/v2/accounts/primary/deposits", request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Transfer> WithdrawAsync(WithdrawalRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (request.Amount <= 0)
            throw new ArgumentException("Amount must be greater than zero", nameof(request));

        if (string.IsNullOrWhiteSpace(request.PaymentMethodId))
            throw new ArgumentException("Payment method ID is required", nameof(request));

        if (string.IsNullOrWhiteSpace(request.Currency))
            throw new ArgumentException("Currency is required", nameof(request));

        return await _apiClient.PostAsync<Transfer>("/v2/accounts/primary/withdrawals", request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Transfer> GetTransferAsync(string transferId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(transferId))
            throw new ArgumentException("Transfer ID cannot be null or empty", nameof(transferId));

        return await _apiClient.GetAsync<Transfer>($"/v2/accounts/primary/transactions/{transferId}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Transfer>> GetTransfersAsync(TransferFilter? filter = null, CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();

        if (filter != null)
        {
            if (filter.Type.HasValue)
                queryParams.Add($"type={filter.Type.Value.ToString().ToLowerInvariant()}");

            if (filter.Status.HasValue)
                queryParams.Add($"status={filter.Status.Value.ToString().ToLowerInvariant()}");

            if (filter.Method.HasValue)
                queryParams.Add($"method={GetTransferMethodString(filter.Method.Value)}");

            if (filter.StartDate.HasValue)
                queryParams.Add($"start_date={filter.StartDate.Value:yyyy-MM-ddTHH:mm:ssZ}");

            if (filter.EndDate.HasValue)
                queryParams.Add($"end_date={filter.EndDate.Value:yyyy-MM-ddTHH:mm:ssZ}");

            if (filter.Limit.HasValue)
                queryParams.Add($"limit={filter.Limit.Value}");

            if (!string.IsNullOrWhiteSpace(filter.Cursor))
                queryParams.Add($"starting_after={filter.Cursor}");
        }

        var endpoint = "/v2/accounts/primary/transactions";
        if (queryParams.Any())
        {
            endpoint += "?" + string.Join("&", queryParams);
        }

        var response = await _apiClient.GetAsync<TransferListResponse>(endpoint, cancellationToken);
        return response.Data ?? new List<Transfer>();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PaymentMethod>> GetPaymentMethodsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _apiClient.GetAsync<PaymentMethodListResponse>("/api/v3/brokerage/payment_methods", cancellationToken);
        return response.Data ?? new List<PaymentMethod>();
    }

    /// <inheritdoc />
    public async Task<PaymentMethod> GetPaymentMethodAsync(string paymentMethodId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(paymentMethodId))
            throw new ArgumentException("Payment method ID cannot be null or empty", nameof(paymentMethodId));

        return await _apiClient.GetAsync<PaymentMethod>($"/v2/payment-methods/{paymentMethodId}", cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Transfer> CommitTransferAsync(string transferId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(transferId))
            throw new ArgumentException("Transfer ID cannot be null or empty", nameof(transferId));

        // Use the correct commit endpoint for deposits
        return await _apiClient.PostAsync<Transfer>($"/v2/accounts/primary/deposits/{transferId}/commit", new { }, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AchDepositResponse> InitiateAchDepositAsync(AchDepositRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (!decimal.TryParse(request.Amount, out var amount) || amount <= 0)
            throw new CoinbaseValidationException("Amount must be a valid number greater than zero");

        if (string.IsNullOrWhiteSpace(request.PaymentMethodId))
            throw new CoinbaseValidationException("Payment method ID is required");

        if (string.IsNullOrWhiteSpace(request.Currency))
            throw new CoinbaseValidationException("Currency is required");

        try
        {
            // Use the v2 deposits endpoint as per Coinbase documentation
            var response = await _apiClient.PostAsync<AchDepositResponse>("/v2/accounts/primary/deposits", request, cancellationToken);
            
            // Return the response even if there's a cancellation reason - let the caller handle it
            // This allows us to see what Coinbase is telling us about why the transfer failed
            return response;
        }
        catch (CoinbaseApiException ex)
        {
            throw new CoinbaseTransferException($"ACH deposit API call failed: {ex.Message}", ex, ex.ErrorCode);
        }
        catch (CoinbaseNetworkException ex)
        {
            throw new CoinbaseTransferException($"Network error during ACH deposit: {ex.Message}", ex);
        }
    }

    private static string GetTransferMethodString(TransferMethod method)
    {
        return method switch
        {
            TransferMethod.AchBankAccount => "ach_bank_account",
            TransferMethod.Wire => "wire",
            TransferMethod.CreditCard => "credit_card",
            TransferMethod.DebitCard => "debit_card",
            TransferMethod.PayPal => "paypal_account",
            TransferMethod.BankAccount => "bank_account",
            _ => method.ToString().ToLowerInvariant()
        };
    }
}

/// <summary>
/// Response wrapper for transfer list API calls
/// </summary>
internal class TransferListResponse
{
    public IEnumerable<Transfer>? Data { get; set; }
    public object? Pagination { get; set; }
}

/// <summary>
/// Response wrapper for payment method list API calls
/// </summary>
public class PaymentMethodListResponse
{
    /// <summary>
    /// The list of payment methods
    /// </summary>
    [Newtonsoft.Json.JsonProperty("payment_methods")]
    public IEnumerable<PaymentMethod>? Data { get; set; }
    
    /// <summary>
    /// Pagination information for the response
    /// </summary>
    public object? Pagination { get; set; }
}
using Coinbase.SDK.Interfaces;

namespace Coinbase.SDK;

/// <summary>
/// The main SDK client facade that provides access to all Coinbase functionality
/// </summary>
public class CoinbaseClient : ICoinbaseClient
{
    private readonly IAccountService _accounts;
    private readonly ITradingService _trading;
    private readonly IMarketDataService _marketData;
    private readonly ITransferService _transfers;

    /// <summary>
    /// Initializes a new instance of the CoinbaseClient class with dependency injection
    /// </summary>
    /// <param name="accountService">The account service</param>
    /// <param name="tradingService">The trading service</param>
    /// <param name="marketDataService">The market data service</param>
    /// <param name="transferService">The transfer service</param>
    /// <exception cref="ArgumentNullException">Thrown when any service is null</exception>
    public CoinbaseClient(
        IAccountService accountService,
        ITradingService tradingService,
        IMarketDataService marketDataService,
        ITransferService transferService)
    {
        _accounts = accountService ?? throw new ArgumentNullException(nameof(accountService));
        _trading = tradingService ?? throw new ArgumentNullException(nameof(tradingService));
        _marketData = marketDataService ?? throw new ArgumentNullException(nameof(marketDataService));
        _transfers = transferService ?? throw new ArgumentNullException(nameof(transferService));
    }

    /// <summary>
    /// Initializes a new instance of the CoinbaseClient class with API client (for factory pattern)
    /// </summary>
    /// <param name="apiClient">The API client for making HTTP requests</param>
    /// <exception cref="ArgumentNullException">Thrown when apiClient is null</exception>
    public CoinbaseClient(IApiClient apiClient)
    {
        if (apiClient == null) throw new ArgumentNullException(nameof(apiClient));
        
        _accounts = new Services.AccountService(apiClient);
        _trading = new Services.TradingService(apiClient);
        _marketData = new Services.MarketDataService(apiClient);
        _transfers = new Services.TransferService(apiClient);
    }

    /// <summary>
    /// Gets the account service for managing account-related operations
    /// </summary>
    public IAccountService Accounts => _accounts;

    /// <summary>
    /// Gets the trading service for executing trades and managing orders
    /// </summary>
    public ITradingService Trading => _trading;

    /// <summary>
    /// Gets the market data service for retrieving market data and pricing information
    /// </summary>
    public IMarketDataService MarketData => _marketData;

    /// <summary>
    /// Gets the transfer service for managing deposits and withdrawals
    /// </summary>
    public ITransferService Transfers => _transfers;
}
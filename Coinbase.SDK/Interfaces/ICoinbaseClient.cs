namespace Coinbase.SDK.Interfaces;

/// <summary>
/// The main facade interface that provides access to all SDK functionality
/// </summary>
public interface ICoinbaseClient
{
    /// <summary>
    /// Gets the account service for managing account-related operations
    /// </summary>
    IAccountService Accounts { get; }

    /// <summary>
    /// Gets the trading service for executing trades and managing orders
    /// </summary>
    ITradingService Trading { get; }

    /// <summary>
    /// Gets the market data service for retrieving market data and pricing information
    /// </summary>
    IMarketDataService MarketData { get; }

    /// <summary>
    /// Gets the transfer service for managing deposits and withdrawals
    /// </summary>
    ITransferService Transfers { get; }
}
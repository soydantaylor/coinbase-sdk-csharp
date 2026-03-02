using Coinbase.SDK.Interfaces;
using Coinbase.SDK.Models;
using Coinbase.SDK.Services;
using FsCheck;
using FsCheck.Xunit;
using Moq;

namespace Coinbase.SDK.Tests;

/// <summary>
/// Property-based tests for CoinbaseClient
/// </summary>
public class CoinbaseClientPropertyTests
{
    /// <summary>
    /// **Feature: coinbase-sdk, Property 19: Concurrent operation safety**
    /// **Validates: Requirements 12.3, 12.5**
    /// For any set of simultaneous API operations from multiple threads, all operations should complete successfully without data corruption or race conditions
    /// </summary>
    [Property(MaxTest = 100)]
    public bool ConcurrentOperationSafetyProperty(PositiveInt threadCount, PositiveInt operationsPerThread)
    {
        // Limit thread count and operations to reasonable numbers for testing
        var numThreads = Math.Min(threadCount.Get, 10);
        var numOperations = Math.Min(operationsPerThread.Get, 20);

        // Create a mock API client - we don't need to set up specific calls for this test
        var mockApiClient = new Mock<IApiClient>();

        // Create the client instance
        var client = new CoinbaseClient(mockApiClient.Object);

        // Track successful operations and any exceptions
        var successfulOperations = 0;
        var exceptions = new List<Exception>();
        var lockObject = new object();

        // Create tasks that will access different services concurrently
        var tasks = new List<Task>();

        for (int i = 0; i < numThreads; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < numOperations; j++)
                {
                    try
                    {
                        // Test concurrent access to service properties (the main thread safety concern)
                        var operation = j % 3;
                        switch (operation)
                        {
                            case 0:
                                // Access Accounts service property multiple times
                                var accounts1 = client.Accounts;
                                var accounts2 = client.Accounts;
                                // Verify we get the same instance (singleton behavior)
                                if (!ReferenceEquals(accounts1, accounts2))
                                    throw new InvalidOperationException("Account service instances are not consistent");
                                break;
                            case 1:
                                // Access Trading service property multiple times
                                var trading1 = client.Trading;
                                var trading2 = client.Trading;
                                // Verify we get the same instance (singleton behavior)
                                if (!ReferenceEquals(trading1, trading2))
                                    throw new InvalidOperationException("Trading service instances are not consistent");
                                break;
                            case 2:
                                // Access MarketData service property multiple times
                                var marketData1 = client.MarketData;
                                var marketData2 = client.MarketData;
                                // Verify we get the same instance (singleton behavior)
                                if (!ReferenceEquals(marketData1, marketData2))
                                    throw new InvalidOperationException("MarketData service instances are not consistent");
                                break;
                        }

                        lock (lockObject)
                        {
                            successfulOperations++;
                        }
                    }
                    catch (Exception ex)
                    {
                        lock (lockObject)
                        {
                            exceptions.Add(ex);
                        }
                    }
                }
            }));
        }

        // Wait for all tasks to complete
        Task.WaitAll(tasks.ToArray());

        // Verify that:
        // 1. No exceptions occurred during concurrent access
        // 2. All operations completed successfully
        // 3. The expected number of operations were executed
        var expectedOperations = numThreads * numOperations;
        
        return exceptions.Count == 0 && 
               successfulOperations == expectedOperations;
    }

    /// <summary>
    /// Property test to verify that service instances are properly initialized and reused
    /// </summary>
    [Property(MaxTest = 100)]
    public bool ServiceInstanceConsistencyProperty(PositiveInt accessCount)
    {
        var mockApiClient = new Mock<IApiClient>();
        var client = new CoinbaseClient(mockApiClient.Object);
        
        var numAccesses = Math.Min(accessCount.Get, 50);
        
        // Access each service multiple times and verify we get the same instance
        IAccountService? firstAccountsInstance = null;
        ITradingService? firstTradingInstance = null;
        IMarketDataService? firstMarketDataInstance = null;
        
        for (int i = 0; i < numAccesses; i++)
        {
            var accounts = client.Accounts;
            var trading = client.Trading;
            var marketData = client.MarketData;
            
            if (firstAccountsInstance == null)
            {
                firstAccountsInstance = accounts;
                firstTradingInstance = trading;
                firstMarketDataInstance = marketData;
            }
            else
            {
                // Verify that we get the same instances (singleton behavior)
                if (!ReferenceEquals(firstAccountsInstance, accounts) ||
                    !ReferenceEquals(firstTradingInstance, trading) ||
                    !ReferenceEquals(firstMarketDataInstance, marketData))
                {
                    return false;
                }
            }
        }
        
        return true;
    }
}
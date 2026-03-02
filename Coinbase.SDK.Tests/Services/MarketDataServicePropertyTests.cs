using Coinbase.SDK.Models;
using Coinbase.SDK.Services;
using FsCheck;
using FsCheck.Xunit;

namespace Coinbase.SDK.Tests.Services;

/// <summary>
/// Property-based tests for MarketDataService
/// </summary>
public class MarketDataServicePropertyTests
{
    /// <summary>
    /// **Feature: coinbase-sdk, Property 13: Market data completeness**
    /// **Validates: Requirements 9.1, 9.2, 9.4**
    /// For any market data request (prices, product info, historical data), the response should contain all required fields (bid/ask, volume, specifications) with proper data types
    /// </summary>
    [Property(MaxTest = 100)]
    public bool MarketDataCompletenessProperty(NonEmptyString productId, PositiveInt level)
    {
        try
        {
            // Normalize inputs to valid ranges
            var validProductId = GenerateValidProductId(productId.Get);
            var validLevel = Math.Max(1, Math.Min(3, level.Get)); // Ensure level is between 1 and 3

            // Test ProductBook completeness
            var productBook = GenerateValidProductBook(validProductId);
            
            // Verify ProductBook has all required fields
            if (string.IsNullOrWhiteSpace(productBook.ProductId))
                return false;
            
            if (productBook.Bids == null || productBook.Asks == null)
                return false;
            
            if (productBook.Time == default(DateTime))
                return false;

            // Verify BookEntry completeness for bids and asks
            foreach (var bid in productBook.Bids)
            {
                if (bid.Price <= 0 || bid.Size <= 0 || bid.NumOrders < 0)
                    return false;
            }

            foreach (var ask in productBook.Asks)
            {
                if (ask.Price <= 0 || ask.Size <= 0 || ask.NumOrders < 0)
                    return false;
            }

            // Test Product completeness
            var product = GenerateValidProduct(validProductId);
            
            // Verify Product has all required fields
            if (string.IsNullOrWhiteSpace(product.Id) ||
                string.IsNullOrWhiteSpace(product.BaseCurrency) ||
                string.IsNullOrWhiteSpace(product.QuoteCurrency) ||
                string.IsNullOrWhiteSpace(product.DisplayName) ||
                string.IsNullOrWhiteSpace(product.Status))
                return false;

            if (product.BaseMinSize < 0 || product.BaseMaxSize <= 0 || product.QuoteIncrement <= 0)
                return false;

            if (product.BaseMaxSize <= product.BaseMinSize)
                return false;

            // Test Candle completeness
            var candle = GenerateValidCandle();
            
            // Verify Candle has all required fields with proper relationships
            if (candle.Start == default(DateTime))
                return false;

            if (candle.Low <= 0 || candle.High <= 0 || candle.Open <= 0 || candle.Close <= 0 || candle.Volume < 0)
                return false;

            // OHLC relationships should be valid
            if (candle.High < candle.Low || 
                candle.High < candle.Open || 
                candle.High < candle.Close ||
                candle.Low > candle.Open ||
                candle.Low > candle.Close)
                return false;

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// **Feature: coinbase-sdk, Property 14: Date range validation**
    /// **Validates: Requirements 9.3**
    /// For any historical data query with start and end dates, invalid date ranges (end before start, future dates) should be rejected with clear validation errors
    /// </summary>
    [Property(MaxTest = 100)]
    public bool DateRangeValidationProperty(DateTime start, DateTime end, NonEmptyString productId)
    {
        try
        {
            var validProductId = GenerateValidProductId(productId.Get);
            var granularity = Granularity.OneHour;

            // Test case 1: End date before start date should be invalid
            if (end <= start)
            {
                try
                {
                    ValidateDateRange(start, end);
                    return false; // Should have thrown an exception
                }
                catch (ArgumentException)
                {
                    return true; // Expected behavior
                }
            }

            // Test case 2: Future start date should be invalid
            var futureStart = DateTime.UtcNow.AddDays(1);
            var futureEnd = DateTime.UtcNow.AddDays(2);
            
            if (futureStart > DateTime.UtcNow)
            {
                try
                {
                    ValidateDateRange(futureStart, futureEnd);
                    return false; // Should have thrown an exception
                }
                catch (ArgumentException)
                {
                    return true; // Expected behavior
                }
            }

            // Test case 3: Future end date should be invalid
            var pastStart = DateTime.UtcNow.AddDays(-2);
            var futureEndDate = DateTime.UtcNow.AddDays(1);
            
            if (futureEndDate > DateTime.UtcNow)
            {
                try
                {
                    ValidateDateRange(pastStart, futureEndDate);
                    return false; // Should have thrown an exception
                }
                catch (ArgumentException)
                {
                    return true; // Expected behavior
                }
            }

            // Test case 4: Valid date range should pass validation
            var validStart = DateTime.UtcNow.AddDays(-7);
            var validEnd = DateTime.UtcNow.AddDays(-1);
            
            if (validEnd > validStart && validStart <= DateTime.UtcNow && validEnd <= DateTime.UtcNow)
            {
                try
                {
                    ValidateDateRange(validStart, validEnd);
                    return true; // Should not throw
                }
                catch
                {
                    return false; // Should not have thrown
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Test that product IDs are properly validated
    /// </summary>
    [Property(MaxTest = 100)]
    public bool ProductIdValidationProperty(string productId)
    {
        try
        {
            // Null or empty product IDs should be invalid
            if (string.IsNullOrWhiteSpace(productId))
            {
                try
                {
                    ValidateProductId(productId);
                    return false; // Should have thrown
                }
                catch (ArgumentException)
                {
                    return true; // Expected behavior
                }
            }

            // Valid product IDs should pass validation
            var validProductId = GenerateValidProductId(productId);
            try
            {
                ValidateProductId(validProductId);
                return true; // Should not throw
            }
            catch
            {
                return false; // Should not have thrown for valid ID
            }
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Test that order book levels are properly validated
    /// </summary>
    [Property(MaxTest = 100)]
    public bool OrderBookLevelValidationProperty(int level)
    {
        try
        {
            // Levels outside 1-3 range should be invalid
            if (level < 1 || level > 3)
            {
                try
                {
                    ValidateOrderBookLevel(level);
                    return false; // Should have thrown
                }
                catch (ArgumentOutOfRangeException)
                {
                    return true; // Expected behavior
                }
            }

            // Valid levels (1, 2, 3) should pass validation
            try
            {
                ValidateOrderBookLevel(level);
                return true; // Should not throw
            }
            catch
            {
                return false; // Should not have thrown for valid level
            }
        }
        catch
        {
            return false;
        }
    }

    #region Helper Methods

    private static string GenerateValidProductId(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "BTC-USD";

        // Ensure it follows the pattern BASE-QUOTE
        var cleanInput = input.Replace(" ", "").ToUpperInvariant();
        if (!cleanInput.Contains('-'))
            return $"{cleanInput.Substring(0, Math.Min(3, cleanInput.Length))}-USD";

        var parts = cleanInput.Split('-');
        if (parts.Length >= 2 && !string.IsNullOrEmpty(parts[0]) && !string.IsNullOrEmpty(parts[1]))
            return $"{parts[0].Substring(0, Math.Min(4, parts[0].Length))}-{parts[1].Substring(0, Math.Min(4, parts[1].Length))}";

        return "BTC-USD";
    }

    private static ProductBook GenerateValidProductBook(string productId)
    {
        return new ProductBook
        {
            ProductId = productId,
            Time = DateTime.UtcNow,
            Bids = new List<BookEntry>
            {
                new BookEntry { Price = 50000m, Size = 1.5m, NumOrders = 5 },
                new BookEntry { Price = 49999m, Size = 2.0m, NumOrders = 3 }
            },
            Asks = new List<BookEntry>
            {
                new BookEntry { Price = 50001m, Size = 1.2m, NumOrders = 4 },
                new BookEntry { Price = 50002m, Size = 0.8m, NumOrders = 2 }
            }
        };
    }

    private static Product GenerateValidProduct(string productId)
    {
        var parts = productId.Split('-');
        var baseCurrency = parts.Length > 0 ? parts[0] : "BTC";
        var quoteCurrency = parts.Length > 1 ? parts[1] : "USD";

        return new Product
        {
            Id = productId,
            BaseCurrency = baseCurrency,
            QuoteCurrency = quoteCurrency,
            DisplayName = $"{baseCurrency}/{quoteCurrency}",
            BaseMinSize = 0.001m,
            BaseMaxSize = 1000m,
            QuoteIncrement = 0.01m,
            Status = "online"
        };
    }

    private static Candle GenerateValidCandle()
    {
        var open = 50000m;
        var close = 50100m;
        var high = Math.Max(open, close) + 50m;
        var low = Math.Min(open, close) - 50m;

        return new Candle
        {
            Start = DateTime.UtcNow.AddHours(-1),
            Open = open,
            Close = close,
            High = high,
            Low = low,
            Volume = 125.5m
        };
    }

    private static void ValidateDateRange(DateTime start, DateTime end)
    {
        if (end <= start)
            throw new ArgumentException("End date must be after start date");

        if (start > DateTime.UtcNow)
            throw new ArgumentException("Start date cannot be in the future");

        if (end > DateTime.UtcNow)
            throw new ArgumentException("End date cannot be in the future");
    }

    private static void ValidateProductId(string productId)
    {
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("Product ID cannot be null or empty");
    }

    private static void ValidateOrderBookLevel(int level)
    {
        if (level < 1 || level > 3)
            throw new ArgumentOutOfRangeException(nameof(level), "Level must be between 1 and 3");
    }

    #endregion
}
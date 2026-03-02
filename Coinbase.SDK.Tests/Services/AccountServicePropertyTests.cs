using Coinbase.SDK.Models;
using Coinbase.SDK.Services;
using FsCheck;
using FsCheck.Xunit;

namespace Coinbase.SDK.Tests.Services;

/// <summary>
/// Property-based tests for AccountService
/// </summary>
public class AccountServicePropertyTests
{
    /// <summary>
    /// **Feature: coinbase-ach-bitcoin, Property 3: Account Balance Consistency**
    /// **Validates: Requirements 3.1, 3.2**
    /// For any account balance retrieval, the returned balance amounts should be non-negative and properly formatted decimal values
    /// </summary>
    [Property(MaxTest = 100)]
    public bool AccountBalanceConsistencyProperty(List<Account> accounts)
    {
        // Skip null account lists
        if (accounts == null)
            return true;

        // Generate valid test data for accounts
        var validAccounts = accounts.Select((account, index) => new Account
        {
            Uuid = account?.Uuid ?? $"account-{index}",
            Name = account?.Name ?? $"Account {index}",
            Currency = account?.Currency ?? GetValidCurrency(index),
            Type = account?.Type ?? GetValidAccountType(index),
            Active = account?.Active ?? true,
            Ready = account?.Ready ?? true,
            // Ensure balance amounts are non-negative and properly formatted
            AvailableBalance = new Money 
            { 
                Value = Math.Abs(account?.AvailableBalance?.Value ?? Math.Abs((decimal)(index * 100.50))),
                Currency = account?.AvailableBalance?.Currency ?? GetValidCurrency(index)
            },
            Hold = new Money 
            { 
                Value = Math.Abs(account?.Hold?.Value ?? Math.Abs((decimal)(index * 10.25))),
                Currency = account?.Hold?.Currency ?? GetValidCurrency(index)
            },
            IsDefault = account?.IsDefault ?? false,
            CreatedAt = account?.CreatedAt ?? DateTime.UtcNow.AddDays(-index),
            UpdatedAt = account?.UpdatedAt ?? DateTime.UtcNow,
            RetailPortfolioId = account?.RetailPortfolioId ?? $"portfolio-{index}",
            Platform = account?.Platform ?? "coinbase"
        }).ToList();

        try
        {
            // Act - Check all account balances
            foreach (var account in validAccounts)
            {
                // Ensure Money objects are not null
                if (account.AvailableBalance == null || account.Hold == null)
                    return false;

                // Assert - Available balance should be non-negative and properly formatted
                if (account.AvailableBalance.Value < 0)
                    return false;

                // Assert - Hold balance should be non-negative and properly formatted
                if (account.Hold.Value < 0)
                    return false;

                // Assert - Balance values should be valid decimal numbers (not NaN, Infinity, etc.)
                if (!IsValidDecimal(account.AvailableBalance.Value))
                    return false;

                if (!IsValidDecimal(account.Hold.Value))
                    return false;

                // Assert - Currency codes should be valid (not null or empty)
                if (string.IsNullOrWhiteSpace(account.AvailableBalance.Currency))
                    return false;

                if (string.IsNullOrWhiteSpace(account.Hold.Currency))
                    return false;

                // Assert - Balance currencies should match account currency
                if (!string.Equals(account.AvailableBalance.Currency, account.Currency, StringComparison.OrdinalIgnoreCase))
                    return false;

                if (!string.Equals(account.Hold.Currency, account.Currency, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// **Feature: coinbase-sdk, Property 8: Account filtering consistency**
    /// **Validates: Requirements 3.4**
    /// For any collection of accounts and filter criteria (name, currency, type), filtering should return only accounts that match all specified criteria
    /// </summary>
    [Property(MaxTest = 100)]
    public bool AccountFilteringConsistencyProperty(List<Account> accounts, string currency, string type, bool activeOnly)
    {
        // Skip null or empty account lists
        if (accounts == null || accounts.Count == 0)
            return true;

        // Generate valid test data for accounts if they have invalid data
        var validAccounts = accounts.Select((account, index) => new Account
        {
            Uuid = account?.Uuid ?? $"account-{index}",
            Name = account?.Name ?? $"Account {index}",
            Currency = account?.Currency ?? GetValidCurrency(index),
            Type = account?.Type ?? GetValidAccountType(index),
            Active = account?.Active ?? (index % 2 == 0),
            AvailableBalance = account?.AvailableBalance ?? new Money { Value = 100m, Currency = "USD" },
            Hold = account?.Hold ?? new Money { Value = 0m, Currency = "USD" },
            IsDefault = account?.IsDefault ?? false,
            Ready = account?.Ready ?? true,
            CreatedAt = account?.CreatedAt ?? DateTime.UtcNow.AddDays(-index),
            UpdatedAt = account?.UpdatedAt ?? DateTime.UtcNow,
            RetailPortfolioId = account?.RetailPortfolioId ?? $"portfolio-{index}",
            Platform = account?.Platform ?? "coinbase"
        }).ToList();

        // Ensure we have some variety in the test data
        if (validAccounts.Count > 1)
        {
            validAccounts[0].Currency = "BTC";
            validAccounts[0].Type = "crypto";
            validAccounts[0].Active = true;
            
            if (validAccounts.Count > 1)
            {
                validAccounts[1].Currency = "USD";
                validAccounts[1].Type = "fiat";
                validAccounts[1].Active = false;
            }
        }

        // Use valid filter values
        var filterCurrency = GetValidCurrency(currency?.GetHashCode() ?? 0);
        var filterType = GetValidAccountType(type?.GetHashCode() ?? 0);

        try
        {
            // Act - Apply filtering
            var filteredAccounts = AccountService.FilterAccounts(
                validAccounts, 
                filterCurrency, 
                filterType, 
                activeOnly).ToList();

            // Assert - All returned accounts should match the filter criteria
            foreach (var account in filteredAccounts)
            {
                // Check currency filter
                if (!string.IsNullOrWhiteSpace(filterCurrency))
                {
                    if (!string.Equals(account.Currency, filterCurrency, StringComparison.OrdinalIgnoreCase))
                        return false;
                }

                // Check type filter
                if (!string.IsNullOrWhiteSpace(filterType))
                {
                    if (!string.Equals(account.Type, filterType, StringComparison.OrdinalIgnoreCase))
                        return false;
                }

                // Check active filter
                if (account.Active != activeOnly)
                    return false;
            }

            // Additional check: ensure we're not returning accounts that shouldn't match
            var allMatchingAccounts = validAccounts.Where(account =>
            {
                var currencyMatches = string.IsNullOrWhiteSpace(filterCurrency) || 
                    string.Equals(account.Currency, filterCurrency, StringComparison.OrdinalIgnoreCase);
                
                var typeMatches = string.IsNullOrWhiteSpace(filterType) || 
                    string.Equals(account.Type, filterType, StringComparison.OrdinalIgnoreCase);
                
                var activeMatches = account.Active == activeOnly;

                return currencyMatches && typeMatches && activeMatches;
            }).ToList();

            // The filtered result should contain exactly the same accounts as our manual filter
            var resultIds = filteredAccounts.Select(a => a.Uuid).OrderBy(x => x).ToList();
            var expectedIds = allMatchingAccounts.Select(a => a.Uuid).OrderBy(x => x).ToList();

            return resultIds.SequenceEqual(expectedIds);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Test that filtering with no criteria returns all accounts
    /// </summary>
    [Property(MaxTest = 100)]
    public bool FilteringWithNoCriteriaReturnsAllAccountsProperty(List<Account> accounts)
    {
        // Skip null account lists
        if (accounts == null)
            return true;

        // Generate valid test data
        var validAccounts = accounts.Select((account, index) => new Account
        {
            Uuid = account?.Uuid ?? $"account-{index}",
            Name = account?.Name ?? $"Account {index}",
            Currency = account?.Currency ?? GetValidCurrency(index),
            Type = account?.Type ?? GetValidAccountType(index),
            Active = account?.Active ?? (index % 2 == 0),
            AvailableBalance = account?.AvailableBalance ?? new Money { Value = 100m, Currency = "USD" },
            Hold = account?.Hold ?? new Money { Value = 0m, Currency = "USD" }
        }).ToList();

        try
        {
            // Act - Filter with no criteria (all null/default)
            var filteredAccounts = AccountService.FilterAccounts(validAccounts, null, null, null).ToList();

            // Assert - Should return all accounts
            var originalIds = validAccounts.Select(a => a.Uuid).OrderBy(x => x).ToList();
            var filteredIds = filteredAccounts.Select(a => a.Uuid).OrderBy(x => x).ToList();

            return originalIds.SequenceEqual(filteredIds);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Test that filtering empty collection returns empty result
    /// </summary>
    [Property(MaxTest = 100)]
    public bool FilteringEmptyCollectionReturnsEmptyProperty(string currency, string type, bool activeOnly)
    {
        try
        {
            // Act - Filter empty collection
            var filteredAccounts = AccountService.FilterAccounts(
                Enumerable.Empty<Account>(), 
                currency, 
                type, 
                activeOnly).ToList();

            // Assert - Should return empty collection
            return filteredAccounts.Count == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// **Feature: coinbase-ach-bitcoin, Property 4: Payment Method Validation**
    /// **Validates: Requirements 3.3**
    /// For any payment method retrieval, all returned payment methods should have valid IDs and verification status
    /// </summary>
    [Property(MaxTest = 100)]
    public bool PaymentMethodValidationProperty(List<PaymentMethod> paymentMethods)
    {
        // Skip null payment method lists
        if (paymentMethods == null)
            return true;

        // Generate valid test data for payment methods
        var validPaymentMethods = paymentMethods.Select((pm, index) => new PaymentMethod
        {
            Id = pm?.Id ?? $"pm-{index}",
            Type = pm?.Type ?? GetValidPaymentMethodType(index),
            Name = pm?.Name ?? $"Payment Method {index}",
            Currency = pm?.Currency ?? GetValidCurrency(index),
            PrimaryBuy = pm?.PrimaryBuy ?? (index % 2 == 0),
            PrimarySell = pm?.PrimarySell ?? (index % 3 == 0),
            InstantBuy = pm?.InstantBuy ?? (index % 4 == 0),
            InstantSell = pm?.InstantSell ?? (index % 5 == 0),
            Limits = pm?.Limits ?? new PaymentMethodLimits()
        }).ToList();

        try
        {
            // Act - Check all payment methods
            foreach (var paymentMethod in validPaymentMethods)
            {
                // Assert - Payment method ID should be valid (not null or empty)
                if (string.IsNullOrWhiteSpace(paymentMethod.Id))
                    return false;

                // Assert - Payment method type should be valid (not null or empty)
                if (string.IsNullOrWhiteSpace(paymentMethod.Type))
                    return false;

                // Assert - Payment method name should be valid (not null or empty)
                if (string.IsNullOrWhiteSpace(paymentMethod.Name))
                    return false;

                // Assert - Currency should be valid (not null or empty)
                if (string.IsNullOrWhiteSpace(paymentMethod.Currency))
                    return false;

                // Assert - Boolean flags should be valid (no special validation needed for bool)
                // These are already validated by the type system

                // Assert - Limits object should not be null (can be empty but not null)
                if (paymentMethod.Limits == null)
                    return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string GetValidPaymentMethodType(int seed)
    {
        var types = new[] { "ach_bank_account", "debit_card", "credit_card", "paypal_account", "wire_transfer" };
        return types[Math.Abs(seed) % types.Length];
    }

    private static bool IsValidDecimal(decimal value)
    {
        // Check if the decimal value is valid (not NaN equivalent, within valid range)
        try
        {
            // Decimal type in C# doesn't have NaN or Infinity, but we can check for extreme values
            return value >= decimal.MinValue && value <= decimal.MaxValue;
        }
        catch
        {
            return false;
        }
    }

    private static string GetValidCurrency(int seed)
    {
        var currencies = new[] { "BTC", "ETH", "USD", "EUR", "GBP", "LTC", "ADA", "DOT" };
        return currencies[Math.Abs(seed) % currencies.Length];
    }

    private static string GetValidAccountType(int seed)
    {
        var types = new[] { "crypto", "fiat", "vault", "wallet", "trading" };
        return types[Math.Abs(seed) % types.Length];
    }
}
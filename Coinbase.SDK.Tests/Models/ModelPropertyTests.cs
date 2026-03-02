using Coinbase.SDK.Models;
using FsCheck;
using FsCheck.Xunit;
using Newtonsoft.Json;

namespace Coinbase.SDK.Tests.Models;

/// <summary>
/// Property-based tests for data model serialization
/// </summary>
public class ModelPropertyTests
{
    /// <summary>
    /// **Feature: coinbase-sdk, Property 15: JSON serialization round-trip**
    /// **Validates: Requirements 10.1, 10.2**
    /// For any SDK data model, serializing to JSON then deserializing should produce an object equal to the original, preserving all data types and precision
    /// </summary>
    [Property(MaxTest = 100)]
    public bool JsonSerializationRoundTripProperty(decimal value, string currency, string accountId, string name)
    {
        // Filter invalid inputs
        if (string.IsNullOrWhiteSpace(currency) || string.IsNullOrWhiteSpace(accountId) || string.IsNullOrWhiteSpace(name))
            return true; // Skip invalid inputs

        // Ensure reasonable decimal values
        if (value < -1000000 || value > 1000000)
            return true; // Skip extreme values

        try
        {
            // Test Money model
            var originalMoney = new Money
            {
                Value = value,
                Currency = currency
            };

            var moneyJson = JsonConvert.SerializeObject(originalMoney);
            var deserializedMoney = JsonConvert.DeserializeObject<Money>(moneyJson);

            var moneyRoundTrip = deserializedMoney != null &&
                                deserializedMoney.Value == originalMoney.Value &&
                                deserializedMoney.Currency == originalMoney.Currency;

            // Test Account model
            var originalAccount = new Account
            {
                Uuid = accountId,
                Name = name,
                Currency = currency,
                AvailableBalance = originalMoney,
                IsDefault = true,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Type = "ACCOUNT",
                Ready = true,
                Hold = new Money { Value = 0, Currency = currency },
                RetailPortfolioId = "portfolio-123",
                Platform = "advanced_trade"
            };

            var accountJson = JsonConvert.SerializeObject(originalAccount);
            var deserializedAccount = JsonConvert.DeserializeObject<Account>(accountJson);

            var accountRoundTrip = deserializedAccount != null &&
                                  deserializedAccount.Uuid == originalAccount.Uuid &&
                                  deserializedAccount.Name == originalAccount.Name &&
                                  deserializedAccount.Currency == originalAccount.Currency &&
                                  deserializedAccount.AvailableBalance.Value == originalAccount.AvailableBalance.Value &&
                                  deserializedAccount.AvailableBalance.Currency == originalAccount.AvailableBalance.Currency &&
                                  deserializedAccount.IsDefault == originalAccount.IsDefault &&
                                  deserializedAccount.Active == originalAccount.Active &&
                                  deserializedAccount.Type == originalAccount.Type &&
                                  deserializedAccount.Ready == originalAccount.Ready;

            return moneyRoundTrip && accountRoundTrip;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// **Feature: coinbase-sdk, Property 7: Account data serialization round-trip**
    /// **Validates: Requirements 3.2, 3.3, 10.3**
    /// For any account object with monetary values, serializing to JSON then deserializing should preserve all decimal precision and currency information exactly
    /// </summary>
    [Property(MaxTest = 100)]
    public bool AccountDataSerializationRoundTripProperty(decimal availableBalance, decimal holdBalance, string currency)
    {
        // Filter invalid inputs
        if (string.IsNullOrWhiteSpace(currency))
            return true; // Skip invalid currency

        // Ensure reasonable decimal values with high precision
        if (availableBalance < -1000000 || availableBalance > 1000000 || 
            holdBalance < -1000000 || holdBalance > 1000000)
            return true; // Skip extreme values

        try
        {
            // Create account with precise monetary values
            var originalAccount = new Account
            {
                Uuid = "test-account-123",
                Name = "Test Account",
                Currency = currency,
                AvailableBalance = new Money { Value = availableBalance, Currency = currency },
                Hold = new Money { Value = holdBalance, Currency = currency },
                IsDefault = false,
                Active = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Type = "ACCOUNT",
                Ready = true,
                RetailPortfolioId = "portfolio-456",
                Platform = "advanced_trade"
            };

            // Serialize to JSON
            var json = JsonConvert.SerializeObject(originalAccount);

            // Deserialize back to object
            var deserializedAccount = JsonConvert.DeserializeObject<Account>(json);

            // Verify all monetary precision is preserved
            var precisionPreserved = deserializedAccount != null &&
                                   deserializedAccount.AvailableBalance.Value == originalAccount.AvailableBalance.Value &&
                                   deserializedAccount.AvailableBalance.Currency == originalAccount.AvailableBalance.Currency &&
                                   deserializedAccount.Hold.Value == originalAccount.Hold.Value &&
                                   deserializedAccount.Hold.Currency == originalAccount.Hold.Currency &&
                                   deserializedAccount.Currency == originalAccount.Currency;

            // Verify all other properties are preserved
            var allPropertiesPreserved = deserializedAccount.Uuid == originalAccount.Uuid &&
                                       deserializedAccount.Name == originalAccount.Name &&
                                       deserializedAccount.IsDefault == originalAccount.IsDefault &&
                                       deserializedAccount.Active == originalAccount.Active &&
                                       deserializedAccount.Type == originalAccount.Type &&
                                       deserializedAccount.Ready == originalAccount.Ready &&
                                       deserializedAccount.RetailPortfolioId == originalAccount.RetailPortfolioId &&
                                       deserializedAccount.Platform == originalAccount.Platform;

            return precisionPreserved && allPropertiesPreserved;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// **Feature: coinbase-sdk, Property 17: DateTime conversion accuracy**
    /// **Validates: Requirements 10.4**
    /// For any UTC timestamp from the API, conversion to .NET DateTime should preserve the exact time value and timezone information
    /// </summary>
    [Property(MaxTest = 100)]
    public bool DateTimeConversionAccuracyProperty(int year, int month, int day, int hour, int minute, int second)
    {
        // Filter invalid date components
        if (year < 2020 || year > 2030 || month < 1 || month > 12 || day < 1 || day > 28 ||
            hour < 0 || hour > 23 || minute < 0 || minute > 59 || second < 0 || second > 59)
            return true; // Skip invalid dates

        try
        {
            // Create a UTC DateTime
            var originalDateTime = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);

            // Create an account with this timestamp
            var account = new Account
            {
                Uuid = "test-account",
                Name = "Test Account",
                Currency = "USD",
                AvailableBalance = new Money { Value = 100.50m, Currency = "USD" },
                CreatedAt = originalDateTime,
                UpdatedAt = originalDateTime,
                IsDefault = false,
                Active = true,
                Type = "ACCOUNT",
                Ready = true,
                Hold = new Money { Value = 0, Currency = "USD" },
                RetailPortfolioId = "portfolio-123",
                Platform = "advanced_trade"
            };

            // Serialize to JSON
            var json = JsonConvert.SerializeObject(account);

            // Deserialize back
            var deserializedAccount = JsonConvert.DeserializeObject<Account>(json);

            // Verify DateTime precision is preserved
            var dateTimePreserved = deserializedAccount != null &&
                                  deserializedAccount.CreatedAt.HasValue &&
                                  deserializedAccount.UpdatedAt.HasValue &&
                                  Math.Abs((deserializedAccount.CreatedAt.Value - originalDateTime).TotalSeconds) < 1 &&
                                  Math.Abs((deserializedAccount.UpdatedAt.Value - originalDateTime).TotalSeconds) < 1;

            return dateTimePreserved;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// **Feature: coinbase-sdk, Property 16: Resilient deserialization**
    /// **Validates: Requirements 10.5**
    /// For any valid JSON response with additional unknown fields, deserialization should succeed and ignore extra fields without errors
    /// </summary>
    [Property(MaxTest = 100)]
    public bool ResilientDeserializationProperty(string extraFieldName, string extraFieldValue, decimal balance)
    {
        // Filter invalid inputs
        if (string.IsNullOrWhiteSpace(extraFieldName) || string.IsNullOrWhiteSpace(extraFieldValue) || 
            extraFieldName.Length < 2 || extraFieldValue.Length < 1)
            return true; // Skip invalid extra fields

        // Skip field names with special characters that might cause JSON issues
        if (extraFieldName.Contains("\"") || extraFieldName.Contains("\\") || extraFieldName.Contains("\n") || 
            extraFieldName.Contains("\r") || extraFieldName.Contains("\t"))
            return true; // Skip problematic characters

        // Skip field values with special characters that might cause JSON issues
        if (extraFieldValue.Contains("\"") || extraFieldValue.Contains("\\") || extraFieldValue.Contains("\n") || 
            extraFieldValue.Contains("\r") || extraFieldValue.Contains("\t"))
            return true; // Skip problematic characters

        // Ensure reasonable balance values
        if (balance < -1000000 || balance > 1000000)
            return true; // Skip extreme values

        // Avoid reserved property names
        var reservedNames = new[] { "uuid", "name", "currency", "available_balance", "default", "active", 
                                   "created_at", "updated_at", "deleted_at", "type", "ready", "hold", 
                                   "retail_portfolio_id", "platform" };
        if (reservedNames.Contains(extraFieldName.ToLower()))
            return true; // Skip reserved names

        try
        {
            // Create JSON with extra unknown field
            var jsonWithExtraField = $@"{{
                ""uuid"": ""test-account-123"",
                ""name"": ""Test Account"",
                ""currency"": ""USD"",
                ""available_balance"": {{
                    ""value"": ""{balance}"",
                    ""currency"": ""USD""
                }},
                ""default"": false,
                ""active"": true,
                ""created_at"": ""2023-01-01T00:00:00Z"",
                ""updated_at"": ""2023-01-01T00:00:00Z"",
                ""type"": ""ACCOUNT"",
                ""ready"": true,
                ""hold"": {{
                    ""value"": ""0"",
                    ""currency"": ""USD""
                }},
                ""retail_portfolio_id"": ""portfolio-456"",
                ""platform"": ""advanced_trade"",
                ""{extraFieldName}"": ""{extraFieldValue}""
            }}";

            // Attempt to deserialize - should succeed despite extra field
            var deserializedAccount = JsonConvert.DeserializeObject<Account>(jsonWithExtraField);

            // Verify deserialization succeeded and core properties are correct
            var deserializationSucceeded = deserializedAccount != null &&
                                         deserializedAccount.Uuid == "test-account-123" &&
                                         deserializedAccount.Name == "Test Account" &&
                                         deserializedAccount.Currency == "USD" &&
                                         deserializedAccount.AvailableBalance.Value == balance &&
                                         deserializedAccount.AvailableBalance.Currency == "USD" &&
                                         deserializedAccount.IsDefault == false &&
                                         deserializedAccount.Active == true &&
                                         deserializedAccount.Type == "ACCOUNT" &&
                                         deserializedAccount.Ready == true;

            return deserializationSucceeded;
        }
        catch
        {
            return false;
        }
    }
}
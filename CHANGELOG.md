# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2024-12-30

### Added

#### Core Features
- **JWT Authentication**: Automatic ES256 JWT token generation and management with proper claim handling
- **Account Management**: Complete account operations including retrieval by ID, name, and filtering
- **Trading Operations**: Full trading support with market orders, limit orders, order status, and cancellation
- **Market Data Access**: Real-time and historical market data including order books, products, and candles
- **Configuration Management**: Flexible configuration via environment variables or explicit parameters

#### Technical Features
- **Async/Await Support**: Full asynchronous operation support with cancellation token integration
- **Dependency Injection**: Built-in Microsoft.Extensions.DependencyInjection support with proper service lifetimes
- **Thread Safety**: Safe concurrent operations across all services
- **Comprehensive Error Handling**: Detailed exception hierarchy with specific error types
- **Strong Typing**: Complete type safety with comprehensive XML documentation
- **Multi-Target Framework**: Support for .NET 6.0 and .NET 8.0

#### API Coverage
- Account operations: `GetAccountsAsync`, `GetAccountAsync`, `GetAccountByNameAsync`
- Trading operations: `PlaceMarketOrderAsync`, `PlaceLimitOrderAsync`, `GetOrderAsync`, `CancelOrderAsync`, `GetOrdersAsync`
- Market data operations: `GetProductBookAsync`, `GetProductsAsync`, `GetProductAsync`, `GetCandlesAsync`

#### Models and Enums
- Complete data models: `Account`, `Order`, `Money`, `Product`, `ProductBook`, `Candle`
- Request models: `MarketOrderRequest`, `LimitOrderRequest`, `OrderFilter`
- Enumerations: `OrderSide`, `OrderType`, `OrderStatus`, `TimeInForce`, `Granularity`

#### Exception Types
- `CoinbaseException` (base class)
- `CoinbaseAuthenticationException` for authentication failures
- `CoinbaseApiException` for API-related errors with HTTP status codes
- `CoinbaseNetworkException` for network connectivity issues
- `CoinbaseConfigurationException` for configuration problems

#### Factory and Extensions
- `CoinbaseClientFactory` for easy client creation without DI
- `ServiceCollectionExtensions` for dependency injection registration
- Support for environment variable configuration
- Flexible service registration patterns

### Dependencies
- System.IdentityModel.Tokens.Jwt (8.1.x)
- Newtonsoft.Json (13.0.x)
- Microsoft.Extensions.DependencyInjection.Abstractions (6.0.x - 8.0.x)
- Microsoft.Extensions.Options (6.0.x - 8.0.x)
- Microsoft.Extensions.Options.ConfigurationExtensions (6.0.x - 8.0.x)

### Documentation
- Complete XML documentation for all public APIs
- Comprehensive README with usage examples
- IntelliSense support for all public members
- Property-based testing for correctness verification

### Package Features
- NuGet package with proper metadata and versioning
- Symbol packages for debugging support
- Multi-framework targeting for broad compatibility
- Appropriate dependency version ranges for flexibility
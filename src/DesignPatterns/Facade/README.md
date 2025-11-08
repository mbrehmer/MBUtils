# Facade Design Pattern

This directory contains a small, well-typed implementation of the Facade design pattern supporting both synchronous and asynchronous execution.

Namespace: `MBUtils.DesignPatterns.Facade`

## Components

- `IFacade` and `IFacade<TResult>`
    - The core facade contracts. `IFacade` exposes a parameterless `Execute()` method for operations without a return value, while `IFacade<TResult>` returns a result.

- `IAsyncFacade` and `IAsyncFacade<TResult>`
    - Asynchronous facade contracts. Expose `ExecuteAsync(CancellationToken)` methods for async operations, with or without a return value.

- `FacadeBase` and `FacadeBase<TResult>`
    - Abstract base classes that implement `IFacade` and `IFacade<TResult>` explicitly and expose protected core methods:
    - `protected abstract void ExecuteCore()` (for `FacadeBase`)
    - `protected abstract TResult ExecuteCore()` (for `FacadeBase<TResult>`)
    - Use these bases to implement concrete facades while keeping the public interface explicit.

- `AsyncFacadeBase` and `AsyncFacadeBase<TResult>`
    - Abstract base classes for asynchronous facades that implement `IAsyncFacade` and `IAsyncFacade<TResult>` explicitly and expose protected core methods:
    - `protected abstract Task ExecuteAsyncCore(CancellationToken cancellationToken)` (for `AsyncFacadeBase`)
    - `protected abstract Task<TResult> ExecuteAsyncCore(CancellationToken cancellationToken)` (for `AsyncFacadeBase<TResult>`)

## Quick examples

### Synchronous facade (void)

```csharp
using MBUtils.DesignPatterns.Facade;

// Simulated subsystem components
private sealed class EmailService
{
    public void SendEmail(string to, string message) { /* ... */ }
}

private sealed class LoggingService
{
    public void LogNotification(string details) { /* ... */ }
}

// Facade implementation
private sealed class NotificationFacade : FacadeBase
{
    private readonly EmailService _emailService;
    private readonly LoggingService _loggingService;

    public NotificationFacade()
    {
        _emailService = new EmailService();
        _loggingService = new LoggingService();
    }

    protected override void ExecuteCore()
    {
        _emailService.SendEmail("user@example.com", "Hello!");
        _loggingService.LogNotification("Email sent to user@example.com");
    }
}

IFacade facade = new NotificationFacade();
facade.Execute(); // Sends email and logs notification
```

### Synchronous facade with result

```csharp
// Simulated subsystem components
private sealed class InventoryService
{
    public int GetStock(string productId) => 42;
}

private sealed class PricingService
{
    public decimal GetPrice(string productId) => 19.99m;
}

// Facade implementation
private sealed class ProductInfoFacade : FacadeBase<string>
{
    private readonly InventoryService _inventory;
    private readonly PricingService _pricing;
    private readonly string _productId;

    public ProductInfoFacade(string productId)
    {
        _productId = productId;
        _inventory = new InventoryService();
        _pricing = new PricingService();
    }

    protected override string ExecuteCore()
    {
        int stock = _inventory.GetStock(_productId);
        decimal price = _pricing.GetPrice(_productId);
        return $"Product {_productId}: {stock} in stock at ${price}";
    }
}

IFacade<string> facade = new ProductInfoFacade("PROD-123");
string info = facade.Execute(); // "Product PROD-123: 42 in stock at $19.99"
```

### Asynchronous facade

```csharp
// Simulated async subsystem components
private sealed class AsyncEmailService
{
    public async Task SendEmailAsync(string to, string message, CancellationToken cancellationToken)
    {
        await Task.Delay(100, cancellationToken);
    }
}

private sealed class AsyncLoggingService
{
    public async Task LogNotificationAsync(string details, CancellationToken cancellationToken)
    {
        await Task.Delay(50, cancellationToken);
    }
}

// Async facade implementation
private sealed class AsyncNotificationFacade : AsyncFacadeBase
{
    private readonly AsyncEmailService _emailService;
    private readonly AsyncLoggingService _loggingService;

    public AsyncNotificationFacade()
    {
        _emailService = new AsyncEmailService();
        _loggingService = new AsyncLoggingService();
    }

    protected override async Task ExecuteAsyncCore(CancellationToken cancellationToken)
    {
        await _emailService.SendEmailAsync("user@example.com", "Hello!", cancellationToken);
        await _loggingService.LogNotificationAsync("Email sent to user@example.com", cancellationToken);
    }
}

IAsyncFacade facade = new AsyncNotificationFacade();
await facade.ExecuteAsync(CancellationToken.None);
```

### Asynchronous facade with result

```csharp
// Async facade with result
private sealed class AsyncProductInfoFacade : AsyncFacadeBase<string>
{
    private readonly AsyncInventoryService _inventory;
    private readonly AsyncPricingService _pricing;
    private readonly string _productId;

    public AsyncProductInfoFacade(string productId)
    {
        _productId = productId;
        _inventory = new AsyncInventoryService();
        _pricing = new AsyncPricingService();
    }

    protected override async Task<string> ExecuteAsyncCore(CancellationToken cancellationToken)
    {
        int stock = await _inventory.GetStockAsync(_productId, cancellationToken);
        decimal price = await _pricing.GetPriceAsync(_productId, cancellationToken);
        return $"Product {_productId}: {stock} in stock at ${price}";
    }
}

IAsyncFacade<string> facade = new AsyncProductInfoFacade("PROD-123");
string info = await facade.ExecuteAsync(CancellationToken.None);
```

## Design notes

- The Facade pattern simplifies interaction with complex subsystems by providing a unified, high-level interface.
- Both sync and async execution are first-class. Use the appropriate base class depending on whether your subsystem components are synchronous or asynchronous.
- `IFacade` and `IFacade<TResult>` model operations without or with return values, respectively.
- The facade does not prevent clients from accessing subsystem components directly, but it provides a simpler alternative for common use cases.

## Benefits

- Simplifies complex subsystem interactions by providing a single entry point
- Reduces coupling between client code and subsystem components
- Makes the subsystem easier to use and understand
- Supports both synchronous and asynchronous workflows
- Can be easily extended or replaced without affecting client code

## Testing

Unit tests for the Facade implementation are located in `tests/DesignPatterns/Facade` and cover both sync and async execution with and without return values.

Run the facade tests with:

```pwsh
dotnet test --filter "FullyQualifiedName~MBUtils.Tests.DesignPatterns.Facade"
```

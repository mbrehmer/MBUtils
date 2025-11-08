# Observer Design Pattern

A thread-safe implementation of the Observer pattern supporting both synchronous and asynchronous observers.

Namespace: `MBUtils.DesignPatterns.Observer`

## Interfaces

- `IObserver<T>`
	- Synchronous observer contract with `void Update(T data)`.
	- Receives notifications from subjects when their state changes.
- `IAsyncObserver<T>`
	- Asynchronous observer contract with `Task UpdateAsync(T data, CancellationToken cancellationToken)`.
	- Supports observers that need to perform I/O-bound or long-running operations in response to notifications.
- `ISubject<T>`
	- Subject interface that manages observers and sends notifications.
	- Provides methods to attach/detach both synchronous and asynchronous observers.
	- Supports both synchronous `Notify(T data)` and asynchronous `NotifyAsync(T data, CancellationToken)` notification methods.

## SubjectBase<T>

`SubjectBase<T>` is an abstract base class providing a complete, thread-safe implementation of `ISubject<T>`.

Key features:

- Thread-safety: All public and protected members are safe for concurrent access.
- Separate observer lists: Maintains separate collections for synchronous and asynchronous observers.
- Snapshot notification: Takes a snapshot of observer lists before notifying to avoid issues if observers are attached/detached during notification.
- Exception propagation: If an observer throws an exception during notification, it propagates to the caller and subsequent observers are not notified.

Protected members for derived classes:

- `void Attach(IObserver<T> observer)` – Attach a synchronous observer
- `void Detach(IObserver<T> observer)` – Detach a synchronous observer
- `void Notify(T data)` – Notify all synchronous observers
- `void AttachAsync(IAsyncObserver<T> observer)` – Attach an asynchronous observer
- `void DetachAsync(IAsyncObserver<T> observer)` – Detach an asynchronous observer
- `Task NotifyAsync(T data, CancellationToken cancellationToken)` – Notify all observers asynchronously

## Quick start

### Simple synchronous observer

```csharp
using MBUtils.DesignPatterns.Observer;

// Define a concrete subject
public sealed class TemperatureSensor : SubjectBase<double>
{
    private double _temperature;
    
    public double Temperature
    {
        get => _temperature;
        set
        {
            _temperature = value;
            Notify(_temperature);
        }
    }
}

// Define a concrete observer
public sealed class DisplayObserver : IObserver<double>
{
    public double LastTemperature { get; private set; }
    
    void IObserver<double>.Update(double data)
    {
        LastTemperature = data;
        Console.WriteLine($"Temperature updated: {data}°C");
    }
}

// Usage
TemperatureSensor sensor = new TemperatureSensor();
DisplayObserver display = new DisplayObserver();

ISubject<double> subject = sensor;
subject.Attach(display);

sensor.Temperature = 25.5; // Output: Temperature updated: 25.5°C
sensor.Temperature = 26.0; // Output: Temperature updated: 26.0°C

subject.Detach(display);
sensor.Temperature = 27.0; // No output, observer detached
```

### Asynchronous observer

```csharp
using MBUtils.DesignPatterns.Observer;

// Define an async observer that logs to a database
public sealed class DatabaseLogger : IAsyncObserver<string>
{
    private readonly IDatabase _database;
    
    public DatabaseLogger(IDatabase database)
    {
        _database = database;
    }
    
    async Task IAsyncObserver<string>.UpdateAsync(string data, CancellationToken cancellationToken)
    {
        await _database.LogAsync($"Event occurred: {data}", cancellationToken);
    }
}

// Define a subject that publishes events
public sealed class EventPublisher : SubjectBase<string>
{
    public async Task PublishEventAsync(string eventData, CancellationToken cancellationToken = default)
    {
        await NotifyAsync(eventData, cancellationToken);
    }
}

// Usage
EventPublisher publisher = new EventPublisher();
DatabaseLogger logger = new DatabaseLogger(database);

ISubject<string> subject = publisher;
subject.AttachAsync(logger);

await publisher.PublishEventAsync("User logged in");
```

### Mixed synchronous and asynchronous observers

```csharp
using MBUtils.DesignPatterns.Observer;

// Define a subject
public sealed class StockTicker : SubjectBase<decimal>
{
    private decimal _price;
    
    public decimal Price
    {
        get => _price;
        set
        {
            _price = value;
            Notify(_price);
        }
    }
    
    public async Task UpdatePriceAsync(decimal newPrice, CancellationToken cancellationToken = default)
    {
        _price = newPrice;
        await NotifyAsync(_price, cancellationToken);
    }
}

// Synchronous observer
public sealed class ConsoleObserver : IObserver<decimal>
{
    void IObserver<decimal>.Update(decimal data)
    {
        Console.WriteLine($"Current price: ${data}");
    }
}

// Asynchronous observer
public sealed class AlertService : IAsyncObserver<decimal>
{
    private readonly decimal _threshold;
    private readonly INotificationService _notificationService;
    
    public AlertService(decimal threshold, INotificationService notificationService)
    {
        _threshold = threshold;
        _notificationService = notificationService;
    }
    
    async Task IAsyncObserver<decimal>.UpdateAsync(decimal data, CancellationToken cancellationToken)
    {
        if (data > _threshold)
        {
            await _notificationService.SendAlertAsync($"Price exceeded threshold: ${data}", cancellationToken);
        }
    }
}

// Usage
StockTicker ticker = new StockTicker();
ConsoleObserver console = new ConsoleObserver();
AlertService alerts = new AlertService(100.0m, notificationService);

ISubject<decimal> subject = ticker;
subject.Attach(console);
subject.AttachAsync(alerts);

ticker.Price = 95.0m; // Console output only
await ticker.UpdatePriceAsync(105.0m); // Console output + alert sent
```

## Design notes and guarantees

- Thread-safety: `SubjectBase<T>` uses internal locking to ensure thread-safe observer management and notification.
- Snapshot pattern: Observer lists are copied before notification to prevent issues if observers are added/removed during notification.
- Exception handling: Exceptions thrown by observers propagate to the caller. Consider wrapping observer calls in try-catch if you need different behavior.
- Notification order: Observers are notified in the order they were attached.
- Null safety: All public methods throw `ArgumentNullException` if passed a null observer.
- Explicit interface implementation: `SubjectBase<T>` implements `ISubject<T>` explicitly and exposes protected methods for derived classes.

## Testing

The Observer pattern implementation includes comprehensive unit tests covering:

- **SubjectBase**: Attach/detach operations, synchronous and asynchronous notifications, mixed observers, thread-safety, exception handling

All tests follow xUnit conventions and can be run with:

```pwsh
dotnet test --filter "FullyQualifiedName~MBUtils.Tests.DesignPatterns.Observer"
```

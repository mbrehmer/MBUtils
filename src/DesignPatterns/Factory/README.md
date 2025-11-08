# Factory Pattern

The Factory Pattern implementation in MBUtils provides a flexible, thread-safe way to create objects based on keys or configuration. It supports both synchronous and asynchronous creation scenarios.

## Overview

The Factory Pattern encapsulates object creation logic, allowing you to:
- Decouple object instantiation from usage
- Select concrete implementations at runtime based on a key
- Centralize and reuse complex object creation logic
- Support both synchronous and asynchronous initialization

## Key Components

### Interfaces

#### `IFactory<T>`
A simple factory that creates instances of type `T`.

```csharp
public interface IFactory<out T>
{
    T Create();
}
```

#### `IFactory<TKey, T>`
A keyed factory that creates instances based on a key.

```csharp
public interface IFactory<in TKey, out T> where TKey : notnull
{
    T Create(TKey key);
    bool IsRegistered(TKey key);
}
```

#### `IAsyncFactory<T>`
An asynchronous factory for async initialization.

```csharp
public interface IAsyncFactory<T>
{
    Task<T> CreateAsync(CancellationToken cancellationToken = default);
}
```

#### `IAsyncFactory<TKey, T>`
A keyed asynchronous factory.

```csharp
public interface IAsyncFactory<in TKey, T> where TKey : notnull
{
    Task<T> CreateAsync(TKey key, CancellationToken cancellationToken = default);
    bool IsRegistered(TKey key);
}
```

### Base Classes

#### `FactoryBase<TKey, T>`
Thread-safe base implementation for keyed factories with registration/resolution logic.

**Key Features:**
- Thread-safe registration and resolution
- Protected `Register()`, `Unregister()`, and `ClearRegistrations()` methods
- Custom `IEqualityComparer<TKey>` support
- Throws `InvalidOperationException` when key is not registered

#### `AsyncFactoryBase<TKey, T>`
Thread-safe base implementation for keyed asynchronous factories.

**Key Features:**
- All features of `FactoryBase<TKey, T>`
- Async creator function support with `CancellationToken`
- Overload to register synchronous creators (wrapped in `Task.FromResult`)

## Usage Examples

### Example 1: Simple Document Factory

```csharp
using MBUtils.DesignPatterns.Factory;

// Define document types
public interface IDocument
{
    string GetContent();
}

public class TextDocument : IDocument
{
    public string GetContent() => "Plain text content";
}

public class HtmlDocument : IDocument
{
    public string GetContent() => "<html>HTML content</html>";
}

public class PdfDocument : IDocument
{
    public string GetContent() => "PDF binary content";
}

// Create a concrete factory
public enum DocumentType
{
    Text,
    Html,
    Pdf
}

public class DocumentFactory : FactoryBase<DocumentType, IDocument>
{
    public DocumentFactory()
    {
        Register(DocumentType.Text, () => new TextDocument());
        Register(DocumentType.Html, () => new HtmlDocument());
        Register(DocumentType.Pdf, () => new PdfDocument());
    }
}

// Usage
DocumentFactory factory = new DocumentFactory();

IDocument textDoc = factory.Create(DocumentType.Text);
IDocument htmlDoc = factory.Create(DocumentType.Html);
IDocument pdfDoc = factory.Create(DocumentType.Pdf);

Console.WriteLine(textDoc.GetContent()); // "Plain text content"
Console.WriteLine(htmlDoc.GetContent()); // "<html>HTML content</html>"

// Check if registered
bool isRegistered = factory.IsRegistered(DocumentType.Html); // true
```

### Example 2: Database Connection Factory with Configuration

```csharp
using MBUtils.DesignPatterns.Factory;

public interface IDatabaseConnection
{
    void Connect();
}

public class SqlServerConnection : IDatabaseConnection
{
    private readonly string _connectionString;

    public SqlServerConnection(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void Connect()
    {
        Console.WriteLine($"Connecting to SQL Server: {_connectionString}");
    }
}

public class PostgresConnection : IDatabaseConnection
{
    private readonly string _connectionString;

    public PostgresConnection(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void Connect()
    {
        Console.WriteLine($"Connecting to PostgreSQL: {_connectionString}");
    }
}

public class DatabaseConnectionFactory : FactoryBase<string, IDatabaseConnection>
{
    private readonly Dictionary<string, string> _connectionStrings;

    public DatabaseConnectionFactory(Dictionary<string, string> connectionStrings)
    {
        _connectionStrings = connectionStrings;

        // Register creators that capture configuration
        Register("sqlserver", () => new SqlServerConnection(
            _connectionStrings["sqlserver"]));
        Register("postgres", () => new PostgresConnection(
            _connectionStrings["postgres"]));
    }
}

// Usage
Dictionary<string, string> config = new Dictionary<string, string>
{
    { "sqlserver", "Server=localhost;Database=MyDb;" },
    { "postgres", "Host=localhost;Database=MyDb;" }
};

DatabaseConnectionFactory factory = new DatabaseConnectionFactory(config);

IDatabaseConnection sqlConn = factory.Create("sqlserver");
IDatabaseConnection pgConn = factory.Create("postgres");

sqlConn.Connect(); // "Connecting to SQL Server: Server=localhost;Database=MyDb;"
pgConn.Connect();  // "Connecting to PostgreSQL: Host=localhost;Database=MyDb;"
```

### Example 3: Async Service Factory

```csharp
using System.Threading;
using System.Threading.Tasks;
using MBUtils.DesignPatterns.Factory;

public interface IExternalService
{
    Task<string> FetchDataAsync();
}

public class ApiService : IExternalService
{
    private readonly string _apiUrl;

    public ApiService(string apiUrl)
    {
        _apiUrl = apiUrl;
    }

    public async Task<string> FetchDataAsync()
    {
        // Simulate API call
        await Task.Delay(100);
        return $"Data from {_apiUrl}";
    }
}

public class CachedApiService : IExternalService
{
    private readonly string _apiUrl;
    private string? _cache;

    public CachedApiService(string apiUrl)
    {
        _apiUrl = apiUrl;
    }

    public async Task<string> FetchDataAsync()
    {
        if (_cache != null)
        {
            return _cache;
        }

        await Task.Delay(100);
        _cache = $"Cached data from {_apiUrl}";
        return _cache;
    }
}

public enum ServiceType
{
    Standard,
    Cached
}

public class ExternalServiceFactory : AsyncFactoryBase<ServiceType, IExternalService>
{
    private readonly string _baseUrl;

    public ExternalServiceFactory(string baseUrl)
    {
        _baseUrl = baseUrl;

        // Register async creators
        Register(ServiceType.Standard, async ct =>
        {
            // Simulate async initialization
            await Task.Delay(10, ct);
            return new ApiService(_baseUrl);
        });

        Register(ServiceType.Cached, async ct =>
        {
            await Task.Delay(10, ct);
            return new CachedApiService(_baseUrl);
        });
    }
}

// Usage
ExternalServiceFactory factory = new ExternalServiceFactory("https://api.example.com");

IExternalService standardService = await factory.CreateAsync(ServiceType.Standard);
IExternalService cachedService = await factory.CreateAsync(ServiceType.Cached);

string data1 = await standardService.FetchDataAsync();
string data2 = await cachedService.FetchDataAsync();
```

### Example 4: Dynamic Registration

```csharp
using MBUtils.DesignPatterns.Factory;

public interface IPlugin
{
    string Execute();
}

public class PluginFactory : FactoryBase<string, IPlugin>
{
    // Factory starts empty, plugins are registered at runtime
    public void RegisterPlugin(string name, Func<IPlugin> creator)
    {
        Register(name, creator);
    }

    public void UnregisterPlugin(string name)
    {
        Unregister(name);
    }

    public int GetPluginCount()
    {
        return RegistrationCount;
    }
}

// Usage
PluginFactory factory = new PluginFactory();

// Register plugins dynamically
factory.RegisterPlugin("logger", () => new LoggerPlugin());
factory.RegisterPlugin("validator", () => new ValidatorPlugin());

IPlugin logger = factory.Create("logger");
IPlugin validator = factory.Create("validator");

int count = factory.GetPluginCount(); // 2

// Unregister
factory.UnregisterPlugin("logger");
bool isStillRegistered = factory.IsRegistered("logger"); // false
```

### Example 5: String-based Factory with Case-Insensitive Keys

```csharp
using System;
using MBUtils.DesignPatterns.Factory;

public interface IFormatter
{
    string Format(object value);
}

public class JsonFormatter : IFormatter
{
    public string Format(object value) => $"{{ \"value\": \"{value}\" }}";
}

public class XmlFormatter : IFormatter
{
    public string Format(object value) => $"<value>{value}</value>";
}

public class FormatterFactory : FactoryBase<string, IFormatter>
{
    public FormatterFactory() 
        : base(StringComparer.OrdinalIgnoreCase)
    {
        Register("json", () => new JsonFormatter());
        Register("xml", () => new XmlFormatter());
    }
}

// Usage
FormatterFactory factory = new FormatterFactory();

// Case-insensitive key matching
IFormatter formatter1 = factory.Create("JSON");
IFormatter formatter2 = factory.Create("json");
IFormatter formatter3 = factory.Create("Json");

// All create the same type
Console.WriteLine(formatter1.Format("test")); // { "value": "test" }
Console.WriteLine(formatter2.Format("test")); // { "value": "test" }
Console.WriteLine(formatter3.Format("test")); // { "value": "test" }
```

## Design Considerations

### When to Use Factory Pattern

**Use when:**
- You need to decouple object creation from usage
- The concrete type to instantiate is determined at runtime
- You have complex object creation logic that should be centralized
- You want to provide multiple implementations of an interface
- Object creation involves configuration or resource initialization

**Don't use when:**
- Object creation is trivial (e.g., `new MyClass()`)
- You have a single concrete implementation that won't change
- Direct instantiation is clearer and simpler

### Thread Safety

Both `FactoryBase<TKey, T>` and `AsyncFactoryBase<TKey, T>` are thread-safe:
- Registration and unregistration operations are protected by a lock
- Multiple threads can safely call `Create()` or `CreateAsync()` simultaneously
- Creator functions themselves should be thread-safe

### Exception Handling

The factory implementations throw:
- `ArgumentNullException`: When `key`, `creator`, or `comparer` is null
- `InvalidOperationException`: When attempting to create with an unregistered key or registering a duplicate key

### Async Best Practices

When using `AsyncFactoryBase<TKey, T>`:
- Pass `CancellationToken` through to enable cancellation
- Use `ConfigureAwait(false)` in library code (already done in base class)
- Consider whether initialization truly needs to be async
- Use the synchronous `Register(TKey, Func<T>)` overload for simple cases

## Integration with Other Patterns

### Factory + Strategy
Factories can create different strategy implementations:

```csharp
IStrategy<Context, Result> strategy = strategyFactory.Create(strategyType);
```

### Factory + Singleton
Factories can manage singleton instances:

```csharp
public class SingletonFactory : FactoryBase<Type, object>
{
    private readonly Dictionary<Type, object> _singletons = new();

    protected void RegisterSingleton<T>(Type key, T instance)
    {
        _singletons[key] = instance;
        Register(key, () => (T)_singletons[key]);
    }
}
```

### Factory + Builder
Factories can use builders internally:

```csharp
Register("complex", () => 
{
    ComplexObjectBuilder builder = new ComplexObjectBuilder();
    return builder.WithProperty1("value").WithProperty2(42).Build();
});
```

## Testing

The factory pattern is highly testable:

```csharp
// Test registration
[Fact]
public void Create_WithRegisteredKey_ReturnsInstance()
{
    TestFactory factory = new TestFactory();
    factory.RegisterTestCreator("test", () => new TestObject());
    
    TestObject result = factory.Create("test");
    
    Assert.NotNull(result);
}

// Test exception behavior
[Fact]
public void Create_WithUnregisteredKey_ThrowsInvalidOperationException()
{
    TestFactory factory = new TestFactory();
    
    Assert.Throws<InvalidOperationException>(() => factory.Create("unknown"));
}

// Test thread safety
[Fact]
public void Create_Concurrently_IsThreadSafe()
{
    TestFactory factory = new TestFactory();
    factory.RegisterTestCreator("test", () => new TestObject());
    
    Parallel.For(0, 1000, i =>
    {
        TestObject obj = factory.Create("test");
        Assert.NotNull(obj);
    });
}
```

## See Also

- [Builder Pattern](../Builder/README.md) - For complex object construction
- [Singleton Pattern](../Singleton/README.md) - For single-instance management
- [Strategy Pattern](../Strategy/README.md) - Often used with factories to select algorithms

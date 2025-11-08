# Singleton Design Pattern

## Overview

The Singleton pattern ensures that a class has only one instance throughout the application lifetime and provides a global point of access to that instance. This implementation provides a thread-safe, lazy-initialized singleton base class.

## Implementation

### `SingletonBase<T>`

An abstract base class that provides a thread-safe singleton implementation using .NET's `Lazy<T>` for optimal performance and simplicity.

**Key Features:**
- **Thread-Safe**: Uses `Lazy<T>` with `ExecutionAndPublication` mode to ensure thread-safe lazy initialization
- **Lazy Initialization**: Instance is created only when first accessed
- **Flexible Constructors**: Supports private, protected, or public parameterless constructors
- **Initialization Hook**: Provides `OnInstanceCreated()` virtual method for post-construction initialization
- **Testability**: Includes `ResetInstance()` method for unit testing scenarios

## Usage

### Basic Singleton

```csharp
public class Logger : SingletonBase<Logger>
{
    private Logger()
    {
        // Private constructor prevents direct instantiation
    }
    
    public void Log(string message)
    {
        Console.WriteLine($"[{DateTime.Now}] {message}");
    }
}

// Usage:
Logger.Instance.Log("Application started");
```

### Singleton with Initialization

```csharp
public class Configuration : SingletonBase<Configuration>
{
    public Dictionary<string, string> Settings { get; private set; }
    
    private Configuration()
    {
        Settings = new Dictionary<string, string>();
    }
    
    protected override void OnInstanceCreated()
    {
        // Load configuration after instance is created
        LoadSettings();
    }
    
    private void LoadSettings()
    {
        // Load from file, environment, etc.
        Settings["AppName"] = "MyApp";
        Settings["Version"] = "1.0.0";
    }
}

// Usage:
string appName = Configuration.Instance.Settings["AppName"];
```

### Testing with Reset

```csharp
public class TestableService : SingletonBase<TestableService>
{
    public int CallCount { get; private set; }
    
    private TestableService()
    {
    }
    
    public void DoWork()
    {
        CallCount++;
    }
    
    // Expose reset for testing
    public static void Reset()
    {
        ResetInstance();
    }
}

// In test:
[Fact]
public void Test_SingletonBehavior()
{
    TestableService.Reset(); // Clear previous state
    
    TestableService instance = TestableService.Instance;
    instance.DoWork();
    
    Assert.Equal(1, instance.CallCount);
}
```

## Design Considerations

### When to Use

- You need exactly one instance of a class throughout the application
- The instance needs to be globally accessible
- You want lazy initialization for resource optimization
- Thread-safety is a requirement

### When NOT to Use

- You need multiple instances with shared behavior (consider Factory pattern)
- The singleton has mutable state that complicates testing
- You need dependency injection or IoC containers (they can manage singleton lifetime better)
- The singleton creates tight coupling in your codebase

### Thread Safety

The implementation uses .NET's `Lazy<T>` with `LazyThreadSafetyMode.ExecutionAndPublication`:

- **Lazy initialization**: Instance is created only on first access
- **Thread-safe**: `Lazy<T>` ensures only one thread executes the factory method
- **No locking overhead**: After initialization, access is lock-free
- **Exception caching**: If initialization fails, the exception is cached and re-thrown on subsequent accesses

This approach provides optimal performance while maintaining thread safety through the framework's well-tested implementation.

## Best Practices

1. **Keep constructors private or protected** to prevent direct instantiation
2. **Use `OnInstanceCreated()`** for initialization logic rather than in the constructor
3. **Avoid mutable state** when possible to simplify testing and reasoning
4. **Document thread-safety** characteristics of your singleton methods
5. **Use `ResetInstance()`** only in test code, never in production
6. **Consider alternatives** like dependency injection for better testability

## Implementation Notes

- Uses reflection to invoke parameterless constructors (private, protected, or public)
- Throws `InvalidOperationException` if no parameterless constructor exists
- `OnInstanceCreated()` is called once immediately after instance creation
- `ResetInstance()` is `protected` to discourage misuse but allow testing

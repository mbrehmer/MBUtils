# Decorator Design Pattern

This directory contains a flexible, well-typed implementation of the Decorator design pattern supporting both synchronous and asynchronous execution and composite decoration for chaining multiple decorators.

Namespace: `MBUtils.DesignPatterns.Decorator`

## Components

- `IDecorator<T>`
    - The core decorator contract. Wraps a component of type `T` and provides an `Execute()` method that returns the decorated result.
    - Exposes the wrapped `Component` property.

- `IAsyncDecorator<T>`
    - The asynchronous decorator contract. Wraps a component of type `T` and provides an `ExecuteAsync(CancellationToken)` method.
    - Exposes the wrapped `Component` property.

- `DecoratorBase<T>`
    - Abstract base class that implements `IDecorator<T>` explicitly and exposes a protected core method:
    - `protected abstract T ExecuteCore()`
    - Use this base to implement concrete decorators while keeping the public interface explicit.
    - The constructor ensures the component is not null.

- `AsyncDecoratorBase<T>`
    - Abstract base class that implements `IAsyncDecorator<T>` explicitly and exposes a protected core method:
    - `protected abstract Task<T> ExecuteAsyncCore(CancellationToken cancellationToken)`
    - Use this base for decorators that perform asynchronous work.

- `CompositeDecorator<T>`
    - Chains multiple decorators together, applying them from innermost to outermost.
    - Accepts a list of decorator factories (functions that create decorators given a component).
    - Supports both synchronous and asynchronous decorator chains (but not mixed in the same composite).

## Quick examples

### Simple synchronous decorator (logging)

```csharp
using MBUtils.DesignPatterns.Decorator;

// A simple decorator that logs before returning the component
private sealed class LoggingDecorator : DecoratorBase<string>
{
    public LoggingDecorator(string component) : base(component) { }

    protected override string ExecuteCore()
    {
        Console.WriteLine($"Processing: {Component}");
        return Component;
    }
}

IDecorator<string> decorator = new LoggingDecorator("Hello World");
string result = decorator.Execute(); // Logs and returns "Hello World"
```

### Asynchronous decorator (with delay)

```csharp
private sealed class DelayDecorator : AsyncDecoratorBase<string>
{
    private readonly int _delayMs;

    public DelayDecorator(string component, int delayMs) : base(component)
    {
        _delayMs = delayMs;
    }

    protected override async Task<string> ExecuteAsyncCore(CancellationToken cancellationToken)
    {
        await Task.Delay(_delayMs, cancellationToken).ConfigureAwait(false);
        return Component;
    }
}

IAsyncDecorator<string> decorator = new DelayDecorator("Data", 100);
string result = await decorator.ExecuteAsync(CancellationToken.None);
```

### Composite decorator (chaining multiple decorators)

```csharp
// Example: decorate a string with prefix and suffix
private sealed class PrefixDecorator : DecoratorBase<string>
{
    private readonly string _prefix;

    public PrefixDecorator(string component, string prefix) : base(component)
    {
        _prefix = prefix;
    }

    protected override string ExecuteCore() => _prefix + Component;
}

private sealed class SuffixDecorator : DecoratorBase<string>
{
    private readonly string _suffix;

    public SuffixDecorator(string component, string suffix) : base(component)
    {
        _suffix = suffix;
    }

    protected override string ExecuteCore() => Component + _suffix;
}

// Chain decorators using CompositeDecorator
List<Func<string, IDecorator<string>>> factories = new List<Func<string, IDecorator<string>>>
{
    component => new PrefixDecorator(component, "["),
    component => new SuffixDecorator(component, "]")
};

IDecorator<string> composite = new CompositeDecorator<string>("Hello", factories.AsReadOnly());
string result = composite.Execute(); // Returns "[Hello]"
```

## Use cases

- **Cross-cutting concerns**: Add logging, caching, validation, or error handling around existing components
- **Processing pipelines**: Build chains of transformations or enrichments
- **Middleware patterns**: Wrap requests/responses with additional behavior
- **Feature toggles**: Conditionally add functionality at runtime
- **Aspect-oriented programming**: Apply aspects like timing, retry logic, or authorization

## Design notes

- Both sync and async decorators are first-class. Choose the appropriate interface based on whether your decorator performs asynchronous work.
- `DecoratorBase<T>` and `AsyncDecoratorBase<T>` handle null checks and explicit interface implementation, allowing concrete decorators to focus on their core behavior.
- `CompositeDecorator<T>` uses factory functions to create decorators, allowing each decorator to wrap the result of the previous one.
- The decorator pattern differs from inheritance by allowing behavior to be added dynamically and composed at runtime.

## Benefits

- Add responsibilities to objects dynamically without modifying their classes
- More flexible than static inheritance
- Composable: decorators can be chained in any order
- Single Responsibility Principle: each decorator focuses on one concern
- Open/Closed Principle: extend behavior without modifying existing code

## Testing

Unit tests for the Decorator implementation are located in `tests/DesignPatterns/Decorator` and cover synchronous/asynchronous decoration, null handling, and composite chaining.

Run the decorator tests with:

```pwsh
dotnet test --filter "FullyQualifiedName~MBUtils.Tests.DesignPatterns.Decorator"
```

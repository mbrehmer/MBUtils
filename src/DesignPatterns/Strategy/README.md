# Strategy Design Pattern

This directory contains a small, well-typed implementation of the Strategy design pattern supporting both synchronous and asynchronous execution and a composite that chains strategies.

Namespace: `MBUtils.DesignPatterns.Strategy`

## Components

- `IStrategy<TContext, TResult>`
    - The core strategy contract. Exposes synchronous `Execute(TContext)` and asynchronous `ExecuteAsync(TContext, CancellationToken)` methods.

- `StrategyBase<TContext, TResult>`
    - Abstract base class that implements `IStrategy<TContext, TResult>` explicitly and exposes protected core methods:
    - `protected abstract TResult ExecuteCore(TContext context)`
    - `protected virtual Task<TResult> ExecuteAsyncCore(TContext context, CancellationToken cancellationToken)` (default calls `ExecuteCore` and wraps the result)
    - Use this base to implement concrete strategies while keeping the public interface explicit.

- `CompositeStrategy<TContext, TResult>`
    - Executes a sequence of strategies in order. The output of each strategy is fed as the input (context) of the next.
    - Requires that `TContext` and `TResult` are the same runtime type so outputs can be safely re-used as inputs. The constructor will throw an `InvalidOperationException` if the types differ.

## Quick examples

### Synchronous strategy (increment)

```csharp
using MBUtils.DesignPatterns.Strategy;

private sealed class IncrementStrategy : StrategyBase<int, int>
{
 protected override int ExecuteCore(int context) => context +1;
}

IStrategy<int, int> strategy = new IncrementStrategy();
int result = strategy.Execute(41); //42
```

### Asynchronous strategy

```csharp
private sealed class AsyncIncrementStrategy : StrategyBase<int, int>
{
 protected override int ExecuteCore(int context) => context +1;

 protected override async Task<int> ExecuteAsyncCore(int context, CancellationToken cancellationToken)
 {
 await Task.Delay(1, cancellationToken).ConfigureAwait(false);
 return ExecuteCore(context);
 }
}

int result = await new AsyncIncrementStrategy().ExecuteAsync(100, CancellationToken.None); //101
```

### Composite strategy (chaining)

```csharp
List<IStrategy<int, int>> list = new List<IStrategy<int, int>>
{
 new MultiplyByTwoStrategy(),
 new AddTenStrategy()
};

IStrategy<int, int> composite = new CompositeStrategy<int, int>(list.AsReadOnly());
int final = composite.Execute(5); // (5 *2) +10 =20
```

Note: `CompositeStrategy` enforces that `TContext` and `TResult` are the same runtime type so that each strategy's result may be cast back to the next strategy's input.

## Design notes

- Both sync and async execution are first-class. `StrategyBase` provides a default async implementation that delegates to the sync core method; override the async core when performing true asynchronous work.
- `IStrategy` is intentionally minimal — it models an interchangeable algorithm or policy that operates on a context and returns a result.
- `CompositeStrategy` implements a simple pipeline: each strategy result becomes the next strategy's input. This makes it useful for transform pipelines where input and output types match.

## Benefits

- Clear separation of algorithms from the callers that use them
- Easy to swap behavior at runtime
- Supports both synchronous and asynchronous flows
- Composable using `CompositeStrategy` when appropriate

## Testing

Unit tests for the Strategy implementation are located in `tests/DesignPatterns/Strategy` and cover sync/async execution and composite chaining.

Run the strategy tests with:

```pwsh
dotnet test --filter "FullyQualifiedName~MBUtils.Tests.DesignPatterns.Strategy"
```

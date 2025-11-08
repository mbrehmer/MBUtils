# MBUtils

A small utility library for .NET with focused, reusable building blocks.

## Installation

MBUtils is published as a NuGet package.

- Package ID: `MBUtils`
- Current version: `0.1.0`
- Target frameworks: `net8.0`, `netstandard2.0`

Install using your preferred method, for example:

```pwsh
dotnet add package MBUtils --version 0.1.0
```

## Design Patterns

### Command Pattern

A Command pattern utility with a thread-safe command queue and optional undo/redo support, available for both synchronous and asynchronous commands.

**Key Features:**
- Thread-safe command queue
- Undo/Redo support
- Synchronous and asynchronous commands
- FIFO execution order
- Comprehensive unit tests

See [Command Pattern documentation](src/DesignPatterns/Command/README.md) for detailed information and examples.

### Builder Pattern

A Builder pattern implementation with fluent interface support, validation, state tracking, and async capabilities.

**Key Features:**
- Fluent interface with method chaining
- Built-in validation framework
- State tracking to prevent misuse
- Async/await support with cancellation tokens
- Director pattern for reusable construction algorithms
- Comprehensive unit tests

See [Builder Pattern documentation](src/DesignPatterns/Builder/README.md) for detailed information and examples.

### Singleton Pattern

An opinionated Singleton helper providing a thread-safe, lazy-initialized base class suitable for global, single-instance services.

**Key Features:**
- Thread-safe lazy initialization using `Lazy<T>`
- Optional `OnInstanceCreated()` hook for post-construction initialization
- `ResetInstance()` support for testing scenarios

See [Singleton Pattern documentation](src/DesignPatterns/Singleton/README.md) for full details and examples.

### Strategy Pattern

A Strategy pattern implementation that models interchangeable algorithms as objects and supports both synchronous and asynchronous execution.

**Key Features:**
- `IStrategy<TContext, TResult>` with sync and async execution methods
- `StrategyBase<TContext, TResult>` to simplify concrete implementations and provide a default async behavior
- `CompositeStrategy<TContext, TResult>` to compose/chain strategies when input and output types match
- Explicit interface implementations and clear error handling
- Comprehensive unit tests

See [Strategy Pattern documentation](src/DesignPatterns/Strategy/README.md) for usage examples and details.

### Observer Pattern

A thread-safe implementation of the Observer pattern supporting both synchronous and asynchronous observers.

**Key Features:**
- Thread-safe observer management with locking
- Support for both synchronous and asynchronous observers
- `SubjectBase<T>` abstract base class for easy subject implementation
- Snapshot notification to avoid issues during observer attachment/detachment
- Separate lists for sync and async observers
- Comprehensive unit tests

See [Observer Pattern documentation](src/DesignPatterns/Observer/README.md) for detailed information and examples.

### Factory Pattern

A Factory pattern implementation providing flexible, thread-safe object creation based on keys or configuration, with support for both synchronous and asynchronous initialization.

**Key Features:**
- Thread-safe keyed factory with registration and resolution
- `FactoryBase<TKey, T>` for synchronous object creation
- `AsyncFactoryBase<TKey, T>` for asynchronous initialization scenarios
- Custom `IEqualityComparer<TKey>` support for flexible key matching
- Dynamic registration and unregistration
- Comprehensive unit tests

See [Factory Pattern documentation](src/DesignPatterns/Factory/README.md) for detailed information and examples.

### Decorator Pattern

A Decorator pattern implementation providing dynamic addition of responsibilities to objects, supporting both synchronous and asynchronous decoration with composite chaining.

**Key Features:**
- `IDecorator<T>` and `IAsyncDecorator<T>` for sync and async decorators
- `DecoratorBase<T>` and `AsyncDecoratorBase<T>` base classes for easy implementation
- `CompositeDecorator<T>` for chaining multiple decorators
- Explicit interface implementations and null safety
- Support for cross-cutting concerns (logging, caching, validation, etc.)
- Comprehensive unit tests

See [Decorator Pattern documentation](src/DesignPatterns/Decorator/README.md) for detailed information and examples.

### Chain of Responsibility Pattern

A Chain of Responsibility pattern implementation that allows passing requests along a chain of handlers, where each handler decides either to process the request or to pass it to the next handler in the chain.

**Key Features:**
- `IHandler<TRequest>` and `IAsyncHandler<TRequest>` for sync and async handlers
- `HandlerBase<TRequest>` and `AsyncHandlerBase<TRequest>` with template methods
- `HandlerChain<TRequest>` and `AsyncHandlerChain<TRequest>` fluent builders
- Early termination when a handler processes the request
- Support for middleware pipelines, validation chains, and event routing
- Comprehensive unit tests

See [Chain of Responsibility Pattern documentation](src/DesignPatterns/Chain/README.md) for detailed information and examples.

### Adapter Pattern

An Adapter pattern implementation that allows objects with incompatible interfaces to work together, supporting both synchronous and asynchronous adaptation.

**Key Features:**
- `IAdapter<TSource, TTarget>` and `IAsyncAdapter<TSource, TTarget>` for sync and async adapters
- `AdapterBase<TSource, TTarget>` and `AsyncAdapterBase<TSource, TTarget>` base classes
- Type-safe with variance support (contravariant source, covariant target)
- Built-in null checking and template method pattern
- Cancellation support for async adapters
- Comprehensive unit tests

See [Adapter Pattern documentation](src/DesignPatterns/Adapter/README.md) for detailed information and examples.

### Facade Pattern

A Facade pattern implementation that provides a simplified interface to complex subsystems, supporting both synchronous and asynchronous operations.

**Key Features:**
- `IFacade` and `IFacade<TResult>` for sync facades with or without return values
- `IAsyncFacade` and `IAsyncFacade<TResult>` for async facades
- `FacadeBase` and `AsyncFacadeBase` base classes for easy implementation
- Simplifies complex subsystem interactions with a unified interface
- Reduces coupling between client code and subsystem components
- Comprehensive unit tests

See [Facade Pattern documentation](src/DesignPatterns/Facade/README.md) for detailed information and examples.

## Development

### Building the Project

```pwsh
dotnet build
```

### Running Tests

Run all tests:

```pwsh
dotnet test
```

Run tests for a specific pattern:

```pwsh
# Command pattern tests
dotnet test --filter "FullyQualifiedName~MBUtils.Tests.DesignPatterns.Command"

# Builder pattern tests
dotnet test --filter "FullyQualifiedName~MBUtils.Tests.DesignPatterns.Builder"

# Singleton pattern tests
dotnet test --filter "FullyQualifiedName~MBUtils.Tests.DesignPatterns.Singleton"

# Strategy pattern tests
dotnet test --filter "FullyQualifiedName~MBUtils.Tests.DesignPatterns.Strategy"

# Observer pattern tests
dotnet test --filter "FullyQualifiedName~MBUtils.Tests.DesignPatterns.Observer"

# Factory pattern tests
dotnet test --filter "FullyQualifiedName~MBUtils.Tests.DesignPatterns.Factory"

# Decorator pattern tests
dotnet test --filter "FullyQualifiedName~MBUtils.Tests.DesignPatterns.Decorator"

# Chain of Responsibility pattern tests
dotnet test --filter "FullyQualifiedName~MBUtils.Tests.DesignPatterns.Chain"

# Adapter pattern tests
dotnet test --filter "FullyQualifiedName~MBUtils.Tests.DesignPatterns.Adapter"

# Facade pattern tests
dotnet test --filter "FullyQualifiedName~MBUtils.Tests.DesignPatterns.Facade"
```

## License

See [`LICENSE`](LICENSE) for details.
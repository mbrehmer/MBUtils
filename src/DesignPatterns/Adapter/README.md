# Adapter Pattern

The Adapter pattern allows objects with incompatible interfaces to work together. It acts as a bridge between two incompatible interfaces by wrapping an object and providing a different interface to it.

## Overview

This implementation provides:

- `IAdapter<TSource, TTarget>` - Synchronous adapter interface
- `IAsyncAdapter<TSource, TTarget>` - Asynchronous adapter interface
- `AdapterBase<TSource, TTarget>` - Abstract base class for synchronous adapters
- `AsyncAdapterBase<TSource, TTarget>` - Abstract base class for asynchronous adapters

## Key Features

- **Synchronous and Asynchronous Support**: Both sync and async adaptation patterns
- **Type-Safe**: Strongly typed source and target types with variance support
- **Null Safety**: Built-in null checking with appropriate exceptions
- **Cancellation Support**: Async adapters support cancellation tokens
- **Template Method Pattern**: Base classes handle null checking and delegate to protected core methods
- **Explicit Interface Implementation**: Clean public API surface

## When to Use

Use the Adapter pattern when:

- You need to integrate with a third-party library with an incompatible interface
- You want to reuse existing classes that don't have compatible interfaces
- You need to create a compatibility layer between different versions of an API
- You want to convert data between different formats or representations
- You need to wrap legacy code with a modern interface

## Basic Usage

### Synchronous Adapter

```csharp
using MBUtils.DesignPatterns.Adapter;

// Define your source and target types
public class LegacyUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
}

public class ModernUser
{
    public string FullName { get; set; }
    public string EmailAddress { get; set; }
}

// Implement the adapter
public class LegacyUserAdapter : AdapterBase<LegacyUser, ModernUser>
{
    protected override ModernUser AdaptCore(LegacyUser source)
    {
        return new ModernUser
        {
            FullName = $"{source.FirstName} {source.LastName}",
            EmailAddress = source.Email
        };
    }
}

// Use the adapter
IAdapter<LegacyUser, ModernUser> adapter = new LegacyUserAdapter();
LegacyUser legacyUser = new LegacyUser 
{ 
    FirstName = "John", 
    LastName = "Doe", 
    Email = "john@example.com" 
};

ModernUser modernUser = adapter.Adapt(legacyUser);
Console.WriteLine(modernUser.FullName); // "John Doe"
```

### Asynchronous Adapter

```csharp
using MBUtils.DesignPatterns.Adapter;
using System.Threading;
using System.Threading.Tasks;

// Adapter that performs async operations (e.g., enriching data from external source)
public class UserEnrichmentAdapter : AsyncAdapterBase<ModernUser, EnrichedUser>
{
    private readonly IExternalDataService _dataService;

    public UserEnrichmentAdapter(IExternalDataService dataService)
    {
        _dataService = dataService;
    }

    protected override async Task<EnrichedUser> AdaptAsyncCore(
        ModernUser source, 
        CancellationToken cancellationToken)
    {
        // Perform async operations
        string profilePicture = await _dataService.GetProfilePictureAsync(
            source.EmailAddress, 
            cancellationToken);
        
        string location = await _dataService.GetLocationAsync(
            source.EmailAddress, 
            cancellationToken);

        return new EnrichedUser
        {
            FullName = source.FullName,
            EmailAddress = source.EmailAddress,
            ProfilePictureUrl = profilePicture,
            Location = location
        };
    }
}

// Use the async adapter
IAsyncAdapter<ModernUser, EnrichedUser> adapter = new UserEnrichmentAdapter(dataService);
EnrichedUser enrichedUser = await adapter.AdaptAsync(modernUser, cancellationToken);
```

## Advanced Scenarios

### Chaining Adapters

You can chain multiple adapters to create a pipeline:

```csharp
public class AdapterChain<TSource, TIntermediate, TTarget>
{
    private readonly IAdapter<TSource, TIntermediate> _firstAdapter;
    private readonly IAdapter<TIntermediate, TTarget> _secondAdapter;

    public AdapterChain(
        IAdapter<TSource, TIntermediate> firstAdapter,
        IAdapter<TIntermediate, TTarget> secondAdapter)
    {
        _firstAdapter = firstAdapter;
        _secondAdapter = secondAdapter;
    }

    public TTarget Adapt(TSource source)
    {
        TIntermediate intermediate = _firstAdapter.Adapt(source);
        return _secondAdapter.Adapt(intermediate);
    }
}

// Usage
IAdapter<LegacyUser, ModernUser> legacyAdapter = new LegacyUserAdapter();
IAdapter<ModernUser, EnrichedUser> enrichmentAdapter = new SyncUserEnrichmentAdapter();

AdapterChain<LegacyUser, ModernUser, EnrichedUser> chain = 
    new AdapterChain<LegacyUser, ModernUser, EnrichedUser>(legacyAdapter, enrichmentAdapter);

EnrichedUser result = chain.Adapt(legacyUser);
```

### Bidirectional Adapter

Create an adapter that works in both directions:

```csharp
public class BidirectionalUserAdapter : 
    AdapterBase<LegacyUser, ModernUser>,
    IAdapter<ModernUser, LegacyUser>
{
    protected override ModernUser AdaptCore(LegacyUser source)
    {
        return new ModernUser
        {
            FullName = $"{source.FirstName} {source.LastName}",
            EmailAddress = source.Email
        };
    }

    LegacyUser IAdapter<ModernUser, LegacyUser>.Adapt(ModernUser source)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        string[] nameParts = source.FullName.Split(' ', 2);
        return new LegacyUser
        {
            FirstName = nameParts.Length > 0 ? nameParts[0] : "",
            LastName = nameParts.Length > 1 ? nameParts[1] : "",
            Email = source.EmailAddress
        };
    }
}

// Use in both directions
BidirectionalUserAdapter adapter = new BidirectionalUserAdapter();
ModernUser modern = ((IAdapter<LegacyUser, ModernUser>)adapter).Adapt(legacyUser);
LegacyUser legacy = ((IAdapter<ModernUser, LegacyUser>)adapter).Adapt(modern);
```

### Collection Adapter

Adapt collections of objects:

```csharp
public class CollectionAdapter<TSource, TTarget> : AdapterBase<IEnumerable<TSource>, IReadOnlyList<TTarget>>
{
    private readonly IAdapter<TSource, TTarget> _itemAdapter;

    public CollectionAdapter(IAdapter<TSource, TTarget> itemAdapter)
    {
        _itemAdapter = itemAdapter ?? throw new ArgumentNullException(nameof(itemAdapter));
    }

    protected override IReadOnlyList<TTarget> AdaptCore(IEnumerable<TSource> source)
    {
        List<TTarget> result = new List<TTarget>();
        foreach (TSource item in source)
        {
            result.Add(_itemAdapter.Adapt(item));
        }
        return result.AsReadOnly();
    }
}

// Usage
IAdapter<LegacyUser, ModernUser> itemAdapter = new LegacyUserAdapter();
IAdapter<IEnumerable<LegacyUser>, IReadOnlyList<ModernUser>> collectionAdapter = 
    new CollectionAdapter<LegacyUser, ModernUser>(itemAdapter);

List<LegacyUser> legacyUsers = GetLegacyUsers();
IReadOnlyList<ModernUser> modernUsers = collectionAdapter.Adapt(legacyUsers);
```

### Adapter with Validation

Add validation logic to your adapter:

```csharp
public class ValidatingUserAdapter : AdapterBase<LegacyUser, ModernUser>
{
    protected override ModernUser AdaptCore(LegacyUser source)
    {
        // Validate before adapting
        if (string.IsNullOrWhiteSpace(source.Email))
            throw new InvalidOperationException("Email is required");

        if (string.IsNullOrWhiteSpace(source.FirstName) && 
            string.IsNullOrWhiteSpace(source.LastName))
            throw new InvalidOperationException("At least first name or last name is required");

        return new ModernUser
        {
            FullName = $"{source.FirstName} {source.LastName}".Trim(),
            EmailAddress = source.Email
        };
    }
}
```

## Design Decisions

### Explicit Interface Implementation

Both `AdapterBase<TSource, TTarget>` and `AsyncAdapterBase<TSource, TTarget>` use explicit interface implementation. This provides:

- A clean separation between the public contract and implementation details
- Protection of the template method (`AdaptCore`/`AdaptAsyncCore`)
- Flexibility for derived classes to implement additional interfaces

### Template Method Pattern

The base classes use the Template Method pattern:

1. Public interface method (`Adapt`/`AdaptAsync`) handles cross-cutting concerns (null checking)
2. Protected abstract method (`AdaptCore`/`AdaptAsyncCore`) contains the actual adaptation logic
3. Derived classes only need to implement the core logic

### Null Safety

All adapters enforce null checking at the base class level:

- Null sources throw `ArgumentNullException`
- Derived classes can assume non-null sources in their core methods

### Variance Support

The `IAdapter<in TSource, out TTarget>` interface uses:

- **Contravariance** (`in`) for `TSource` - allows using a more derived source type
- **Covariance** (`out`) for `TTarget` - allows using a more general target type

This enables flexible adapter composition and usage patterns.

## Testing

The implementation includes comprehensive unit tests covering:

- Basic adaptation scenarios
- Null argument handling
- Exception propagation
- Multiple invocations
- Async operation with cancellation tokens
- Parallel async execution
- Edge cases (empty strings, primitive types)

Run adapter tests with:

```pwsh
dotnet test --filter "FullyQualifiedName~MBUtils.Tests.DesignPatterns.Adapter"
```

## Thread Safety

- **Synchronous adapters**: Thread safety depends on the implementation. If your `AdaptCore` method is stateless or uses only local variables, it's thread-safe.
- **Asynchronous adapters**: Each async operation gets its own execution context, making them safe for concurrent use as long as shared state is properly synchronized.

## Best Practices

1. **Keep adapters focused**: Each adapter should have a single, clear purpose
2. **Prefer composition**: Chain adapters rather than creating complex single adapters
3. **Use async for I/O**: Use `AsyncAdapterBase` when adaptation involves I/O operations
4. **Validate early**: Perform validation in `AdaptCore` if business rules need checking
5. **Consider caching**: If adaptation is expensive and results are reusable, add caching
6. **Handle nullability**: Be explicit about whether your target type can be null
7. **Document assumptions**: Clearly document any assumptions about source data format

## Related Patterns

- **Facade**: Adapters convert interfaces; Facades simplify interfaces
- **Decorator**: Both wrap objects, but Decorator adds behavior while Adapter changes interface
- **Proxy**: Similar to Adapter but maintains the same interface
- **Strategy**: Can be used together - adapt strategies to work with different contexts
- **Factory**: Factories can create appropriate adapters based on input types

## See Also

- [Builder Pattern](../Builder/README.md)
- [Strategy Pattern](../Strategy/README.md)
- [Decorator Pattern](../Decorator/README.md)

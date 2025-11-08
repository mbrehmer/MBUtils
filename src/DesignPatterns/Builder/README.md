# Builder Design Pattern

This directory contains a comprehensive implementation of the Builder design pattern with advanced features.

## Components

### Core Interfaces

- **`IBuilder<T>`** - Basic builder interface with `Reset()` and `Build()` methods
- **`IFluentBuilder<TProduct>`** - Extended interface adding `IsValid` property for validation checking
- **`IAsyncBuilder<T>`** - Asynchronous builder interface with `BuildAsync()` method

### Base Classes

- **`BuilderBase<TBuilder, TProduct>`** - Abstract base class with:
  - Self-referencing generic pattern for fluent interface support
  - Built-in validation with `Validate()` method
  - State tracking to prevent builder misuse
  - `IsValid` property for pre-build validation
  - Protected `This()` method for method chaining

- **`AsyncBuilderBase<TBuilder, TProduct>`** - Async version with:
  - Asynchronous build and validation operations
  - Cancellation token support
  - Same state tracking and fluent interface features

### Director Pattern

- **`Director<TBuilder, TProduct>`** - Encapsulates construction algorithms
- **`IConstructionStrategy<TBuilder, TProduct>`** - Strategy interface for reusable construction patterns

## Usage Examples

### Basic Builder Example

```csharp
using MBUtils.DesignPatterns.Builder;
using System.Collections.Generic;

// Product class
public class User
{
    public string Username { get; set; }
    public string Email { get; set; }
    public int Age { get; set; }
    public List<string> Roles { get; set; }
}

// Concrete builder with fluent interface
public class UserBuilder : BuilderBase<UserBuilder, User>
{
    private string _username;
    private string _email;
    private int _age;
    private List<string> _roles = new List<string>();

    public UserBuilder WithUsername(string username)
    {
        _username = username;
        return This(); // Enable method chaining
    }

    public UserBuilder WithEmail(string email)
    {
        _email = email;
        return This();
    }

    public UserBuilder WithAge(int age)
    {
        _age = age;
        return This();
    }

    public UserBuilder WithRole(string role)
    {
        _roles.Add(role);
        return This();
    }

    protected override void ResetCore()
    {
        _username = null;
        _email = null;
        _age = 0;
        _roles.Clear();
    }

    protected override User BuildCore()
    {
        return new User
        {
            Username = _username,
            Email = _email,
            Age = _age,
            Roles = new List<string>(_roles)
        };
    }

    protected override IReadOnlyCollection<string> Validate()
    {
        List<string> errors = new List<string>();

        if (string.IsNullOrWhiteSpace(_username))
            errors.Add("Username is required");

        if (string.IsNullOrWhiteSpace(_email))
            errors.Add("Email is required");

        if (_age < 18)
            errors.Add("User must be at least 18 years old");

        return errors;
    }
}

// Usage with fluent interface
UserBuilder builder = new UserBuilder();

User user = builder
    .WithUsername("john_doe")
    .WithEmail("john@example.com")
    .WithAge(25)
    .WithRole("Admin")
    .WithRole("User")
    .Build();

// Check validity before building
if (builder.IsValid)
{
    User anotherUser = builder.Build();
}

// Reuse builder
builder.Reset();
User newUser = builder
    .WithUsername("jane_smith")
    .WithEmail("jane@example.com")
    .WithAge(30)
    .Build();
```

### Using Director Pattern

```csharp
// Define construction strategies
public class AdminUserStrategy : IConstructionStrategy<UserBuilder, User>
{
    public User Construct(UserBuilder builder)
    {
        return builder
            .WithRole("Admin")
            .WithRole("User")
            .Build();
    }
}

public class GuestUserStrategy : IConstructionStrategy<UserBuilder, User>
{
    public User Construct(UserBuilder builder)
    {
        return builder
            .WithRole("Guest")
            .Build();
    }
}

// Use director
Director<UserBuilder, User> director = new Director<UserBuilder, User>(new UserBuilder());

User adminUser = director.Construct(builder =>
{
    builder
        .WithUsername("admin")
        .WithEmail("admin@example.com")
        .WithAge(35)
        .WithRole("Admin");
});

// Or use a strategy
User guestUser = director.Construct(new GuestUserStrategy());
```

### Async Builder Example

```csharp
using System.Threading;
using System.Threading.Tasks;

public class AsyncUserBuilder : AsyncBuilderBase<AsyncUserBuilder, User>
{
    private string _username;
    private string _email;
    
    public AsyncUserBuilder WithUsername(string username)
    {
        _username = username;
        return This();
    }

    public AsyncUserBuilder WithEmail(string email)
    {
        _email = email;
        return This();
    }

    protected override void ResetCore()
    {
        _username = null;
        _email = null;
    }

    protected override async Task<User> BuildAsyncCore(CancellationToken cancellationToken)
    {
        // Simulate async operation (e.g., database lookup, API call)
        await Task.Delay(100, cancellationToken);
        
        return new User
        {
            Username = _username,
            Email = _email
        };
    }

    protected override async Task<IReadOnlyCollection<string>> ValidateAsync(CancellationToken cancellationToken)
    {
        List<string> errors = new List<string>();

        // Simulate async validation (e.g., check if username exists)
        await Task.Delay(50, cancellationToken);

        if (string.IsNullOrWhiteSpace(_username))
            errors.Add("Username is required");

        return errors;
    }
}

// Usage
AsyncUserBuilder asyncBuilder = new AsyncUserBuilder();
User user = await asyncBuilder
    .WithUsername("async_user")
    .WithEmail("async@example.com")
    .BuildAsync();
```

## Features

### 1. Fluent Interface
- Method chaining support via self-referencing generic pattern
- Clean, readable API
- Type-safe builder methods

### 2. Validation
- Built-in validation framework
- Pre-build validation with `IsValid` property
- Detailed error messages
- Prevents building invalid objects

### 3. State Tracking
- Prevents accidental builder reuse
- `IsBuilt` property to track builder state
- Enforces `Reset()` call between builds
- Clear error messages for misuse

### 4. Director Pattern
- Encapsulate construction algorithms
- Reusable construction strategies
- Separation of concerns

### 5. Async Support
- Full async/await pattern support
- Cancellation token support
- Async validation

## Benefits

- **Type Safety**: Compile-time type checking
- **Immutability**: Products can be immutable (builder handles construction)
- **Reusability**: Builders can be reused after reset
- **Testability**: Easy to test with mock builders
- **Maintainability**: Clear separation of concerns
- **Flexibility**: Easy to add new builders or construction strategies

## Testing

The Builder pattern implementation includes comprehensive unit tests covering:

- **BuilderBase**: Validation, state tracking, fluent interface, error handling
- **AsyncBuilderBase**: Async operations, cancellation token support, async validation
- **Director**: Construction algorithms, strategy pattern, builder reuse

All tests follow xUnit conventions and can be run with:

```pwsh
dotnet test --filter "FullyQualifiedName~MBUtils.Tests.DesignPatterns.Builder"
```

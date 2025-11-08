# Chain of Responsibility Design Pattern

A Chain of Responsibility pattern implementation that allows passing requests along a chain of handlers, where each handler decides either to process the request or to pass it to the next handler in the chain.

Namespace: `MBUtils.DesignPatterns.Chain`

## Interfaces

- `IHandler<TRequest>`
	- Synchronous handler contract with `bool Handle(TRequest request)` and `IHandler<TRequest> SetNext(IHandler<TRequest> next)`.
- `IAsyncHandler<TRequest>`
	- Asynchronous handler contract with `Task<bool> HandleAsync(TRequest request, CancellationToken cancellationToken)` and `IAsyncHandler<TRequest> SetNext(IAsyncHandler<TRequest> next)`.

Both interfaces support fluent chaining via `SetNext()`, which returns the next handler to allow chaining multiple handlers in sequence.

## Base Classes

### HandlerBase<TRequest>

Abstract base class for synchronous handlers that implements `IHandler<TRequest>`.

- Template methods for derived classes:
	- `protected abstract bool CanHandle(TRequest request)` – determine if this handler can process the request.
	- `protected abstract void HandleCore(TRequest request)` – process the request (only called when `CanHandle` returns `true`).

The `Handle()` method orchestrates the logic: if `CanHandle()` returns `true`, it calls `HandleCore()` and returns `true`. Otherwise, it delegates to the next handler in the chain (if any).

### AsyncHandlerBase<TRequest>

Abstract base class for asynchronous handlers that implements `IAsyncHandler<TRequest>`.

- Template methods for derived classes:
	- `protected abstract Task<bool> CanHandleAsync(TRequest request, CancellationToken cancellationToken)` – determine if this handler can process the request.
	- `protected abstract Task HandleCoreAsync(TRequest request, CancellationToken cancellationToken)` – process the request asynchronously (only called when `CanHandleAsync` returns `true`).

The `HandleAsync()` method orchestrates the logic: if `CanHandleAsync()` returns `true`, it calls `HandleCoreAsync()` and returns `true`. Otherwise, it delegates to the next handler in the chain (if any).

## Helper Classes

### HandlerChain<TRequest>

Fluent builder for constructing synchronous handler chains.

- `HandlerChain<TRequest> Add(IHandler<TRequest> handler)` – adds a handler to the end of the chain.
- `IHandler<TRequest>? Build()` – links all handlers together and returns the first handler in the chain.

### AsyncHandlerChain<TRequest>

Fluent builder for constructing asynchronous handler chains.

- `AsyncHandlerChain<TRequest> Add(IAsyncHandler<TRequest> handler)` – adds a handler to the end of the chain.
- `IAsyncHandler<TRequest>? Build()` – links all handlers together and returns the first handler in the chain.

## Quick start

### Synchronous handler chain

```csharp
using MBUtils.DesignPatterns.Chain;

// Define a simple request type
public sealed class SupportRequest
{
		public string Type { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public bool Handled { get; set; }
}

// Handler for basic requests
public sealed class BasicSupportHandler : HandlerBase<SupportRequest>
{
		protected override bool CanHandle(SupportRequest request)
		{
				return request.Type == "Basic";
		}

		protected override void HandleCore(SupportRequest request)
		{
				Console.WriteLine($"BasicSupport: Handling {request.Description}");
				request.Handled = true;
		}
}

// Handler for advanced requests
public sealed class AdvancedSupportHandler : HandlerBase<SupportRequest>
{
		protected override bool CanHandle(SupportRequest request)
		{
				return request.Type == "Advanced";
		}

		protected override void HandleCore(SupportRequest request)
		{
				Console.WriteLine($"AdvancedSupport: Handling {request.Description}");
				request.Handled = true;
		}
}

// Build and use the chain
IHandler<SupportRequest>? chain = new HandlerChain<SupportRequest>()
		.Add(new BasicSupportHandler())
		.Add(new AdvancedSupportHandler())
		.Build();

SupportRequest request = new SupportRequest
{
		Type = "Advanced",
		Description = "Need help with complex issue"
};

bool handled = chain?.Handle(request) ?? false;
// Output: AdvancedSupport: Handling Need help with complex issue
// handled == true
```

### Asynchronous handler chain

```csharp
using MBUtils.DesignPatterns.Chain;

// Async handler for authentication
public sealed class AuthenticationHandler : AsyncHandlerBase<HttpRequest>
{
		private readonly IAuthService _authService;

		public AuthenticationHandler(IAuthService authService)
		{
				_authService = authService;
		}

		protected override async Task<bool> CanHandleAsync(
				HttpRequest request,
				CancellationToken cancellationToken)
		{
				return request.Headers.ContainsKey("Authorization");
		}

		protected override async Task HandleCoreAsync(
				HttpRequest request,
				CancellationToken cancellationToken)
		{
				string token = request.Headers["Authorization"];
				request.User = await _authService.ValidateTokenAsync(token, cancellationToken);
		}
}

// Async handler for logging
public sealed class LoggingHandler : AsyncHandlerBase<HttpRequest>
{
		private readonly ILogger _logger;

		public LoggingHandler(ILogger logger)
		{
				_logger = logger;
		}

		protected override Task<bool> CanHandleAsync(
				HttpRequest request,
				CancellationToken cancellationToken)
		{
				return Task.FromResult(true); // Always log
		}

		protected override async Task HandleCoreAsync(
				HttpRequest request,
				CancellationToken cancellationToken)
		{
				await _logger.LogAsync($"Request: {request.Method} {request.Path}", cancellationToken);
		}
}

// Build and use the async chain
IAsyncHandler<HttpRequest>? chain = new AsyncHandlerChain<HttpRequest>()
		.Add(new AuthenticationHandler(authService))
		.Add(new LoggingHandler(logger))
		.Build();

HttpRequest request = new HttpRequest { Method = "GET", Path = "/api/data" };
bool handled = await (chain?.HandleAsync(request) ?? Task.FromResult(false));
```

### Manual chaining without the builder

You can also manually chain handlers using `SetNext()`:

```csharp
IHandler<SupportRequest> handler1 = new BasicSupportHandler();
IHandler<SupportRequest> handler2 = new AdvancedSupportHandler();

handler1.SetNext(handler2);

SupportRequest request = new SupportRequest { Type = "Advanced" };
bool handled = handler1.Handle(request);
// Request passes through handler1 to handler2 where it's handled
```

## Design notes and guarantees

- **Early termination**: the chain stops processing as soon as a handler returns `true` (i.e., when `CanHandle` returns `true` and the handler processes the request).
- **Fallback behavior**: if no handler in the chain can process the request, the final `Handle()`/`HandleAsync()` call returns `false`.
- **Null safety**: `SetNext()` throws `ArgumentNullException` if passed a null handler. The builder's `Add()` method also validates for null.
- **Async support**: async handlers properly support cancellation tokens and use `ConfigureAwait(false)` to avoid capturing synchronization contexts.
- **Immutable chains**: once built via `HandlerChain`, the links are established. To modify the chain, build a new one.

## Common use cases

- **Middleware pipelines**: ASP.NET-style request processing (authentication, logging, validation, etc.)
- **Validation chains**: multiple validators that can reject or pass the request along
- **Event handling**: route events to appropriate handlers based on event type
- **Authorization**: check permissions at multiple levels (user, role, resource, etc.)
- **Error handling**: try multiple error recovery strategies in sequence
- **Logging and auditing**: cross-cutting concerns that always process but don't block the chain

## Testing

The Chain of Responsibility pattern implementation includes comprehensive unit tests covering:

- **HandlerBase/AsyncHandlerBase**: Handler logic, chaining behavior, early termination
- **HandlerChain/AsyncHandlerChain**: Fluent building, multiple handlers, edge cases
- Error handling, null safety, and async cancellation support

All tests follow xUnit conventions and can be run with:

```pwsh
dotnet test --filter "FullyQualifiedName~MBUtils.Tests.DesignPatterns.Chain"
```

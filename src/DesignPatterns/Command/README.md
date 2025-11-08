# Command Design Pattern

A Command pattern utility with a thread-safe command queue and optional undo/redo support, available for both synchronous and asynchronous commands.

Namespace: `MBUtils.DesignPatterns.Command`

## Interfaces

- `ICommand`
	- Synchronous command contract with `void Execute()`.
- `IUndoableCommand : ICommand`
	- Adds `void Undo()`; implementations should restore state to before the last `Execute()`.
- `IAsyncCommand : ICommand`
	- Adds `Task ExecuteAsync()` for asynchronous execution.
- `IAsyncUndoableCommand : IAsyncCommand`
	- Adds `Task UndoAsync()` for asynchronous undo.

All async interfaces still inherit from `ICommand` so you can mix sync and async commands in one queue.

## CommandQueue<T>

`CommandQueue<T>` executes commands in FIFO order and tracks undo/redo for undoable commands.

- Generic constraint: `where T : ICommand`
- Thread-safety: public members are safe for concurrent access.
- Exception behavior: if a command throws during Execute/Undo/Redo, internal stacks are not mutated and the exception is propagated.

Key members:

- Enqueue and execute
	- `void Enqueue(T command)`
	- `void ExecuteNext()` / `Task ExecuteNextAsync()`
	- `bool TryExecuteNext(out T executedCommand)`
	- `Task<(bool success, T executedCommand)> TryExecuteNextAsync()`
	- `void ExecuteAll()` / `Task ExecuteAllAsync()`
- Undo/Redo
	- `void UndoLast()` / `Task<(bool success, IUndoableCommand? undoneCommand)> TryUndoLastAsync()`
	- `bool TryUndoLast(out IUndoableCommand? undoneCommand)`
	- `void RedoLast()` / `Task<(bool success, IUndoableCommand? redoneCommand)> TryRedoLastAsync()`
	- `bool TryRedoLast(out IUndoableCommand? redoneCommand)`
	- `void UndoAll()` / `Task UndoAllAsync()`
	- `void RedoAll()` / `Task RedoAllAsync()`
- Maintenance
	- `void Clear()` – clears the queue and both undo/redo stacks.

Notes:

- Async commands are supported transparently. When you call a synchronous API on an async command, the queue will block on the async work. Prefer the `Async` variants to avoid blocking.
- Redo stack is cleared when a new command executes successfully (standard undo/redo semantics).

## Quick start

### Synchronous, undoable command

```csharp
using MBUtils.DesignPatterns.Command;

// A simple counter increment with undo
public sealed class IncrementCounter : IUndoableCommand
{
		private readonly Counter _counter;
		public IncrementCounter(Counter counter) => _counter = counter;
		public void Execute() => _counter.Value++;
		public void Undo() => _counter.Value--;
}

public sealed class Counter { public int Value { get; set; } }

Counter counter = new Counter();
CommandQueue<ICommand> queue = new CommandQueue<ICommand>();

queue.Enqueue(new IncrementCounter(counter));
queue.ExecuteAll();
// counter.Value == 1

queue.UndoLast();
// counter.Value == 0

queue.RedoLast();
// counter.Value == 1
```

### Asynchronous, undoable command

```csharp
using MBUtils.DesignPatterns.Command;

public sealed class SaveDocumentAsync : IAsyncUndoableCommand
{
		private readonly IDocumentStore _store;
		private readonly Document _doc;
		private string? _lastVersionId;

		public SaveDocumentAsync(IDocumentStore store, Document doc)
		{
				_store = store; _doc = doc;
		}

		public void Execute() => throw new NotSupportedException("Use ExecuteAsync");
		public void Undo() => throw new NotSupportedException("Use UndoAsync");

		public async Task ExecuteAsync()
		{
				_lastVersionId = await _store.SaveAsync(_doc);
		}

		public async Task UndoAsync()
		{
				if (_lastVersionId != null)
						await _store.DeleteVersionAsync(_lastVersionId);
		}
}

CommandQueue<ICommand> queue = new CommandQueue<ICommand>();
queue.Enqueue(new SaveDocumentAsync(store, doc));
await queue.ExecuteAllAsync();
await queue.TryUndoLastAsync();
```

## Design notes and guarantees

- Consistent stacks: undo/redo stacks are updated only after successful operations.
- Mixed sync/async: you can enqueue any mix of `ICommand`, `IUndoableCommand`, `IAsyncCommand`, `IAsyncUndoableCommand` as `T : ICommand`.
- Concurrency: methods use internal locking to protect queue and stacks; external command implementations must ensure their own thread-safety.

## Testing

The Command pattern implementation includes comprehensive unit tests covering:

- **CommandQueue**: Enqueue/execute operations, undo/redo functionality, async commands, error handling, thread-safety

All tests follow xUnit conventions and can be run with:

```pwsh
dotnet test --filter "FullyQualifiedName~MBUtils.Tests.DesignPatterns.Command"
```

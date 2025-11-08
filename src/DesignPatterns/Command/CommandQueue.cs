using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MBUtils.DesignPatterns.Command
{
    /// <summary>
    /// Represents a queue of commands that can be executed in FIFO order and
    /// provides undo/redo support for commands that implement <see cref="IUndoableCommand"/>.
    /// </summary>
    /// <typeparam name="T">The command type. Must implement <see cref="ICommand"/>.</typeparam>
    /// <remarks>
    /// Enqueued commands are executed one-by-one when <see cref="ExecuteNext"/> or
    /// <see cref="ExecuteAll"/> is called. Commands that implement <see cref="IUndoableCommand"/>
    /// are tracked on internal undo and redo stacks so their effects may be reverted
    /// with <see cref="UndoLast"/>, reapplied with <see cref="RedoLast"/>, or processed
    /// in bulk with <see cref="UndoAll"/> and <see cref="RedoAll"/>.
    ///
    /// Thread-safety: public members are safe for concurrent access.
    ///
    /// Exception-safety: if a command's Execute/Undo/Redo throws, the internal stacks
    /// will not be modified (so undo/redo state remains consistent). The exception
    /// will propagate to the caller.
    /// </remarks>
    public class CommandQueue<T> where T : ICommand
    {
        private readonly Queue<T> _queue = new Queue<T>();
        private readonly Stack<IUndoableCommand> _undoStack = new Stack<IUndoableCommand>();
        private readonly Stack<IUndoableCommand> _redoStack = new Stack<IUndoableCommand>();
        private readonly object _sync = new object();

        /// <summary>
        /// Adapter that wraps sync and/or async undoable commands so they can be
        /// stored in a single stack while exposing both sync and async APIs.
        /// </summary>
        private sealed class UndoableAdapter : IUndoableCommand, IAsyncUndoableCommand
        {
            private readonly IUndoableCommand? _innerSync;
            private readonly IAsyncUndoableCommand? _innerAsync;

            public UndoableAdapter(IUndoableCommand sync)
            {
                _innerSync = sync ?? throw new ArgumentNullException(nameof(sync));
            }

            public UndoableAdapter(IAsyncUndoableCommand async)
            {
                _innerAsync = async ?? throw new ArgumentNullException(nameof(async));
            }

            // Synchronous Execute - blocks for async commands
            public void Execute()
            {
                if (_innerSync != null)
                {
                    _innerSync.Execute();
                }
                else
                {
                    // Block on the async execute for callers using sync API
                    _innerAsync!.ExecuteAsync().GetAwaiter().GetResult();
                }
            }

            // Synchronous Undo - blocks for async commands
            public void Undo()
            {
                if (_innerSync != null)
                {
                    _innerSync.Undo();
                }
                else
                {
                    _innerAsync!.UndoAsync().GetAwaiter().GetResult();
                }
            }

            // Asynchronous Execute
            public Task ExecuteAsync()
            {
                if (_innerAsync != null)
                {
                    return _innerAsync.ExecuteAsync();
                }
                else
                {
                    return Task.Run(() => _innerSync!.Execute());
                }
            }

            // Asynchronous Undo
            public Task UndoAsync()
            {
                if (_innerAsync != null)
                {
                    return _innerAsync.UndoAsync();
                }
                else
                {
                    return Task.Run(() => _innerSync!.Undo());
                }
            }
        }

        /// <summary>
        /// Enqueues a command to be executed later.
        /// </summary>
        /// <param name="command">The command to enqueue.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is <c>null</c>.</exception>
        public void Enqueue(T command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            lock (_sync)
            {
                _queue.Enqueue(command);
            }
        }

        /// <summary>
        /// Dequeues and executes the next command in the queue, if any.
        /// Existing behavior retained: void signature that ignores result.
        /// </summary>
        public void ExecuteNext()
        {
            TryExecuteNext(out _);
        }

        /// <summary>
        /// Executes the next command asynchronously, if any.
        /// </summary>
        public async Task ExecuteNextAsync()
        {
            await TryExecuteNextAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Attempts to dequeue and execute the next command (sync).
        /// Returns true if a command was executed; false when the queue was empty.
        /// </summary>
        /// <param name="executedCommand">The command that was executed, or default if none.</param>
        /// <returns>True if a command was executed; otherwise false.</returns>
        public bool TryExecuteNext(out T executedCommand)
        {
            executedCommand = default!;
            T command;
            lock (_sync)
            {
                if (_queue.Count == 0) return false;
                command = _queue.Dequeue();
            }

            // Execute synchronously. If this throws, do not modify stacks.
            if (command is IAsyncCommand asyncCmd)
            {
                // Block for callers using the sync API
                asyncCmd.ExecuteAsync().GetAwaiter().GetResult();
            }
            else
            {
                command.Execute();
            }

            // Only mutate stacks after successful execution.
            lock (_sync)
            {
                _redoStack.Clear(); // clear redo for any new action
                if (command is IAsyncUndoableCommand asyncUndoable)
                {
                    _undoStack.Push(new UndoableAdapter(asyncUndoable));
                }
                else if (command is IUndoableCommand undoableCommand)
                {
                    _undoStack.Push(new UndoableAdapter(undoableCommand));
                }
            }

            executedCommand = command;
            return true;
        }

        /// <summary>
        /// Attempts to dequeue and execute the next command asynchronously.
        /// Returns true if a command was executed; false when the queue was empty.
        /// </summary>
        /// <returns>A task that results in a tuple of (success, executedCommand).</returns>
        public async Task<(bool success, T executedCommand)> TryExecuteNextAsync()
        {
            T command;
            lock (_sync)
            {
                if (_queue.Count == 0) return (false, default!);
                command = _queue.Dequeue();
            }

            // Execute asynchronously without blocking the calling thread.
            if (command is IAsyncCommand asyncCmd)
            {
                await asyncCmd.ExecuteAsync().ConfigureAwait(false);
            }
            else
            {
                await Task.Run(() => command.Execute()).ConfigureAwait(false);
            }

            // Only mutate stacks after successful execution.
            lock (_sync)
            {
                _redoStack.Clear();
                if (command is IAsyncUndoableCommand asyncUndoable)
                {
                    _undoStack.Push(new UndoableAdapter(asyncUndoable));
                }
                else if (command is IUndoableCommand undoableCommand)
                {
                    _undoStack.Push(new UndoableAdapter(undoableCommand));
                }
            }

            return (true, command);
        }

        /// <summary>
        /// Undoes the last executed undoable command, if any.
        /// Existing behavior retained: void signature that ignores result.
        /// </summary>
        public void UndoLast()
        {
            TryUndoLast(out _);
        }

        /// <summary>
        /// Attempts to undo the last executed undoable command (sync).
        /// Returns true if a command was undone; false when none available.
        /// </summary>
        /// <param name="undoneCommand">The command that was undone, or null if none.</param>
        /// <returns>True if undone; otherwise false.</returns>
        public bool TryUndoLast(out IUndoableCommand? undoneCommand)
        {
            undoneCommand = null;
            IUndoableCommand command;
            lock (_sync)
            {
                if (_undoStack.Count == 0) return false;
                command = _undoStack.Pop();
            }

            // Call Undo (sync) outside lock. For async commands the adapter will block.
            command.Undo();

            lock (_sync)
            {
                _redoStack.Push(command);
            }

            undoneCommand = command;
            return true;
        }

        /// <summary>
        /// Attempts to undo the last executed undoable command asynchronously.
        /// Returns true if a command was undone; false when none available.
        /// </summary>
        public async Task<(bool success, IUndoableCommand? undoneCommand)> TryUndoLastAsync()
        {
            IUndoableCommand command;
            lock (_sync)
            {
                if (_undoStack.Count == 0) return (false, null);
                command = _undoStack.Pop();
            }

            // Call Undo asynchronously. The adapter exposes UndoAsync for both sync and async commands.
            if (command is IAsyncUndoableCommand asyncUndo)
            {
                await asyncUndo.UndoAsync().ConfigureAwait(false);
            }
            else
            {
                // Fallback: run sync Undo on threadpool
                await Task.Run(() => command.Undo()).ConfigureAwait(false);
            }

            lock (_sync)
            {
                _redoStack.Push(command);
            }

            return (true, command);
        }

        /// <summary>
        /// Re-executes the last undone command, if any.
        /// Existing behavior retained: void signature that ignores result.
        /// </summary>
        public void RedoLast()
        {
            TryRedoLast(out _);
        }

        /// <summary>
        /// Attempts to redo the last undone command (sync).
        /// Returns true if a command was redone; false when none available.
        /// </summary>
        /// <param name="redoneCommand">The command that was redone, or null if none.</param>
        /// <returns>True if redone; otherwise false.</returns>
        public bool TryRedoLast(out IUndoableCommand? redoneCommand)
        {
            redoneCommand = null;
            IUndoableCommand command;
            lock (_sync)
            {
                if (_redoStack.Count == 0) return false;
                command = _redoStack.Pop();
            }

            // Execute synchronously. The adapter will block for async commands.
            command.Execute();

            lock (_sync)
            {
                _undoStack.Push(command);
            }

            redoneCommand = command;
            return true;
        }

        /// <summary>
        /// Attempts to redo the last undone command asynchronously.
        /// Returns true if a command was redone; false when none available.
        /// </summary>
        public async Task<(bool success, IUndoableCommand? redoneCommand)> TryRedoLastAsync()
        {
            IUndoableCommand command;
            lock (_sync)
            {
                if (_redoStack.Count == 0) return (false, null);
                command = _redoStack.Pop();
            }

            if (command is IAsyncUndoableCommand asyncCmd)
            {
                await asyncCmd.ExecuteAsync().ConfigureAwait(false);
            }
            else
            {
                await Task.Run(() => command.Execute()).ConfigureAwait(false);
            }

            lock (_sync)
            {
                _undoStack.Push(command);
            }

            return (true, command);
        }

        /// <summary>
        /// Executes all commands currently in the queue in FIFO order.
        /// </summary>
        public void ExecuteAll()
        {
            while (TryExecuteNext(out _)) { }
        }

        /// <summary>
        /// Executes all commands currently in the queue asynchronously in FIFO order.
        /// </summary>
        public async Task ExecuteAllAsync()
        {
            while (true)
            {
                (bool success, T executedCommand) = await TryExecuteNextAsync().ConfigureAwait(false);
                if (!success) break;
            }
        }

        /// <summary>
        /// Undoes all commands currently recorded on the undo stack.
        /// </summary>
        public void UndoAll()
        {
            while (TryUndoLast(out _)) { }
        }

        /// <summary>
        /// Undoes all commands currently recorded on the undo stack asynchronously.
        /// </summary>
        public async Task UndoAllAsync()
        {
            while (true)
            {
                (bool success, IUndoableCommand? undoneCommand) = await TryUndoLastAsync().ConfigureAwait(false);
                if (!success) break;
            }
        }

        /// <summary>
        /// Redoes all commands currently recorded on the redo stack.
        /// </summary>
        public void RedoAll()
        {
            while (TryRedoLast(out _)) { }
        }

        /// <summary>
        /// Redoes all commands currently recorded on the redo stack asynchronously.
        /// </summary>
        public async Task RedoAllAsync()
        {
            while (true)
            {
                (bool success, IUndoableCommand? redoneCommand) = await TryRedoLastAsync().ConfigureAwait(false);
                if (!success) break;
            }
        }

        /// <summary>
        /// Clears the command queue and both undo/redo stacks.
        /// </summary>
        public void Clear()
        {
            lock (_sync)
            {
                _queue.Clear();
                _undoStack.Clear();
                _redoStack.Clear();
            }
        }
    }
}
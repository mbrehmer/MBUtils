using MBUtils.DesignPatterns.Command;

namespace MBUtils.Tests.DesignPatterns.Command
{
    public class CommandQueueTests
    {
        [Fact]
        public void ExecuteNext_DequeuesAndExecutes_SyncCommand()
        {
            CommandQueue<ICommand> queue = new CommandQueue<ICommand>();
            TestCommand cmd = new TestCommand();
            queue.Enqueue(cmd);

            queue.ExecuteNext();

            Assert.True(cmd.Executed);
        }

        [Fact]
        public async Task ExecuteNextAsync_ExecutesAsyncCommand()
        {
            CommandQueue<ICommand> queue = new CommandQueue<ICommand>();
            TestAsyncCommand cmd = new TestAsyncCommand();
            queue.Enqueue(cmd);

            await queue.ExecuteNextAsync();

            Assert.True(cmd.Executed);
        }

        [Fact]
        public void TryExecuteNext_ReturnsExecutedCommand()
        {
            CommandQueue<ICommand> queue = new CommandQueue<ICommand>();
            TestCommand cmd = new TestCommand();
            queue.Enqueue(cmd);

            ICommand executed;
            bool result = queue.TryExecuteNext(out executed);

            Assert.True(result);
            Assert.Same(cmd, executed);
        }

        [Fact]
        public void UndoRedo_SyncUndoableCommand_Works()
        {
            CommandQueue<ICommand> queue = new CommandQueue<ICommand>();
            TestUndoableCommand cmd = new TestUndoableCommand();
            queue.Enqueue(cmd);

            queue.ExecuteAll();
            Assert.Equal(1, cmd.Counter);

            queue.UndoLast();
            Assert.Equal(0, cmd.Counter);

            queue.RedoLast();
            Assert.Equal(1, cmd.Counter);
        }

        [Fact]
        public async Task UndoRedo_AsyncUndoableCommand_WorksAsync()
        {
            CommandQueue<ICommand> queue = new CommandQueue<ICommand>();
            TestAsyncUndoableCommand cmd = new TestAsyncUndoableCommand();
            queue.Enqueue(cmd);

            await queue.ExecuteAllAsync();
            Assert.Equal(1, cmd.Counter);

            await queue.TryUndoLastAsync();
            Assert.Equal(0, cmd.Counter);

            await queue.TryRedoLastAsync();
            Assert.Equal(1, cmd.Counter);
        }

        [Fact]
        public void Execute_ThrowingCommand_DoesNotAffectUndoStack()
        {
            CommandQueue<ICommand> queue = new CommandQueue<ICommand>();
            ThrowingCommand throwing = new ThrowingCommand();
            queue.Enqueue(throwing);

            Assert.Throws<InvalidOperationException>(() => queue.ExecuteNext());

            // No undo available
            bool undone = queue.TryUndoLast(out IUndoableCommand? _);
            Assert.False(undone);
        }

        [Fact]
        public void Clear_RemovesAllCommandsAndStacks()
        {
            CommandQueue<ICommand> queue = new CommandQueue<ICommand>();
            TestUndoableCommand cmd1 = new TestUndoableCommand();
            TestUndoableCommand cmd2 = new TestUndoableCommand();

            queue.Enqueue(cmd1);
            queue.Enqueue(cmd2);

            queue.ExecuteNext();
            queue.Clear();

            // After clear, nothing to execute or undo
            Assert.False(queue.TryExecuteNext(out ICommand _));
            Assert.False(queue.TryUndoLast(out IUndoableCommand? _));
            Assert.False(queue.TryRedoLast(out IUndoableCommand? _));
        }

        // --- Test helper command implementations ---
        private class TestCommand : ICommand
        {
            public bool Executed { get; private set; }

            public void Execute() => Executed = true;
        }

        private class TestAsyncCommand : IAsyncCommand
        {
            public bool Executed { get; private set; }

            public void Execute() => Executed = true;

            public Task ExecuteAsync()
            {
                Executed = true;
                return Task.CompletedTask;
            }
        }

        private class TestUndoableCommand : IUndoableCommand
        {
            public int Counter { get; private set; }

            public void Execute() => Counter++;

            public void Undo() => Counter--;
        }

        private class TestAsyncUndoableCommand : IAsyncUndoableCommand
        {
            public int Counter { get; private set; }

            public void Execute() => throw new NotSupportedException("Use ExecuteAsync for async command");

            public Task ExecuteAsync()
            {
                Counter++;
                return Task.CompletedTask;
            }

            public Task UndoAsync()
            {
                Counter--;
                return Task.CompletedTask;
            }

            public void Undo() => throw new NotSupportedException("Use UndoAsync for async command");
        }

        private class ThrowingCommand : ICommand
        {
            public void Execute() => throw new InvalidOperationException("Boom");
        }
    }
}
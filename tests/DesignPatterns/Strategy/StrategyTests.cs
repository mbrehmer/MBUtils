using MBUtils.DesignPatterns.Strategy;

namespace MBUtils.Tests.DesignPatterns.Strategy
{
    public class StrategyTests
    {
        [Fact]
        public void SingleStrategy_Execute_ReturnsExpected()
        {
            IStrategy<int, int> strategy = new IncrementStrategy();
            int result = strategy.Execute(41);
            Assert.Equal(42, result);
        }

        [Fact]
        public async Task SingleStrategy_ExecuteAsync_ReturnsExpected()
        {
            IStrategy<int, int> strategy = new AsyncIncrementStrategy();
            int result = await strategy.ExecuteAsync(100, CancellationToken.None);
            Assert.Equal(101, result);
        }

        [Fact]
        public void CompositeStrategy_ExecutesAll_ReturnsLastResult()
        {
            List<IStrategy<int, int>> list = new List<IStrategy<int, int>>
            {
                new MultiplyByTwoStrategy(),
                new AddTenStrategy()
            };

            IStrategy<int, int> composite = new CompositeStrategy<int, int>(list.AsReadOnly());
            int result = composite.Execute(5);
            // (5 * 2) + 10 = 20
            Assert.Equal(20, result);
        }

        [Fact]
        public async Task CompositeStrategy_ExecutesAllAsync_ReturnsLastResult()
        {
            List<IStrategy<int, int>> list = new List<IStrategy<int, int>>
            {
                new AsyncMultiplyByTwoStrategy(),
                new AsyncAddTenStrategy()
            };

            IStrategy<int, int> composite = new CompositeStrategy<int, int>(list.AsReadOnly());
            int result = await composite.ExecuteAsync(5, CancellationToken.None);
            // (5 * 2) + 10 = 20
            Assert.Equal(20, result);
        }

        // Local test strategies implementing interfaces explicitly

        private sealed class IncrementStrategy : StrategyBase<int, int>
        {
            protected override int ExecuteCore(int context)
            {
                return context + 1;
            }
        }

        private sealed class AsyncIncrementStrategy : StrategyBase<int, int>
        {
            protected override int ExecuteCore(int context)
            {
                return context + 1;
            }

            protected override async Task<int> ExecuteAsyncCore(int context, CancellationToken cancellationToken)
            {
                await Task.Delay(1, cancellationToken).ConfigureAwait(false);
                return ExecuteCore(context);
            }
        }

        private sealed class MultiplyByTwoStrategy : StrategyBase<int, int>
        {
            protected override int ExecuteCore(int context)
            {
                return context * 2;
            }
        }

        private sealed class AddTenStrategy : StrategyBase<int, int>
        {
            protected override int ExecuteCore(int context)
            {
                return context + 10;
            }
        }

        private sealed class AsyncMultiplyByTwoStrategy : StrategyBase<int, int>
        {
            protected override int ExecuteCore(int context)
            {
                return context * 2;
            }

            protected override async Task<int> ExecuteAsyncCore(int context, CancellationToken cancellationToken)
            {
                await Task.Delay(1, cancellationToken).ConfigureAwait(false);
                return ExecuteCore(context);
            }
        }

        private sealed class AsyncAddTenStrategy : StrategyBase<int, int>
        {
            protected override int ExecuteCore(int context)
            {
                return context + 10;
            }

            protected override async Task<int> ExecuteAsyncCore(int context, CancellationToken cancellationToken)
            {
                await Task.Delay(1, cancellationToken).ConfigureAwait(false);
                return ExecuteCore(context);
            }
        }
    }
}
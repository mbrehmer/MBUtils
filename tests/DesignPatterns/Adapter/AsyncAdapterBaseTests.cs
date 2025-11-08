using MBUtils.DesignPatterns.Adapter;

namespace MBUtils.Tests.DesignPatterns.Adapter
{
    public class AsyncAdapterBaseTests
    {
        private class TestSource
        {
            public string Value { get; set; } = string.Empty;
            public int Number { get; set; }
        }

        private class TestTarget
        {
            public string ConvertedValue { get; set; } = string.Empty;
            public int DoubledNumber { get; set; }
        }

        private class TestAsyncAdapter : AsyncAdapterBase<TestSource, TestTarget>
        {
            public int AdaptCallCount { get; private set; }

            protected override async Task<TestTarget> AdaptAsyncCore(TestSource source, CancellationToken cancellationToken)
            {
                AdaptCallCount++;
                await Task.Delay(10, cancellationToken);
                return new TestTarget
                {
                    ConvertedValue = source.Value.ToUpperInvariant(),
                    DoubledNumber = source.Number * 2
                };
            }
        }

        private class ThrowingAsyncAdapter : AsyncAdapterBase<TestSource, TestTarget>
        {
            protected override Task<TestTarget> AdaptAsyncCore(TestSource source, CancellationToken cancellationToken)
            {
                throw new InvalidOperationException("Test exception");
            }
        }

        private class CancellationRespectingAdapter : AsyncAdapterBase<TestSource, TestTarget>
        {
            protected override async Task<TestTarget> AdaptAsyncCore(TestSource source, CancellationToken cancellationToken)
            {
                await Task.Delay(100, cancellationToken);
                return new TestTarget
                {
                    ConvertedValue = source.Value,
                    DoubledNumber = source.Number
                };
            }
        }

        [Fact]
        public async Task AdaptAsync_WithValidSource_ReturnsAdaptedTarget()
        {
            // Arrange
            IAsyncAdapter<TestSource, TestTarget> adapter = new TestAsyncAdapter();
            TestSource source = new TestSource { Value = "test", Number = 5 };

            // Act
            TestTarget result = await adapter.AdaptAsync(source);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TEST", result.ConvertedValue);
            Assert.Equal(10, result.DoubledNumber);
        }

        [Fact]
        public async Task AdaptAsync_WithNullSource_ThrowsArgumentNullException()
        {
            // Arrange
            IAsyncAdapter<TestSource, TestTarget> adapter = new TestAsyncAdapter();

            // Act & Assert
            ArgumentNullException exception = await Assert.ThrowsAsync<ArgumentNullException>(() => adapter.AdaptAsync(null!));
            Assert.Equal("source", exception.ParamName);
        }

        [Fact]
        public async Task AdaptAsync_WhenAdaptAsyncCoreThrows_PropagatesException()
        {
            // Arrange
            IAsyncAdapter<TestSource, TestTarget> adapter = new ThrowingAsyncAdapter();
            TestSource source = new TestSource { Value = "test", Number = 5 };

            // Act & Assert
            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => adapter.AdaptAsync(source));
            Assert.Equal("Test exception", exception.Message);
        }

        [Fact]
        public async Task AdaptAsync_CalledMultipleTimes_WorksCorrectly()
        {
            // Arrange
            TestAsyncAdapter adapter = new TestAsyncAdapter();
            TestSource source1 = new TestSource { Value = "first", Number = 1 };
            TestSource source2 = new TestSource { Value = "second", Number = 2 };

            // Act
            TestTarget result1 = await ((IAsyncAdapter<TestSource, TestTarget>)adapter).AdaptAsync(source1);
            TestTarget result2 = await ((IAsyncAdapter<TestSource, TestTarget>)adapter).AdaptAsync(source2);

            // Assert
            Assert.Equal("FIRST", result1.ConvertedValue);
            Assert.Equal(2, result1.DoubledNumber);
            Assert.Equal("SECOND", result2.ConvertedValue);
            Assert.Equal(4, result2.DoubledNumber);
            Assert.Equal(2, adapter.AdaptCallCount);
        }

        [Fact]
        public async Task AdaptAsync_WithCancellationToken_RespectsCancellation()
        {
            // Arrange
            IAsyncAdapter<TestSource, TestTarget> adapter = new CancellationRespectingAdapter();
            TestSource source = new TestSource { Value = "test", Number = 5 };
            CancellationTokenSource cts = new CancellationTokenSource();

            // Act
            Task<TestTarget> task = adapter.AdaptAsync(source, cts.Token);
            cts.Cancel();

            // Assert
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
        }

        [Fact]
        public async Task AdaptAsync_WithDefaultCancellationToken_WorksCorrectly()
        {
            // Arrange
            IAsyncAdapter<TestSource, TestTarget> adapter = new TestAsyncAdapter();
            TestSource source = new TestSource { Value = "test", Number = 5 };

            // Act
            TestTarget result = await adapter.AdaptAsync(source, default);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TEST", result.ConvertedValue);
            Assert.Equal(10, result.DoubledNumber);
        }

        [Fact]
        public async Task AdaptAsync_WithoutCancellationToken_WorksCorrectly()
        {
            // Arrange
            IAsyncAdapter<TestSource, TestTarget> adapter = new TestAsyncAdapter();
            TestSource source = new TestSource { Value = "test", Number = 5 };

            // Act
            TestTarget result = await adapter.AdaptAsync(source);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TEST", result.ConvertedValue);
            Assert.Equal(10, result.DoubledNumber);
        }

        private class StringToIntAsyncAdapter : AsyncAdapterBase<string, int>
        {
            protected override async Task<int> AdaptAsyncCore(string source, CancellationToken cancellationToken)
            {
                await Task.Delay(1, cancellationToken);
                return int.Parse(source);
            }
        }

        [Fact]
        public async Task AdaptAsync_WithPrimitiveTypes_WorksCorrectly()
        {
            // Arrange
            IAsyncAdapter<string, int> adapter = new StringToIntAsyncAdapter();

            // Act
            int result = await adapter.AdaptAsync("42");

            // Assert
            Assert.Equal(42, result);
        }

        [Fact]
        public async Task AdaptAsync_WithEmptyString_HandlesEdgeCase()
        {
            // Arrange
            TestAsyncAdapter adapter = new TestAsyncAdapter();
            TestSource source = new TestSource { Value = "", Number = 0 };

            // Act
            TestTarget result = await ((IAsyncAdapter<TestSource, TestTarget>)adapter).AdaptAsync(source);

            // Assert
            Assert.Equal("", result.ConvertedValue);
            Assert.Equal(0, result.DoubledNumber);
        }

        [Fact]
        public async Task AdaptAsync_ParallelExecution_WorksCorrectly()
        {
            // Arrange
            TestAsyncAdapter adapter = new TestAsyncAdapter();
            TestSource source1 = new TestSource { Value = "first", Number = 1 };
            TestSource source2 = new TestSource { Value = "second", Number = 2 };
            TestSource source3 = new TestSource { Value = "third", Number = 3 };
            IAsyncAdapter<TestSource, TestTarget> interfaceAdapter = adapter;

            // Act
            Task<TestTarget>[] tasks = new[]
            {
                interfaceAdapter.AdaptAsync(source1),
                interfaceAdapter.AdaptAsync(source2),
                interfaceAdapter.AdaptAsync(source3)
            };
            TestTarget[] results = await Task.WhenAll(tasks);

            // Assert
            Assert.Equal(3, results.Length);
            Assert.Equal("FIRST", results[0].ConvertedValue);
            Assert.Equal("SECOND", results[1].ConvertedValue);
            Assert.Equal("THIRD", results[2].ConvertedValue);
            Assert.Equal(3, adapter.AdaptCallCount);
        }
    }
}
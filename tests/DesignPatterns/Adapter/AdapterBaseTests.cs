using MBUtils.DesignPatterns.Adapter;

namespace MBUtils.Tests.DesignPatterns.Adapter
{
    public class AdapterBaseTests
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

        private class TestAdapter : AdapterBase<TestSource, TestTarget>
        {
            protected override TestTarget AdaptCore(TestSource source)
            {
                return new TestTarget
                {
                    ConvertedValue = source.Value.ToUpperInvariant(),
                    DoubledNumber = source.Number * 2
                };
            }
        }

        private class ThrowingAdapter : AdapterBase<TestSource, TestTarget>
        {
            protected override TestTarget AdaptCore(TestSource source)
            {
                throw new InvalidOperationException("Test exception");
            }
        }

        [Fact]
        public void Adapt_WithValidSource_ReturnsAdaptedTarget()
        {
            // Arrange
            IAdapter<TestSource, TestTarget> adapter = new TestAdapter();
            TestSource source = new TestSource { Value = "test", Number = 5 };

            // Act
            TestTarget result = adapter.Adapt(source);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TEST", result.ConvertedValue);
            Assert.Equal(10, result.DoubledNumber);
        }

        [Fact]
        public void Adapt_WithNullSource_ThrowsArgumentNullException()
        {
            // Arrange
            IAdapter<TestSource, TestTarget> adapter = new TestAdapter();

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => adapter.Adapt(null!));
            Assert.Equal("source", exception.ParamName);
        }

        [Fact]
        public void Adapt_WhenAdaptCorethrows_PropagatesException()
        {
            // Arrange
            IAdapter<TestSource, TestTarget> adapter = new ThrowingAdapter();
            TestSource source = new TestSource { Value = "test", Number = 5 };

            // Act & Assert
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => adapter.Adapt(source));
            Assert.Equal("Test exception", exception.Message);
        }

        [Fact]
        public void Adapt_CalledMultipleTimes_WorksCorrectly()
        {
            // Arrange
            IAdapter<TestSource, TestTarget> adapter = new TestAdapter();
            TestSource source1 = new TestSource { Value = "first", Number = 1 };
            TestSource source2 = new TestSource { Value = "second", Number = 2 };

            // Act
            TestTarget result1 = adapter.Adapt(source1);
            TestTarget result2 = adapter.Adapt(source2);

            // Assert
            Assert.Equal("FIRST", result1.ConvertedValue);
            Assert.Equal(2, result1.DoubledNumber);
            Assert.Equal("SECOND", result2.ConvertedValue);
            Assert.Equal(4, result2.DoubledNumber);
        }

        [Fact]
        public void Adapt_WithDifferentSourceTypes_SupportsVariance()
        {
            // Arrange
            TestAdapter adapter = new TestAdapter();
            TestSource source = new TestSource { Value = "variance", Number = 3 };

            // Act - adapter can be used through interface
            IAdapter<TestSource, TestTarget> interfaceAdapter = adapter;
            TestTarget result = interfaceAdapter.Adapt(source);

            // Assert
            Assert.Equal("VARIANCE", result.ConvertedValue);
            Assert.Equal(6, result.DoubledNumber);
        }

        private class StringToIntAdapter : AdapterBase<string, int>
        {
            protected override int AdaptCore(string source)
            {
                return int.Parse(source);
            }
        }

        [Fact]
        public void Adapt_WithPrimitiveTypes_WorksCorrectly()
        {
            // Arrange
            IAdapter<string, int> adapter = new StringToIntAdapter();

            // Act
            int result = adapter.Adapt("42");

            // Assert
            Assert.Equal(42, result);
        }

        [Fact]
        public void Adapt_WithEmptyString_HandlesEdgeCase()
        {
            // Arrange
            IAdapter<TestSource, TestTarget> adapter = new TestAdapter();
            TestSource source = new TestSource { Value = "", Number = 0 };

            // Act
            TestTarget result = adapter.Adapt(source);

            // Assert
            Assert.Equal("", result.ConvertedValue);
            Assert.Equal(0, result.DoubledNumber);
        }
    }
}
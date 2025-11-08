using MBUtils.DesignPatterns.Builder;

namespace MBUtils.Tests.DesignPatterns.Builder
{
    public class AsyncBuilderBaseTests
    {
        [Fact]
        public async Task BuildAsync_WithValidState_ReturnsProduct()
        {
            TestAsyncProductBuilder builder = new TestAsyncProductBuilder();
            builder.WithName("Async Product");

            TestAsyncProduct product = await builder.BuildAsync();

            Assert.NotNull(product);
            Assert.Equal("Async Product", product.Name);
        }

        [Fact]
        public async Task BuildAsync_WithInvalidState_ThrowsInvalidOperationException()
        {
            TestAsyncProductBuilder builder = new TestAsyncProductBuilder();
            // Don't set name (required field)

            InvalidOperationException ex = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await builder.BuildAsync());
            Assert.Contains("Name is required", ex.Message);
        }

        [Fact]
        public async Task BuildAsync_CalledTwiceWithoutReset_ThrowsInvalidOperationException()
        {
            TestAsyncProductBuilder builder = new TestAsyncProductBuilder();
            builder.WithName("Async Product");

            await builder.BuildAsync();

            InvalidOperationException ex = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await builder.BuildAsync());
            Assert.Contains("already been used", ex.Message);
        }

        [Fact]
        public async Task Reset_AllowsBuilderReuse()
        {
            TestAsyncProductBuilder builder = new TestAsyncProductBuilder();
            builder.WithName("First Async Product");

            TestAsyncProduct first = await builder.BuildAsync();

            builder.Reset();
            builder.WithName("Second Async Product");

            TestAsyncProduct second = await builder.BuildAsync();

            Assert.Equal("First Async Product", first.Name);
            Assert.Equal("Second Async Product", second.Name);
        }

        [Fact]
        public async Task IsValidAsync_WithValidState_ReturnsTrue()
        {
            TestAsyncProductBuilder builder = new TestAsyncProductBuilder();
            builder.WithName("Async Product");

            bool isValid = await builder.IsValidAsync();

            Assert.True(isValid);
        }

        [Fact]
        public async Task IsValidAsync_WithInvalidState_ReturnsFalse()
        {
            TestAsyncProductBuilder builder = new TestAsyncProductBuilder();
            // Don't set name (required field)

            bool isValid = await builder.IsValidAsync();

            Assert.False(isValid);
        }

        [Fact]
        public async Task IsValidAsync_AfterBuild_ReturnsFalse()
        {
            TestAsyncProductBuilder builder = new TestAsyncProductBuilder();
            builder.WithName("Async Product");
            await builder.BuildAsync();

            bool isValid = await builder.IsValidAsync();

            Assert.False(isValid);
        }

        [Fact]
        public async Task FluentInterface_SupportsMethodChaining()
        {
            TestAsyncProductBuilder builder = new TestAsyncProductBuilder();

            TestAsyncProduct product = await builder
                .WithName("Chained Async Product")
                .WithDescription("Chained Async Description")
                .WithQuantity(100)
                .BuildAsync();

            Assert.Equal("Chained Async Product", product.Name);
            Assert.Equal("Chained Async Description", product.Description);
            Assert.Equal(100, product.Quantity);
        }

        [Fact]
        public async Task BuildAsync_WithCancellationToken_CanBeCancelled()
        {
            TestAsyncProductBuilder builder = new TestAsyncProductBuilder();
            builder.WithName("Cancellable Product");
            builder.SetBuildDelay(TimeSpan.FromSeconds(5));

            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMilliseconds(100));

            await Assert.ThrowsAsync<TaskCanceledException>(
                async () => await builder.BuildAsync(cts.Token));
        }

        [Fact]
        public async Task ValidateAsync_WithMultipleErrors_ReportsAll()
        {
            TestAsyncProductBuilder builder = new TestAsyncProductBuilder();
            // Don't set name (required) and set negative quantity (invalid)
            builder.WithQuantity(-5);

            InvalidOperationException ex = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await builder.BuildAsync());
            Assert.Contains("Name is required", ex.Message);
            Assert.Contains("Quantity must be non-negative", ex.Message);
        }

        [Fact]
        public async Task Reset_ClearsBuilderState()
        {
            TestAsyncProductBuilder builder = new TestAsyncProductBuilder();
            builder.WithName("Async Product")
                   .WithDescription("Async Description")
                   .WithQuantity(50);

            builder.Reset();

            // After reset, should not be valid (missing name)
            bool isValid = await builder.IsValidAsync();
            Assert.False(isValid);
        }

        // --- Test helper classes ---
        private class TestAsyncProduct
        {
            public string? Name { get; set; }
            public string? Description { get; set; }
            public int Quantity { get; set; }
        }

        private class TestAsyncProductBuilder : AsyncBuilderBase<TestAsyncProductBuilder, TestAsyncProduct>
        {
            private string? _name;
            private string? _description;
            private int _quantity;
            private TimeSpan _buildDelay = TimeSpan.Zero;

            public TestAsyncProductBuilder WithName(string name)
            {
                _name = name;
                return This();
            }

            public TestAsyncProductBuilder WithDescription(string description)
            {
                _description = description;
                return This();
            }

            public TestAsyncProductBuilder WithQuantity(int quantity)
            {
                _quantity = quantity;
                return This();
            }

            public void SetBuildDelay(TimeSpan delay)
            {
                _buildDelay = delay;
            }

            protected override void ResetCore()
            {
                _name = null;
                _description = null;
                _quantity = 0;
                _buildDelay = TimeSpan.Zero;
            }

            protected override async Task<TestAsyncProduct> BuildAsyncCore(CancellationToken cancellationToken)
            {
                // Simulate async operation
                if (_buildDelay > TimeSpan.Zero)
                {
                    await Task.Delay(_buildDelay, cancellationToken);
                }
                else
                {
                    await Task.Delay(10, cancellationToken);
                }

                return new TestAsyncProduct
                {
                    Name = _name,
                    Description = _description,
                    Quantity = _quantity
                };
            }

            protected override async Task<IReadOnlyCollection<string>> ValidateAsync(CancellationToken cancellationToken)
            {
                // Simulate async validation
                await Task.Delay(10, cancellationToken);

                List<string> errors = new List<string>();

                if (string.IsNullOrWhiteSpace(_name))
                    errors.Add("Name is required");

                if (_quantity < 0)
                    errors.Add("Quantity must be non-negative");

                return errors;
            }
        }
    }
}
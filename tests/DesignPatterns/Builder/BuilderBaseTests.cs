using MBUtils.DesignPatterns.Builder;

namespace MBUtils.Tests.DesignPatterns.Builder
{
    public class BuilderBaseTests
    {
        [Fact]
        public void Build_WithValidState_ReturnsProduct()
        {
            TestProductBuilder builder = new TestProductBuilder();
            builder.WithName("Test Product");

            TestProduct product = builder.Build();

            Assert.NotNull(product);
            Assert.Equal("Test Product", product.Name);
        }

        [Fact]
        public void Build_WithInvalidState_ThrowsInvalidOperationException()
        {
            TestProductBuilder builder = new TestProductBuilder();
            // Don't set name (required field)

            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => builder.Build());
            Assert.Contains("Name is required", ex.Message);
        }

        [Fact]
        public void Build_CalledTwiceWithoutReset_ThrowsInvalidOperationException()
        {
            TestProductBuilder builder = new TestProductBuilder();
            builder.WithName("Test Product");

            builder.Build();

            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => builder.Build());
            Assert.Contains("already been used", ex.Message);
        }

        [Fact]
        public void Reset_AllowsBuilderReuse()
        {
            TestProductBuilder builder = new TestProductBuilder();
            builder.WithName("First Product");

            TestProduct first = builder.Build();

            builder.Reset();
            builder.WithName("Second Product");

            TestProduct second = builder.Build();

            Assert.Equal("First Product", first.Name);
            Assert.Equal("Second Product", second.Name);
        }

        [Fact]
        public void IsValid_WithValidState_ReturnsTrue()
        {
            TestProductBuilder builder = new TestProductBuilder();
            builder.WithName("Test Product");

            Assert.True(builder.IsValid);
        }

        [Fact]
        public void IsValid_WithInvalidState_ReturnsFalse()
        {
            TestProductBuilder builder = new TestProductBuilder();
            // Don't set name (required field)

            Assert.False(builder.IsValid);
        }

        [Fact]
        public void IsValid_AfterBuild_ReturnsFalse()
        {
            TestProductBuilder builder = new TestProductBuilder();
            builder.WithName("Test Product");
            builder.Build();

            Assert.False(builder.IsValid);
        }

        [Fact]
        public void FluentInterface_SupportsMethodChaining()
        {
            TestProductBuilder builder = new TestProductBuilder();

            TestProduct product = builder
                .WithName("Chained Product")
                .WithDescription("Chained Description")
                .WithPrice(99.99m)
                .Build();

            Assert.Equal("Chained Product", product.Name);
            Assert.Equal("Chained Description", product.Description);
            Assert.Equal(99.99m, product.Price);
        }

        [Fact]
        public void Build_WithMultipleValidationErrors_ReportsAll()
        {
            TestProductBuilder builder = new TestProductBuilder();
            // Don't set name (required) and set negative price (invalid)
            builder.WithPrice(-10);

            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => builder.Build());
            Assert.Contains("Name is required", ex.Message);
            Assert.Contains("Price must be non-negative", ex.Message);
        }

        [Fact]
        public void Reset_ClearsBuilderState()
        {
            TestProductBuilder builder = new TestProductBuilder();
            builder.WithName("Test Product")
                   .WithDescription("Test Description")
                   .WithPrice(50);

            builder.Reset();

            Assert.False(builder.IsValid);
        }

        // --- Test helper classes ---
        private class TestProduct
        {
            public string? Name { get; set; }
            public string? Description { get; set; }
            public decimal Price { get; set; }
        }

        private class TestProductBuilder : BuilderBase<TestProductBuilder, TestProduct>
        {
            private string? _name;
            private string? _description;
            private decimal _price;

            public TestProductBuilder WithName(string name)
            {
                _name = name;
                return This();
            }

            public TestProductBuilder WithDescription(string description)
            {
                _description = description;
                return This();
            }

            public TestProductBuilder WithPrice(decimal price)
            {
                _price = price;
                return This();
            }

            protected override void ResetCore()
            {
                _name = null;
                _description = null;
                _price = 0;
            }

            protected override TestProduct BuildCore()
            {
                return new TestProduct
                {
                    Name = _name,
                    Description = _description,
                    Price = _price
                };
            }

            protected override IReadOnlyCollection<string> Validate()
            {
                List<string> errors = new List<string>();

                if (string.IsNullOrWhiteSpace(_name))
                    errors.Add("Name is required");

                if (_price < 0)
                    errors.Add("Price must be non-negative");

                return errors;
            }
        }
    }
}
using MBUtils.DesignPatterns.Builder;

namespace MBUtils.Tests.DesignPatterns.Builder
{
    public class DirectorTests
    {
        [Fact]
        public void Constructor_WithBuilder_SetsBuilder()
        {
            TestProductBuilder builder = new TestProductBuilder();
            Director<TestProductBuilder, TestProduct> director = new Director<TestProductBuilder, TestProduct>(builder);

            Assert.Same(builder, director.Builder);
        }

        [Fact]
        public void Constructor_WithNullBuilder_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new Director<TestProductBuilder, TestProduct>(null!));
        }

        [Fact]
        public void Builder_SetToNull_ThrowsArgumentNullException()
        {
            Director<TestProductBuilder, TestProduct> director = new Director<TestProductBuilder, TestProduct>();

            Assert.Throws<ArgumentNullException>(() => director.Builder = null!);
        }

        [Fact]
        public void Builder_GetWithoutSet_ThrowsInvalidOperationException()
        {
            Director<TestProductBuilder, TestProduct> director = new Director<TestProductBuilder, TestProduct>();

            Assert.Throws<InvalidOperationException>(() => director.Builder);
        }

        [Fact]
        public void Construct_WithAction_BuildsProduct()
        {
            TestProductBuilder builder = new TestProductBuilder();
            Director<TestProductBuilder, TestProduct> director = new Director<TestProductBuilder, TestProduct>(builder);

            TestProduct product = director.Construct(b =>
            {
                b.WithName("Director Product");
                b.WithDescription("Built by Director");
            });

            Assert.NotNull(product);
            Assert.Equal("Director Product", product.Name);
            Assert.Equal("Built by Director", product.Description);
        }

        [Fact]
        public void Construct_ResetsBuilderBeforeConstruction()
        {
            TestProductBuilder builder = new TestProductBuilder();
            builder.WithName("Old Name");

            Director<TestProductBuilder, TestProduct> director = new Director<TestProductBuilder, TestProduct>(builder);

            TestProduct product = director.Construct(b =>
            {
                b.WithName("New Name");
            });

            Assert.Equal("New Name", product.Name);
        }

        [Fact]
        public void Construct_WithNullAction_ThrowsArgumentNullException()
        {
            TestProductBuilder builder = new TestProductBuilder();
            Director<TestProductBuilder, TestProduct> director = new Director<TestProductBuilder, TestProduct>(builder);

            Assert.Throws<ArgumentNullException>(() => director.Construct((Action<TestProductBuilder>)null!));
        }

        [Fact]
        public void Construct_WithStrategy_BuildsProduct()
        {
            TestProductBuilder builder = new TestProductBuilder();
            Director<TestProductBuilder, TestProduct> director = new Director<TestProductBuilder, TestProduct>(builder);
            TestConstructionStrategy strategy = new TestConstructionStrategy("Strategy Product");

            TestProduct product = director.Construct(strategy);

            Assert.NotNull(product);
            Assert.Equal("Strategy Product", product.Name);
        }

        [Fact]
        public void Construct_WithNullStrategy_ThrowsArgumentNullException()
        {
            TestProductBuilder builder = new TestProductBuilder();
            Director<TestProductBuilder, TestProduct> director = new Director<TestProductBuilder, TestProduct>(builder);

            Assert.Throws<ArgumentNullException>(() => director.Construct((IConstructionStrategy<TestProductBuilder, TestProduct>)null!));
        }

        [Fact]
        public void Construct_MultipleTimes_EachCallResetsBuilder()
        {
            TestProductBuilder builder = new TestProductBuilder();
            Director<TestProductBuilder, TestProduct> director = new Director<TestProductBuilder, TestProduct>(builder);

            TestProduct first = director.Construct(b => b.WithName("First"));
            TestProduct second = director.Construct(b => b.WithName("Second"));
            TestProduct third = director.Construct(b => b.WithName("Third"));

            Assert.Equal("First", first.Name);
            Assert.Equal("Second", second.Name);
            Assert.Equal("Third", third.Name);
        }

        [Fact]
        public void Construct_WithDifferentStrategies_BuildsDifferentProducts()
        {
            TestProductBuilder builder = new TestProductBuilder();
            Director<TestProductBuilder, TestProduct> director = new Director<TestProductBuilder, TestProduct>(builder);

            TestProduct basic = director.Construct(new BasicProductStrategy());
            TestProduct premium = director.Construct(new PremiumProductStrategy());

            Assert.Equal("Basic Product", basic.Name);
            Assert.Equal(0, basic.Price);

            Assert.Equal("Premium Product", premium.Name);
            Assert.Equal(999.99m, premium.Price);
        }

        [Fact]
        public void Construct_WithoutBuilderSet_ThrowsInvalidOperationException()
        {
            Director<TestProductBuilder, TestProduct> director = new Director<TestProductBuilder, TestProduct>();

            Assert.Throws<InvalidOperationException>(() =>
                director.Construct(b => b.WithName("Test")));
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

                return errors;
            }
        }

        private class TestConstructionStrategy : IConstructionStrategy<TestProductBuilder, TestProduct>
        {
            private readonly string _name;

            public TestConstructionStrategy(string name)
            {
                _name = name;
            }

            public TestProduct Construct(TestProductBuilder builder)
            {
                return builder
                    .WithName(_name)
                    .Build();
            }
        }

        private class BasicProductStrategy : IConstructionStrategy<TestProductBuilder, TestProduct>
        {
            public TestProduct Construct(TestProductBuilder builder)
            {
                return builder
                    .WithName("Basic Product")
                    .WithDescription("A basic product")
                    .WithPrice(0)
                    .Build();
            }
        }

        private class PremiumProductStrategy : IConstructionStrategy<TestProductBuilder, TestProduct>
        {
            public TestProduct Construct(TestProductBuilder builder)
            {
                return builder
                    .WithName("Premium Product")
                    .WithDescription("A premium product with all features")
                    .WithPrice(999.99m)
                    .Build();
            }
        }
    }
}
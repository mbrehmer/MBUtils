using MBUtils.DesignPatterns.Factory;

namespace MBUtils.Tests.DesignPatterns.Factory
{
    public class FactoryBaseTests
    {
        // Test types
        private interface ITestProduct
        {
            string GetName();
        }

        private class TestProductA : ITestProduct
        {
            public string GetName() => "Product A";
        }

        private class TestProductB : ITestProduct
        {
            public string GetName() => "Product B";
        }

        private class TestProductC : ITestProduct
        {
            public string GetName() => "Product C";
        }

        // Concrete factory for testing
        private class TestFactory : FactoryBase<string, ITestProduct>
        {
            public void RegisterPublic(string key, Func<ITestProduct> creator)
            {
                Register(key, creator);
            }

            public bool UnregisterPublic(string key)
            {
                return Unregister(key);
            }

            public void ClearPublic()
            {
                ClearRegistrations();
            }

            public int CountPublic => RegistrationCount;
        }

        [Fact]
        public void Constructor_InitializesEmptyFactory()
        {
            TestFactory factory = new TestFactory();

            Assert.Equal(0, factory.CountPublic);
        }

        [Fact]
        public void Register_WithValidKeyAndCreator_RegistersSuccessfully()
        {
            TestFactory factory = new TestFactory();

            factory.RegisterPublic("A", () => new TestProductA());

            Assert.Equal(1, factory.CountPublic);
            Assert.True(factory.IsRegistered("A"));
        }

        [Fact]
        public void Register_WithNullKey_ThrowsArgumentNullException()
        {
            TestFactory factory = new TestFactory();

            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
                () => factory.RegisterPublic(null!, () => new TestProductA()));

            Assert.Equal("key", exception.ParamName);
        }

        [Fact]
        public void Register_WithNullCreator_ThrowsArgumentNullException()
        {
            TestFactory factory = new TestFactory();

            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
                () => factory.RegisterPublic("A", null!));

            Assert.Equal("creator", exception.ParamName);
        }

        [Fact]
        public void Register_WithDuplicateKey_ThrowsInvalidOperationException()
        {
            TestFactory factory = new TestFactory();
            factory.RegisterPublic("A", () => new TestProductA());

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => factory.RegisterPublic("A", () => new TestProductA()));

            Assert.Contains("already registered", exception.Message);
            Assert.Contains("A", exception.Message);
        }

        [Fact]
        public void Create_WithRegisteredKey_ReturnsNewInstance()
        {
            TestFactory factory = new TestFactory();
            factory.RegisterPublic("A", () => new TestProductA());

            ITestProduct product = factory.Create("A");

            Assert.NotNull(product);
            Assert.IsType<TestProductA>(product);
            Assert.Equal("Product A", product.GetName());
        }

        [Fact]
        public void Create_WithUnregisteredKey_ThrowsInvalidOperationException()
        {
            TestFactory factory = new TestFactory();

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => factory.Create("Unknown"));

            Assert.Contains("No creator registered", exception.Message);
            Assert.Contains("Unknown", exception.Message);
        }

        [Fact]
        public void Create_WithNullKey_ThrowsArgumentNullException()
        {
            TestFactory factory = new TestFactory();

            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
                () => factory.Create(null!));

            Assert.Equal("key", exception.ParamName);
        }

        [Fact]
        public void Create_CalledMultipleTimes_CreatesNewInstancesEachTime()
        {
            TestFactory factory = new TestFactory();
            factory.RegisterPublic("A", () => new TestProductA());

            ITestProduct product1 = factory.Create("A");
            ITestProduct product2 = factory.Create("A");

            Assert.NotNull(product1);
            Assert.NotNull(product2);
            Assert.NotSame(product1, product2);
        }

        [Fact]
        public void Create_WithMultipleRegistrations_CreatesCorrectType()
        {
            TestFactory factory = new TestFactory();
            factory.RegisterPublic("A", () => new TestProductA());
            factory.RegisterPublic("B", () => new TestProductB());
            factory.RegisterPublic("C", () => new TestProductC());

            ITestProduct productA = factory.Create("A");
            ITestProduct productB = factory.Create("B");
            ITestProduct productC = factory.Create("C");

            Assert.IsType<TestProductA>(productA);
            Assert.IsType<TestProductB>(productB);
            Assert.IsType<TestProductC>(productC);
            Assert.Equal("Product A", productA.GetName());
            Assert.Equal("Product B", productB.GetName());
            Assert.Equal("Product C", productC.GetName());
        }

        [Fact]
        public void IsRegistered_WithRegisteredKey_ReturnsTrue()
        {
            TestFactory factory = new TestFactory();
            factory.RegisterPublic("A", () => new TestProductA());

            bool isRegistered = factory.IsRegistered("A");

            Assert.True(isRegistered);
        }

        [Fact]
        public void IsRegistered_WithUnregisteredKey_ReturnsFalse()
        {
            TestFactory factory = new TestFactory();

            bool isRegistered = factory.IsRegistered("Unknown");

            Assert.False(isRegistered);
        }

        [Fact]
        public void IsRegistered_WithNullKey_ReturnsFalse()
        {
            TestFactory factory = new TestFactory();

            bool isRegistered = factory.IsRegistered(null!);

            Assert.False(isRegistered);
        }

        [Fact]
        public void Unregister_WithRegisteredKey_RemovesCreator()
        {
            TestFactory factory = new TestFactory();
            factory.RegisterPublic("A", () => new TestProductA());

            bool result = factory.UnregisterPublic("A");

            Assert.True(result);
            Assert.False(factory.IsRegistered("A"));
            Assert.Equal(0, factory.CountPublic);
        }

        [Fact]
        public void Unregister_WithUnregisteredKey_ReturnsFalse()
        {
            TestFactory factory = new TestFactory();

            bool result = factory.UnregisterPublic("Unknown");

            Assert.False(result);
        }

        [Fact]
        public void Unregister_WithNullKey_ReturnsFalse()
        {
            TestFactory factory = new TestFactory();

            bool result = factory.UnregisterPublic(null!);

            Assert.False(result);
        }

        [Fact]
        public void ClearRegistrations_RemovesAllCreators()
        {
            TestFactory factory = new TestFactory();
            factory.RegisterPublic("A", () => new TestProductA());
            factory.RegisterPublic("B", () => new TestProductB());
            factory.RegisterPublic("C", () => new TestProductC());

            factory.ClearPublic();

            Assert.Equal(0, factory.CountPublic);
            Assert.False(factory.IsRegistered("A"));
            Assert.False(factory.IsRegistered("B"));
            Assert.False(factory.IsRegistered("C"));
        }

        [Fact]
        public void RegistrationCount_ReflectsNumberOfRegistrations()
        {
            TestFactory factory = new TestFactory();

            Assert.Equal(0, factory.CountPublic);

            factory.RegisterPublic("A", () => new TestProductA());
            Assert.Equal(1, factory.CountPublic);

            factory.RegisterPublic("B", () => new TestProductB());
            Assert.Equal(2, factory.CountPublic);

            factory.UnregisterPublic("A");
            Assert.Equal(1, factory.CountPublic);

            factory.ClearPublic();
            Assert.Equal(0, factory.CountPublic);
        }

        [Fact]
        public void Constructor_WithCustomComparer_UsesCaseInsensitiveKeys()
        {
            TestFactoryWithComparer factory = new TestFactoryWithComparer();
            factory.RegisterPublic("test", () => new TestProductA());

            Assert.True(factory.IsRegistered("test"));
            Assert.True(factory.IsRegistered("TEST"));
            Assert.True(factory.IsRegistered("Test"));
            Assert.True(factory.IsRegistered("TeSt"));
        }

        [Fact]
        public void Constructor_WithNullComparer_ThrowsArgumentNullException()
        {
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
                () => new TestFactoryWithNullComparer());

            Assert.Equal("comparer", exception.ParamName);
        }

        // Factory with custom comparer for testing
        private class TestFactoryWithComparer : FactoryBase<string, ITestProduct>
        {
            public TestFactoryWithComparer() : base(StringComparer.OrdinalIgnoreCase)
            {
            }

            public void RegisterPublic(string key, Func<ITestProduct> creator)
            {
                Register(key, creator);
            }
        }

        // Factory with null comparer for testing
        private class TestFactoryWithNullComparer : FactoryBase<string, ITestProduct>
        {
            public TestFactoryWithNullComparer() : base(null!)
            {
            }
        }
    }
}
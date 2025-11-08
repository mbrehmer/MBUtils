using MBUtils.DesignPatterns.Factory;

namespace MBUtils.Tests.DesignPatterns.Factory
{
    public class AsyncFactoryBaseTests
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
        private class TestAsyncFactory : AsyncFactoryBase<string, ITestProduct>
        {
            public void RegisterPublic(string key, Func<CancellationToken, Task<ITestProduct>> creator)
            {
                Register(key, creator);
            }

            public void RegisterPublicSync(string key, Func<ITestProduct> creator)
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
            TestAsyncFactory factory = new TestAsyncFactory();

            Assert.Equal(0, factory.CountPublic);
        }

        [Fact]
        public void Register_WithValidKeyAndAsyncCreator_RegistersSuccessfully()
        {
            TestAsyncFactory factory = new TestAsyncFactory();

            factory.RegisterPublic("A", async ct =>
            {
                await Task.Delay(1, ct);
                return new TestProductA();
            });

            Assert.Equal(1, factory.CountPublic);
            Assert.True(factory.IsRegistered("A"));
        }

        [Fact]
        public void Register_WithValidKeyAndSyncCreator_RegistersSuccessfully()
        {
            TestAsyncFactory factory = new TestAsyncFactory();

            factory.RegisterPublicSync("A", () => new TestProductA());

            Assert.Equal(1, factory.CountPublic);
            Assert.True(factory.IsRegistered("A"));
        }

        [Fact]
        public void Register_WithNullKey_ThrowsArgumentNullException()
        {
            TestAsyncFactory factory = new TestAsyncFactory();

            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
                () => factory.RegisterPublic(null!, ct => Task.FromResult<ITestProduct>(new TestProductA())));

            Assert.Equal("key", exception.ParamName);
        }

        [Fact]
        public void Register_WithNullAsyncCreator_ThrowsArgumentNullException()
        {
            TestAsyncFactory factory = new TestAsyncFactory();

            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
                () => factory.RegisterPublic("A", null!));

            Assert.Equal("creator", exception.ParamName);
        }

        [Fact]
        public void Register_WithNullSyncCreator_ThrowsArgumentNullException()
        {
            TestAsyncFactory factory = new TestAsyncFactory();

            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
                () => factory.RegisterPublicSync("A", null!));

            Assert.Equal("creator", exception.ParamName);
        }

        [Fact]
        public void Register_WithDuplicateKey_ThrowsInvalidOperationException()
        {
            TestAsyncFactory factory = new TestAsyncFactory();
            factory.RegisterPublic("A", ct => Task.FromResult<ITestProduct>(new TestProductA()));

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
                () => factory.RegisterPublic("A", ct => Task.FromResult<ITestProduct>(new TestProductA())));

            Assert.Contains("already registered", exception.Message);
            Assert.Contains("A", exception.Message);
        }

        [Fact]
        public async Task CreateAsync_WithRegisteredKey_ReturnsNewInstance()
        {
            TestAsyncFactory factory = new TestAsyncFactory();
            factory.RegisterPublic("A", async ct =>
            {
                await Task.Delay(10, ct);
                return new TestProductA();
            });

            ITestProduct product = await factory.CreateAsync("A");

            Assert.NotNull(product);
            Assert.IsType<TestProductA>(product);
            Assert.Equal("Product A", product.GetName());
        }

        [Fact]
        public async Task CreateAsync_WithSyncCreator_ReturnsNewInstance()
        {
            TestAsyncFactory factory = new TestAsyncFactory();
            factory.RegisterPublicSync("A", () => new TestProductA());

            ITestProduct product = await factory.CreateAsync("A");

            Assert.NotNull(product);
            Assert.IsType<TestProductA>(product);
            Assert.Equal("Product A", product.GetName());
        }

        [Fact]
        public async Task CreateAsync_WithUnregisteredKey_ThrowsInvalidOperationException()
        {
            TestAsyncFactory factory = new TestAsyncFactory();

            InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await factory.CreateAsync("Unknown"));

            Assert.Contains("No creator registered", exception.Message);
            Assert.Contains("Unknown", exception.Message);
        }

        [Fact]
        public async Task CreateAsync_WithNullKey_ThrowsArgumentNullException()
        {
            TestAsyncFactory factory = new TestAsyncFactory();

            ArgumentNullException exception = await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await factory.CreateAsync(null!));

            Assert.Equal("key", exception.ParamName);
        }

        [Fact]
        public async Task CreateAsync_CalledMultipleTimes_CreatesNewInstancesEachTime()
        {
            TestAsyncFactory factory = new TestAsyncFactory();
            factory.RegisterPublic("A", ct => Task.FromResult<ITestProduct>(new TestProductA()));

            ITestProduct product1 = await factory.CreateAsync("A");
            ITestProduct product2 = await factory.CreateAsync("A");

            Assert.NotNull(product1);
            Assert.NotNull(product2);
            Assert.NotSame(product1, product2);
        }

        [Fact]
        public async Task CreateAsync_WithMultipleRegistrations_CreatesCorrectType()
        {
            TestAsyncFactory factory = new TestAsyncFactory();
            factory.RegisterPublic("A", ct => Task.FromResult<ITestProduct>(new TestProductA()));
            factory.RegisterPublic("B", ct => Task.FromResult<ITestProduct>(new TestProductB()));
            factory.RegisterPublic("C", ct => Task.FromResult<ITestProduct>(new TestProductC()));

            ITestProduct productA = await factory.CreateAsync("A");
            ITestProduct productB = await factory.CreateAsync("B");
            ITestProduct productC = await factory.CreateAsync("C");

            Assert.IsType<TestProductA>(productA);
            Assert.IsType<TestProductB>(productB);
            Assert.IsType<TestProductC>(productC);
            Assert.Equal("Product A", productA.GetName());
            Assert.Equal("Product B", productB.GetName());
            Assert.Equal("Product C", productC.GetName());
        }

        [Fact]
        public async Task CreateAsync_WithCancellationToken_PassesTokenToCreator()
        {
            TestAsyncFactory factory = new TestAsyncFactory();
            bool tokenPassed = false;

            factory.RegisterPublic("A", async ct =>
            {
                tokenPassed = ct.CanBeCanceled;
                await Task.Delay(1, ct);
                return new TestProductA();
            });

            CancellationTokenSource cts = new CancellationTokenSource();
            ITestProduct product = await factory.CreateAsync("A", cts.Token);

            Assert.True(tokenPassed);
            Assert.NotNull(product);
        }

        [Fact]
        public async Task CreateAsync_WithCancelledToken_ThrowsOperationCanceledException()
        {
            TestAsyncFactory factory = new TestAsyncFactory();
            factory.RegisterPublic("A", async ct =>
            {
                await Task.Delay(100, ct);
                return new TestProductA();
            });

            CancellationTokenSource cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                async () => await factory.CreateAsync("A", cts.Token));
        }

        [Fact]
        public void IsRegistered_WithRegisteredKey_ReturnsTrue()
        {
            TestAsyncFactory factory = new TestAsyncFactory();
            factory.RegisterPublic("A", ct => Task.FromResult<ITestProduct>(new TestProductA()));

            bool isRegistered = factory.IsRegistered("A");

            Assert.True(isRegistered);
        }

        [Fact]
        public void IsRegistered_WithUnregisteredKey_ReturnsFalse()
        {
            TestAsyncFactory factory = new TestAsyncFactory();

            bool isRegistered = factory.IsRegistered("Unknown");

            Assert.False(isRegistered);
        }

        [Fact]
        public void IsRegistered_WithNullKey_ReturnsFalse()
        {
            TestAsyncFactory factory = new TestAsyncFactory();

            bool isRegistered = factory.IsRegistered(null!);

            Assert.False(isRegistered);
        }

        [Fact]
        public void Unregister_WithRegisteredKey_RemovesCreator()
        {
            TestAsyncFactory factory = new TestAsyncFactory();
            factory.RegisterPublic("A", ct => Task.FromResult<ITestProduct>(new TestProductA()));

            bool result = factory.UnregisterPublic("A");

            Assert.True(result);
            Assert.False(factory.IsRegistered("A"));
            Assert.Equal(0, factory.CountPublic);
        }

        [Fact]
        public void Unregister_WithUnregisteredKey_ReturnsFalse()
        {
            TestAsyncFactory factory = new TestAsyncFactory();

            bool result = factory.UnregisterPublic("Unknown");

            Assert.False(result);
        }

        [Fact]
        public void Unregister_WithNullKey_ReturnsFalse()
        {
            TestAsyncFactory factory = new TestAsyncFactory();

            bool result = factory.UnregisterPublic(null!);

            Assert.False(result);
        }

        [Fact]
        public void ClearRegistrations_RemovesAllCreators()
        {
            TestAsyncFactory factory = new TestAsyncFactory();
            factory.RegisterPublic("A", ct => Task.FromResult<ITestProduct>(new TestProductA()));
            factory.RegisterPublic("B", ct => Task.FromResult<ITestProduct>(new TestProductB()));
            factory.RegisterPublic("C", ct => Task.FromResult<ITestProduct>(new TestProductC()));

            factory.ClearPublic();

            Assert.Equal(0, factory.CountPublic);
            Assert.False(factory.IsRegistered("A"));
            Assert.False(factory.IsRegistered("B"));
            Assert.False(factory.IsRegistered("C"));
        }

        [Fact]
        public void RegistrationCount_ReflectsNumberOfRegistrations()
        {
            TestAsyncFactory factory = new TestAsyncFactory();

            Assert.Equal(0, factory.CountPublic);

            factory.RegisterPublic("A", ct => Task.FromResult<ITestProduct>(new TestProductA()));
            Assert.Equal(1, factory.CountPublic);

            factory.RegisterPublic("B", ct => Task.FromResult<ITestProduct>(new TestProductB()));
            Assert.Equal(2, factory.CountPublic);

            factory.UnregisterPublic("A");
            Assert.Equal(1, factory.CountPublic);

            factory.ClearPublic();
            Assert.Equal(0, factory.CountPublic);
        }

        [Fact]
        public void Constructor_WithCustomComparer_UsesCaseInsensitiveKeys()
        {
            TestAsyncFactoryWithComparer factory = new TestAsyncFactoryWithComparer();
            factory.RegisterPublic("test", ct => Task.FromResult<ITestProduct>(new TestProductA()));

            Assert.True(factory.IsRegistered("test"));
            Assert.True(factory.IsRegistered("TEST"));
            Assert.True(factory.IsRegistered("Test"));
            Assert.True(factory.IsRegistered("TeSt"));
        }

        [Fact]
        public void Constructor_WithNullComparer_ThrowsArgumentNullException()
        {
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
                () => new TestAsyncFactoryWithNullComparer());

            Assert.Equal("comparer", exception.ParamName);
        }

        [Fact]
        public async Task CreateAsync_ConcurrentCalls_AreThreadSafe()
        {
            TestAsyncFactory factory = new TestAsyncFactory();
            int creationCount = 0;

            factory.RegisterPublic("A", async ct =>
            {
                Interlocked.Increment(ref creationCount);
                await Task.Delay(10, ct);
                return new TestProductA();
            });

            Task<ITestProduct>[] tasks = new Task<ITestProduct>[100];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = factory.CreateAsync("A");
            }

            ITestProduct[] products = await Task.WhenAll(tasks);

            Assert.Equal(100, products.Length);
            Assert.Equal(100, creationCount);
            Assert.All(products, p => Assert.IsType<TestProductA>(p));
        }

        // Factory with custom comparer for testing
        private class TestAsyncFactoryWithComparer : AsyncFactoryBase<string, ITestProduct>
        {
            public TestAsyncFactoryWithComparer() : base(StringComparer.OrdinalIgnoreCase)
            {
            }

            public void RegisterPublic(string key, Func<CancellationToken, Task<ITestProduct>> creator)
            {
                Register(key, creator);
            }
        }

        // Factory with null comparer for testing
        private class TestAsyncFactoryWithNullComparer : AsyncFactoryBase<string, ITestProduct>
        {
            public TestAsyncFactoryWithNullComparer() : base(null!)
            {
            }
        }
    }
}
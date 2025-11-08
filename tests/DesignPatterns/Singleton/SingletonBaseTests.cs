using MBUtils.DesignPatterns.Singleton;

namespace MBUtils.Tests.DesignPatterns.Singleton
{
    public class SingletonBaseTests
    {
        [Fact]
        public void Instance_FirstAccess_CreatesInstance()
        {
            TestSingleton.Reset();

            TestSingleton instance = TestSingleton.Instance;

            Assert.NotNull(instance);
        }

        [Fact]
        public void Instance_MultipleAccesses_ReturnsSameInstance()
        {
            TestSingleton.Reset();

            TestSingleton instance1 = TestSingleton.Instance;
            TestSingleton instance2 = TestSingleton.Instance;

            Assert.Same(instance1, instance2);
        }

        [Fact]
        public void Instance_OnInstanceCreated_CalledOnce()
        {
            TestSingleton.Reset();

            TestSingleton instance1 = TestSingleton.Instance;
            int callCount1 = instance1.InitializationCount;

            TestSingleton instance2 = TestSingleton.Instance;
            int callCount2 = instance2.InitializationCount;

            Assert.Equal(1, callCount1);
            Assert.Equal(1, callCount2);
        }

        [Fact]
        public async Task Instance_ConcurrentAccess_CreatesSingleInstance()
        {
            TestSingleton.Reset();
            TestSingleton? instance1 = null;
            TestSingleton? instance2 = null;

            Task task1 = Task.Run(() => instance1 = TestSingleton.Instance);
            Task task2 = Task.Run(() => instance2 = TestSingleton.Instance);

            await Task.WhenAll(task1, task2);

            Assert.NotNull(instance1);
            Assert.NotNull(instance2);
            Assert.Same(instance1, instance2);
        }

        [Fact]
        public async Task Instance_ConcurrentAccessManyThreads_CreatesSingleInstance()
        {
            TestSingleton.Reset();
            const int threadCount = 100;
            TestSingleton?[] instances = new TestSingleton[threadCount];
            Task[] tasks = new Task[threadCount];

            for (int i = 0; i < threadCount; i++)
            {
                int index = i;
                tasks[i] = Task.Run(() => instances[index] = TestSingleton.Instance);
            }

            await Task.WhenAll(tasks);

            TestSingleton firstInstance = instances[0]!;
            for (int i = 1; i < threadCount; i++)
            {
                Assert.Same(firstInstance, instances[i]);
            }
        }

        [Fact]
        public void ResetInstance_AfterCreation_AllowsNewInstance()
        {
            TestSingleton.Reset();

            TestSingleton instance1 = TestSingleton.Instance;
            instance1.Value = 42;

            TestSingleton.Reset();

            TestSingleton instance2 = TestSingleton.Instance;

            Assert.NotSame(instance1, instance2);
            Assert.Equal(0, instance2.Value);
        }

        [Fact]
        public void Instance_WithPrivateConstructor_CreatesInstance()
        {
            PrivateConstructorSingleton.Reset();

            PrivateConstructorSingleton instance = PrivateConstructorSingleton.Instance;

            Assert.NotNull(instance);
        }

        [Fact]
        public void Instance_WithProtectedConstructor_CreatesInstance()
        {
            ProtectedConstructorSingleton.Reset();

            ProtectedConstructorSingleton instance = ProtectedConstructorSingleton.Instance;

            Assert.NotNull(instance);
        }

        [Fact]
        public void Instance_WithoutParameterlessConstructor_ThrowsInvalidOperationException()
        {
            NoParameterlessConstructorSingleton.Reset();

            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(
                () => NoParameterlessConstructorSingleton.Instance
            );

            Assert.Contains("does not have a parameterless constructor", ex.Message);
        }

        [Fact]
        public void Instance_ConstructorThrowsException_ThrowsInvalidOperationException()
        {
            ThrowingConstructorSingleton.Reset();

            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(
                () => ThrowingConstructorSingleton.Instance
            );

            Assert.Contains("Failed to create singleton instance", ex.Message);
            Assert.NotNull(ex.InnerException);
        }

        [Fact]
        public void OnInstanceCreated_CalledAfterConstruction_HasAccessToInstanceState()
        {
            InitializationOrderSingleton.Reset();

            InitializationOrderSingleton instance = InitializationOrderSingleton.Instance;

            Assert.True(instance.ConstructorCalled);
            Assert.True(instance.OnInstanceCreatedCalled);
            Assert.True(instance.ConstructorCalledBeforeOnInstanceCreated);
        }

        [Fact]
        public async Task Instance_ThreadSafety_InitializationCountIsOne()
        {
            CountingInitializationSingleton.Reset();
            const int threadCount = 50;
            CountingInitializationSingleton?[] instances = new CountingInitializationSingleton[threadCount];
            Task[] tasks = new Task[threadCount];

            for (int i = 0; i < threadCount; i++)
            {
                int index = i;
                tasks[i] = Task.Run(() =>
                {
                    Thread.Sleep(1); // Add small delay to increase contention
                    instances[index] = CountingInitializationSingleton.Instance;
                });
            }

            await Task.WhenAll(tasks);

            Assert.Equal(1, CountingInitializationSingleton.TotalInitializationCount);
        }
    }

    // Test helper classes

    public class TestSingleton : SingletonBase<TestSingleton>
    {
        public int InitializationCount { get; private set; }
        public int Value { get; set; }

        private TestSingleton()
        {
        }

        protected override void OnInstanceCreated()
        {
            InitializationCount++;
        }

        public static void Reset()
        {
            ResetInstance();
        }
    }

    public class PrivateConstructorSingleton : SingletonBase<PrivateConstructorSingleton>
    {
        private PrivateConstructorSingleton()
        {
        }

        public static void Reset()
        {
            ResetInstance();
        }
    }

    public class ProtectedConstructorSingleton : SingletonBase<ProtectedConstructorSingleton>
    {
        protected ProtectedConstructorSingleton()
        {
        }

        public static void Reset()
        {
            ResetInstance();
        }
    }

    public class NoParameterlessConstructorSingleton : SingletonBase<NoParameterlessConstructorSingleton>
    {
        private NoParameterlessConstructorSingleton(int value)
        {
        }

        public static void Reset()
        {
            ResetInstance();
        }
    }

    public class ThrowingConstructorSingleton : SingletonBase<ThrowingConstructorSingleton>
    {
        private ThrowingConstructorSingleton()
        {
            throw new InvalidOperationException("Constructor intentionally throws");
        }

        public static void Reset()
        {
            ResetInstance();
        }
    }

    public class InitializationOrderSingleton : SingletonBase<InitializationOrderSingleton>
    {
        public bool ConstructorCalled { get; private set; }
        public bool OnInstanceCreatedCalled { get; private set; }
        public bool ConstructorCalledBeforeOnInstanceCreated { get; private set; }

        private InitializationOrderSingleton()
        {
            ConstructorCalled = true;
        }

        protected override void OnInstanceCreated()
        {
            OnInstanceCreatedCalled = true;
            ConstructorCalledBeforeOnInstanceCreated = ConstructorCalled;
        }

        public static void Reset()
        {
            ResetInstance();
        }
    }

    public class CountingInitializationSingleton : SingletonBase<CountingInitializationSingleton>
    {
        private static int _totalCount = 0;
        public static int TotalInitializationCount => _totalCount;

        private CountingInitializationSingleton()
        {
        }

        protected override void OnInstanceCreated()
        {
            Interlocked.Increment(ref _totalCount);
        }

        public static void Reset()
        {
            ResetInstance();
            _totalCount = 0;
        }
    }
}
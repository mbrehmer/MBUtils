using MBUtils.DesignPatterns.Facade;

namespace MBUtils.Tests.DesignPatterns.Facade
{
    public class AsyncFacadeBaseTests
    {
        [Fact]
        public async Task AsyncFacadeBase_ExecuteAsync_CallsExecuteAsyncCore()
        {
            TestAsyncFacade facade = new TestAsyncFacade();
            IAsyncFacade iFacade = facade;

            await iFacade.ExecuteAsync(CancellationToken.None);

            Assert.True(facade.ExecuteCalled);
        }

        [Fact]
        public async Task AsyncFacadeBaseWithResult_ExecuteAsync_ReturnsExpectedResult()
        {
            TestAsyncFacadeWithResult facade = new TestAsyncFacadeWithResult(42);
            IAsyncFacade<int> iFacade = facade;

            int result = await iFacade.ExecuteAsync(CancellationToken.None);

            Assert.Equal(42, result);
            Assert.True(facade.ExecuteCalled);
        }

        [Fact]
        public async Task ComplexAsyncFacade_ExecuteAsync_CoordinatesSubsystems()
        {
            // Simulate an async facade coordinating multiple async subsystems
            ComplexAsyncFacade facade = new ComplexAsyncFacade();
            IAsyncFacade<string> iFacade = facade;

            string result = await iFacade.ExecuteAsync(CancellationToken.None);

            Assert.Equal("AsyncService1:OK|AsyncService2:OK|AsyncService3:OK", result);
        }

        [Fact]
        public async Task AsyncFacadeBase_RespectsCancellation()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            TestCancellableAsyncFacade facade = new TestCancellableAsyncFacade(cts.Token);
            IAsyncFacade iFacade = facade;

            cts.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await iFacade.ExecuteAsync(cts.Token);
            });
        }

        // Test implementations

        private sealed class TestAsyncFacade : AsyncFacadeBase
        {
            public bool ExecuteCalled { get; private set; }

            protected override Task ExecuteAsyncCore(CancellationToken cancellationToken)
            {
                ExecuteCalled = true;
                return Task.CompletedTask;
            }
        }

        private sealed class TestAsyncFacadeWithResult : AsyncFacadeBase<int>
        {
            private readonly int _result;
            public bool ExecuteCalled { get; private set; }

            public TestAsyncFacadeWithResult(int result)
            {
                _result = result;
            }

            protected override Task<int> ExecuteAsyncCore(CancellationToken cancellationToken)
            {
                ExecuteCalled = true;
                return Task.FromResult(_result);
            }
        }

        private sealed class ComplexAsyncFacade : AsyncFacadeBase<string>
        {
            private readonly AsyncSubsystemService1 _service1;
            private readonly AsyncSubsystemService2 _service2;
            private readonly AsyncSubsystemService3 _service3;

            public ComplexAsyncFacade()
            {
                _service1 = new AsyncSubsystemService1();
                _service2 = new AsyncSubsystemService2();
                _service3 = new AsyncSubsystemService3();
            }

            protected override async Task<string> ExecuteAsyncCore(CancellationToken cancellationToken)
            {
                string result1 = await _service1.ProcessAsync(cancellationToken);
                string result2 = await _service2.ProcessAsync(cancellationToken);
                string result3 = await _service3.ProcessAsync(cancellationToken);
                return $"{result1}|{result2}|{result3}";
            }
        }

        private sealed class TestCancellableAsyncFacade : AsyncFacadeBase
        {
            private readonly CancellationToken _internalToken;

            public TestCancellableAsyncFacade(CancellationToken internalToken)
            {
                _internalToken = internalToken;
            }

            protected override async Task ExecuteAsyncCore(CancellationToken cancellationToken)
            {
                _internalToken.ThrowIfCancellationRequested();
                await Task.Delay(100, cancellationToken);
            }
        }

        // Simulated async subsystem components

        private sealed class AsyncSubsystemService1
        {
            public async Task<string> ProcessAsync(CancellationToken cancellationToken)
            {
                await Task.Delay(10, cancellationToken);
                return "AsyncService1:OK";
            }
        }

        private sealed class AsyncSubsystemService2
        {
            public async Task<string> ProcessAsync(CancellationToken cancellationToken)
            {
                await Task.Delay(10, cancellationToken);
                return "AsyncService2:OK";
            }
        }

        private sealed class AsyncSubsystemService3
        {
            public async Task<string> ProcessAsync(CancellationToken cancellationToken)
            {
                await Task.Delay(10, cancellationToken);
                return "AsyncService3:OK";
            }
        }
    }
}
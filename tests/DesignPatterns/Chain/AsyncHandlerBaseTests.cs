using MBUtils.DesignPatterns.Chain;

namespace MBUtils.Tests.DesignPatterns.Chain
{
    public class AsyncHandlerBaseTests
    {
        [Fact]
        public async Task HandleAsync_WhenCanHandle_ReturnsTrue()
        {
            TestRequest request = new TestRequest { Value = 5 };
            SimpleAsyncHandler handler = new SimpleAsyncHandler(canHandle: true);

            bool result = await handler.HandleAsync(request);

            Assert.True(result);
            Assert.True(handler.WasHandled);
        }

        [Fact]
        public async Task HandleAsync_WhenCannotHandle_ReturnsFalse()
        {
            TestRequest request = new TestRequest { Value = 5 };
            SimpleAsyncHandler handler = new SimpleAsyncHandler(canHandle: false);

            bool result = await handler.HandleAsync(request);

            Assert.False(result);
            Assert.False(handler.WasHandled);
        }

        [Fact]
        public async Task HandleAsync_WhenCannotHandleButNextCanHandle_ReturnsTrue()
        {
            TestRequest request = new TestRequest { Value = 5 };
            SimpleAsyncHandler handler1 = new SimpleAsyncHandler(canHandle: false);
            SimpleAsyncHandler handler2 = new SimpleAsyncHandler(canHandle: true);

            handler1.SetNext(handler2);

            bool result = await handler1.HandleAsync(request);

            Assert.True(result);
            Assert.False(handler1.WasHandled);
            Assert.True(handler2.WasHandled);
        }

        [Fact]
        public async Task HandleAsync_WithChainOfThree_StopsAtFirstMatch()
        {
            TestRequest request = new TestRequest { Value = 5 };
            SimpleAsyncHandler handler1 = new SimpleAsyncHandler(canHandle: false);
            SimpleAsyncHandler handler2 = new SimpleAsyncHandler(canHandle: true);
            SimpleAsyncHandler handler3 = new SimpleAsyncHandler(canHandle: true);

            handler1.SetNext(handler2);
            handler2.SetNext(handler3);

            bool result = await handler1.HandleAsync(request);

            Assert.True(result);
            Assert.False(handler1.WasHandled);
            Assert.True(handler2.WasHandled);
            Assert.False(handler3.WasHandled);
        }

        [Fact]
        public async Task HandleAsync_WithNoMatch_ReturnsFalse()
        {
            TestRequest request = new TestRequest { Value = 5 };
            SimpleAsyncHandler handler1 = new SimpleAsyncHandler(canHandle: false);
            SimpleAsyncHandler handler2 = new SimpleAsyncHandler(canHandle: false);

            handler1.SetNext(handler2);

            bool result = await handler1.HandleAsync(request);

            Assert.False(result);
            Assert.False(handler1.WasHandled);
            Assert.False(handler2.WasHandled);
        }

        [Fact]
        public void SetNext_WithNullHandler_ThrowsArgumentNullException()
        {
            SimpleAsyncHandler handler = new SimpleAsyncHandler(canHandle: true);

            Assert.Throws<ArgumentNullException>(() => handler.SetNext(null!));
        }

        [Fact]
        public void SetNext_ReturnsNextHandler()
        {
            SimpleAsyncHandler handler1 = new SimpleAsyncHandler(canHandle: true);
            SimpleAsyncHandler handler2 = new SimpleAsyncHandler(canHandle: true);

            IAsyncHandler<TestRequest> result = handler1.SetNext(handler2);

            Assert.Same(handler2, result);
        }

        [Fact]
        public async Task HandleAsync_WithCancellationToken_PropagatesToken()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            TestRequest request = new TestRequest { Value = 5 };
            CancellationTokenCapturingHandler handler = new CancellationTokenCapturingHandler();

            await handler.HandleAsync(request, cts.Token);

            Assert.Equal(cts.Token, handler.CapturedToken);
        }

        [Fact]
        public async Task HandleAsync_WithCanceledToken_ThrowsOperationCanceledException()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.Cancel();
            TestRequest request = new TestRequest { Value = 5 };
            CancellationSensitiveHandler handler = new CancellationSensitiveHandler();

            await Assert.ThrowsAsync<OperationCanceledException>(
                async () => await handler.HandleAsync(request, cts.Token));
        }

        [Fact]
        public async Task HandleAsync_WithDelayedHandler_WorksCorrectly()
        {
            TestRequest request = new TestRequest { Value = 5 };
            DelayedHandler handler = new DelayedHandler(delayMilliseconds: 50);

            bool result = await handler.HandleAsync(request);

            Assert.True(result);
            Assert.True(handler.WasHandled);
        }

        // Test helper classes
        private sealed class TestRequest
        {
            public int Value { get; set; }
        }

        private sealed class SimpleAsyncHandler : AsyncHandlerBase<TestRequest>
        {
            private readonly bool _canHandle;

            public bool WasHandled { get; private set; }

            public SimpleAsyncHandler(bool canHandle)
            {
                _canHandle = canHandle;
            }

            protected override Task<bool> CanHandleAsync(TestRequest request, CancellationToken cancellationToken)
            {
                return Task.FromResult(_canHandle);
            }

            protected override Task HandleCoreAsync(TestRequest request, CancellationToken cancellationToken)
            {
                WasHandled = true;
                return Task.CompletedTask;
            }
        }

        private sealed class CancellationTokenCapturingHandler : AsyncHandlerBase<TestRequest>
        {
            public CancellationToken CapturedToken { get; private set; }

            protected override Task<bool> CanHandleAsync(TestRequest request, CancellationToken cancellationToken)
            {
                CapturedToken = cancellationToken;
                return Task.FromResult(true);
            }

            protected override Task HandleCoreAsync(TestRequest request, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }

        private sealed class CancellationSensitiveHandler : AsyncHandlerBase<TestRequest>
        {
            protected override Task<bool> CanHandleAsync(TestRequest request, CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Task.FromResult(true);
            }

            protected override Task HandleCoreAsync(TestRequest request, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }

        private sealed class DelayedHandler : AsyncHandlerBase<TestRequest>
        {
            private readonly int _delayMilliseconds;

            public bool WasHandled { get; private set; }

            public DelayedHandler(int delayMilliseconds)
            {
                _delayMilliseconds = delayMilliseconds;
            }

            protected override Task<bool> CanHandleAsync(TestRequest request, CancellationToken cancellationToken)
            {
                return Task.FromResult(true);
            }

            protected override async Task HandleCoreAsync(TestRequest request, CancellationToken cancellationToken)
            {
                await Task.Delay(_delayMilliseconds, cancellationToken);
                WasHandled = true;
            }
        }
    }
}
using MBUtils.DesignPatterns.Chain;

namespace MBUtils.Tests.DesignPatterns.Chain
{
    public class AsyncHandlerChainTests
    {
        [Fact]
        public void Build_WithNoHandlers_ReturnsNull()
        {
            AsyncHandlerChain<int> chain = new AsyncHandlerChain<int>();

            IAsyncHandler<int>? result = chain.Build();

            Assert.Null(result);
        }

        [Fact]
        public void Build_WithOneHandler_ReturnsThatHandler()
        {
            TestAsyncHandler handler = new TestAsyncHandler(canHandle: true);
            AsyncHandlerChain<TestRequest> chain = new AsyncHandlerChain<TestRequest>();

            chain.Add(handler);
            IAsyncHandler<TestRequest>? result = chain.Build();

            Assert.Same(handler, result);
        }

        [Fact]
        public async Task Build_WithMultipleHandlers_ChainsThemCorrectly()
        {
            TestAsyncHandler handler1 = new TestAsyncHandler(canHandle: false);
            TestAsyncHandler handler2 = new TestAsyncHandler(canHandle: false);
            TestAsyncHandler handler3 = new TestAsyncHandler(canHandle: true);

            IAsyncHandler<TestRequest>? chain = new AsyncHandlerChain<TestRequest>()
                .Add(handler1)
                .Add(handler2)
                .Add(handler3)
                .Build();

            TestRequest request = new TestRequest();
            bool result = await chain!.HandleAsync(request);

            Assert.True(result);
            Assert.False(handler1.WasHandled);
            Assert.False(handler2.WasHandled);
            Assert.True(handler3.WasHandled);
        }

        [Fact]
        public void Add_WithNullHandler_ThrowsArgumentNullException()
        {
            AsyncHandlerChain<TestRequest> chain = new AsyncHandlerChain<TestRequest>();

            Assert.Throws<ArgumentNullException>(() => chain.Add(null!));
        }

        [Fact]
        public void Add_ReturnsChainForFluentInterface()
        {
            AsyncHandlerChain<TestRequest> chain = new AsyncHandlerChain<TestRequest>();
            TestAsyncHandler handler = new TestAsyncHandler(canHandle: true);

            AsyncHandlerChain<TestRequest> result = chain.Add(handler);

            Assert.Same(chain, result);
        }

        [Fact]
        public async Task Build_WithFiveHandlers_WorksCorrectly()
        {
            TestAsyncHandler handler1 = new TestAsyncHandler(canHandle: false);
            TestAsyncHandler handler2 = new TestAsyncHandler(canHandle: false);
            TestAsyncHandler handler3 = new TestAsyncHandler(canHandle: false);
            TestAsyncHandler handler4 = new TestAsyncHandler(canHandle: true);
            TestAsyncHandler handler5 = new TestAsyncHandler(canHandle: true);

            IAsyncHandler<TestRequest>? chain = new AsyncHandlerChain<TestRequest>()
                .Add(handler1)
                .Add(handler2)
                .Add(handler3)
                .Add(handler4)
                .Add(handler5)
                .Build();

            TestRequest request = new TestRequest();
            bool result = await chain!.HandleAsync(request);

            Assert.True(result);
            Assert.False(handler1.WasHandled);
            Assert.False(handler2.WasHandled);
            Assert.False(handler3.WasHandled);
            Assert.True(handler4.WasHandled);
            Assert.False(handler5.WasHandled);
        }

        // Test helper classes
        private sealed class TestRequest
        {
        }

        private sealed class TestAsyncHandler : AsyncHandlerBase<TestRequest>
        {
            private readonly bool _canHandle;

            public bool WasHandled { get; private set; }

            public TestAsyncHandler(bool canHandle)
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
    }
}
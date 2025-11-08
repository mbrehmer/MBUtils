using MBUtils.DesignPatterns.Chain;

namespace MBUtils.Tests.DesignPatterns.Chain
{
    public class HandlerChainTests
    {
        [Fact]
        public void Build_WithNoHandlers_ReturnsNull()
        {
            HandlerChain<int> chain = new HandlerChain<int>();

            IHandler<int>? result = chain.Build();

            Assert.Null(result);
        }

        [Fact]
        public void Build_WithOneHandler_ReturnsThatHandler()
        {
            TestHandler handler = new TestHandler(canHandle: true);
            HandlerChain<TestRequest> chain = new HandlerChain<TestRequest>();

            chain.Add(handler);
            IHandler<TestRequest>? result = chain.Build();

            Assert.Same(handler, result);
        }

        [Fact]
        public void Build_WithMultipleHandlers_ChainsThemCorrectly()
        {
            TestHandler handler1 = new TestHandler(canHandle: false);
            TestHandler handler2 = new TestHandler(canHandle: false);
            TestHandler handler3 = new TestHandler(canHandle: true);

            IHandler<TestRequest>? chain = new HandlerChain<TestRequest>()
                .Add(handler1)
                .Add(handler2)
                .Add(handler3)
                .Build();

            TestRequest request = new TestRequest();
            bool result = chain!.Handle(request);

            Assert.True(result);
            Assert.False(handler1.WasHandled);
            Assert.False(handler2.WasHandled);
            Assert.True(handler3.WasHandled);
        }

        [Fact]
        public void Add_WithNullHandler_ThrowsArgumentNullException()
        {
            HandlerChain<TestRequest> chain = new HandlerChain<TestRequest>();

            Assert.Throws<ArgumentNullException>(() => chain.Add(null!));
        }

        [Fact]
        public void Add_ReturnsChainForFluentInterface()
        {
            HandlerChain<TestRequest> chain = new HandlerChain<TestRequest>();
            TestHandler handler = new TestHandler(canHandle: true);

            HandlerChain<TestRequest> result = chain.Add(handler);

            Assert.Same(chain, result);
        }

        [Fact]
        public void Build_WithFiveHandlers_WorksCorrectly()
        {
            TestHandler handler1 = new TestHandler(canHandle: false);
            TestHandler handler2 = new TestHandler(canHandle: false);
            TestHandler handler3 = new TestHandler(canHandle: false);
            TestHandler handler4 = new TestHandler(canHandle: true);
            TestHandler handler5 = new TestHandler(canHandle: true);

            IHandler<TestRequest>? chain = new HandlerChain<TestRequest>()
                .Add(handler1)
                .Add(handler2)
                .Add(handler3)
                .Add(handler4)
                .Add(handler5)
                .Build();

            TestRequest request = new TestRequest();
            bool result = chain!.Handle(request);

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

        private sealed class TestHandler : HandlerBase<TestRequest>
        {
            private readonly bool _canHandle;

            public bool WasHandled { get; private set; }

            public TestHandler(bool canHandle)
            {
                _canHandle = canHandle;
            }

            protected override bool CanHandle(TestRequest request)
            {
                return _canHandle;
            }

            protected override void HandleCore(TestRequest request)
            {
                WasHandled = true;
            }
        }
    }
}
using MBUtils.DesignPatterns.Chain;

namespace MBUtils.Tests.DesignPatterns.Chain
{
    public class HandlerBaseTests
    {
        [Fact]
        public void Handle_WhenCanHandle_ReturnsTrue()
        {
            TestRequest request = new TestRequest { Value = 5 };
            SimpleHandler handler = new SimpleHandler(canHandle: true);

            bool result = handler.Handle(request);

            Assert.True(result);
            Assert.True(handler.WasHandled);
        }

        [Fact]
        public void Handle_WhenCannotHandle_ReturnsFalse()
        {
            TestRequest request = new TestRequest { Value = 5 };
            SimpleHandler handler = new SimpleHandler(canHandle: false);

            bool result = handler.Handle(request);

            Assert.False(result);
            Assert.False(handler.WasHandled);
        }

        [Fact]
        public void Handle_WhenCannotHandleButNextCanHandle_ReturnsTrue()
        {
            TestRequest request = new TestRequest { Value = 5 };
            SimpleHandler handler1 = new SimpleHandler(canHandle: false);
            SimpleHandler handler2 = new SimpleHandler(canHandle: true);

            handler1.SetNext(handler2);

            bool result = handler1.Handle(request);

            Assert.True(result);
            Assert.False(handler1.WasHandled);
            Assert.True(handler2.WasHandled);
        }

        [Fact]
        public void Handle_WithChainOfThree_StopsAtFirstMatch()
        {
            TestRequest request = new TestRequest { Value = 5 };
            SimpleHandler handler1 = new SimpleHandler(canHandle: false);
            SimpleHandler handler2 = new SimpleHandler(canHandle: true);
            SimpleHandler handler3 = new SimpleHandler(canHandle: true);

            handler1.SetNext(handler2);
            handler2.SetNext(handler3);

            bool result = handler1.Handle(request);

            Assert.True(result);
            Assert.False(handler1.WasHandled);
            Assert.True(handler2.WasHandled);
            Assert.False(handler3.WasHandled);
        }

        [Fact]
        public void Handle_WithNoMatch_ReturnsFalse()
        {
            TestRequest request = new TestRequest { Value = 5 };
            SimpleHandler handler1 = new SimpleHandler(canHandle: false);
            SimpleHandler handler2 = new SimpleHandler(canHandle: false);

            handler1.SetNext(handler2);

            bool result = handler1.Handle(request);

            Assert.False(result);
            Assert.False(handler1.WasHandled);
            Assert.False(handler2.WasHandled);
        }

        [Fact]
        public void SetNext_WithNullHandler_ThrowsArgumentNullException()
        {
            SimpleHandler handler = new SimpleHandler(canHandle: true);

            Assert.Throws<ArgumentNullException>(() => handler.SetNext(null!));
        }

        [Fact]
        public void SetNext_ReturnsNextHandler()
        {
            SimpleHandler handler1 = new SimpleHandler(canHandle: true);
            SimpleHandler handler2 = new SimpleHandler(canHandle: true);

            IHandler<TestRequest> result = handler1.SetNext(handler2);

            Assert.Same(handler2, result);
        }

        [Fact]
        public void Handle_WithValueTypeRequest_WorksCorrectly()
        {
            IntHandler handler = new IntHandler(threshold: 10);

            bool result1 = handler.Handle(5);
            bool result2 = handler.Handle(15);

            Assert.True(result1);
            Assert.False(result2);
        }

        // Test helper classes
        private sealed class TestRequest
        {
            public int Value { get; set; }
        }

        private sealed class SimpleHandler : HandlerBase<TestRequest>
        {
            private readonly bool _canHandle;

            public bool WasHandled { get; private set; }

            public SimpleHandler(bool canHandle)
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

        private sealed class IntHandler : HandlerBase<int>
        {
            private readonly int _threshold;

            public IntHandler(int threshold)
            {
                _threshold = threshold;
            }

            protected override bool CanHandle(int request)
            {
                return request < _threshold;
            }

            protected override void HandleCore(int request)
            {
                // Nothing to do
            }
        }
    }
}
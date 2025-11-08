using MBUtils.DesignPatterns.Decorator;

namespace MBUtils.Tests.DesignPatterns.Decorator
{
    public class DecoratorTests
    {
        [Fact]
        public void DecoratorBase_NullComponent_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new TestDecorator(null!));
        }

        [Fact]
        public void AsyncDecoratorBase_NullComponent_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new TestAsyncDecorator(null!));
        }

        [Fact]
        public void Decorator_Execute_ReturnsDecoratedValue()
        {
            IDecorator<string> decorator = new PrefixDecorator("World", "Hello ");
            string result = decorator.Execute();
            Assert.Equal("Hello World", result);
        }

        [Fact]
        public void Decorator_Component_ReturnsOriginalComponent()
        {
            string component = "Test";
            IDecorator<string> decorator = new PrefixDecorator(component, "Prefix");
            Assert.Equal(component, decorator.Component);
        }

        [Fact]
        public async Task AsyncDecorator_ExecuteAsync_ReturnsDecoratedValue()
        {
            IAsyncDecorator<string> decorator = new AsyncPrefixDecorator("World", "Hello ");
            string result = await decorator.ExecuteAsync(CancellationToken.None);
            Assert.Equal("Hello World", result);
        }

        [Fact]
        public void AsyncDecorator_Component_ReturnsOriginalComponent()
        {
            string component = "Test";
            IAsyncDecorator<string> decorator = new AsyncPrefixDecorator(component, "Prefix");
            Assert.Equal(component, decorator.Component);
        }

        [Fact]
        public void CompositeDecorator_ChainsSyncDecorators_AppliesAllInOrder()
        {
            List<Func<string, IDecorator<string>>> factories = new List<Func<string, IDecorator<string>>>
            {
                component => new PrefixDecorator(component, "["),
                component => new SuffixDecorator(component, "]")
            };

            IDecorator<string> composite = new CompositeDecorator<string>("Hello", factories.AsReadOnly());
            string result = composite.Execute();
            Assert.Equal("[Hello]", result);
        }

        [Fact]
        public async Task CompositeDecorator_ChainsAsyncDecorators_AppliesAllInOrder()
        {
            List<Func<string, IAsyncDecorator<string>>> factories = new List<Func<string, IAsyncDecorator<string>>>
            {
                component => new AsyncPrefixDecorator(component, "["),
                component => new AsyncSuffixDecorator(component, "]")
            };

            IAsyncDecorator<string> composite = new CompositeDecorator<string>("Hello", factories.AsReadOnly());
            string result = await composite.ExecuteAsync(CancellationToken.None);
            Assert.Equal("[Hello]", result);
        }

        [Fact]
        public void CompositeDecorator_NullComponent_ThrowsArgumentNullException()
        {
            List<Func<string, IDecorator<string>>> factories = new List<Func<string, IDecorator<string>>>
            {
                component => new PrefixDecorator(component, "Test")
            };

            Assert.Throws<ArgumentNullException>(() => new CompositeDecorator<string>(null!, factories.AsReadOnly()));
        }

        [Fact]
        public void CompositeDecorator_NullFactories_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CompositeDecorator<string>("Test", (IReadOnlyList<Func<string, IDecorator<string>>>)null!));
        }

        [Fact]
        public void CompositeDecorator_EmptyFactories_ThrowsArgumentException()
        {
            List<Func<string, IDecorator<string>>> emptyFactories = new List<Func<string, IDecorator<string>>>();
            Assert.Throws<ArgumentException>(() => new CompositeDecorator<string>("Test", emptyFactories.AsReadOnly()));
        }

        [Fact]
        public void CompositeDecorator_ExecuteOnAsyncComposite_ThrowsInvalidOperationException()
        {
            List<Func<string, IAsyncDecorator<string>>> asyncFactories = new List<Func<string, IAsyncDecorator<string>>>
            {
                component => new AsyncPrefixDecorator(component, "Test")
            };

            IDecorator<string> composite = new CompositeDecorator<string>("Hello", asyncFactories.AsReadOnly());
            Assert.Throws<InvalidOperationException>(() => composite.Execute());
        }

        [Fact]
        public async Task CompositeDecorator_ExecuteAsyncOnSyncComposite_ThrowsInvalidOperationException()
        {
            List<Func<string, IDecorator<string>>> syncFactories = new List<Func<string, IDecorator<string>>>
            {
                component => new PrefixDecorator(component, "Test")
            };

            IAsyncDecorator<string> composite = new CompositeDecorator<string>("Hello", syncFactories.AsReadOnly());
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await composite.ExecuteAsync(CancellationToken.None));
        }

        [Fact]
        public void CompositeDecorator_MultipleDecorators_AppliesInCorrectOrder()
        {
            List<Func<int, IDecorator<int>>> factories = new List<Func<int, IDecorator<int>>>
            {
                component => new MultiplyDecorator(component, 2),
                component => new AddDecorator(component, 10)
            };

            IDecorator<int> composite = new CompositeDecorator<int>(5, factories.AsReadOnly());
            int result = composite.Execute();
            // (5 * 2) = 10, then 10 + 10 = 20
            Assert.Equal(20, result);
        }

        [Fact]
        public async Task AsyncDecorator_Cancellation_PropagatesCancellation()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.Cancel();

            IAsyncDecorator<string> decorator = new CancellableAsyncDecorator("Test");
            await Assert.ThrowsAsync<TaskCanceledException>(async () => await decorator.ExecuteAsync(cts.Token));
        }

        // Test helper decorators

        private sealed class TestDecorator : DecoratorBase<string>
        {
            public TestDecorator(string component) : base(component)
            {
            }

            protected override string ExecuteCore()
            {
                return Component;
            }
        }

        private sealed class TestAsyncDecorator : AsyncDecoratorBase<string>
        {
            public TestAsyncDecorator(string component) : base(component)
            {
            }

            protected override Task<string> ExecuteAsyncCore(CancellationToken cancellationToken)
            {
                return Task.FromResult(Component);
            }
        }

        private sealed class PrefixDecorator : DecoratorBase<string>
        {
            private readonly string _prefix;

            public PrefixDecorator(string component, string prefix) : base(component)
            {
                _prefix = prefix;
            }

            protected override string ExecuteCore()
            {
                return _prefix + Component;
            }
        }

        private sealed class SuffixDecorator : DecoratorBase<string>
        {
            private readonly string _suffix;

            public SuffixDecorator(string component, string suffix) : base(component)
            {
                _suffix = suffix;
            }

            protected override string ExecuteCore()
            {
                return Component + _suffix;
            }
        }

        private sealed class AsyncPrefixDecorator : AsyncDecoratorBase<string>
        {
            private readonly string _prefix;

            public AsyncPrefixDecorator(string component, string prefix) : base(component)
            {
                _prefix = prefix;
            }

            protected override async Task<string> ExecuteAsyncCore(CancellationToken cancellationToken)
            {
                await Task.Delay(1, cancellationToken).ConfigureAwait(false);
                return _prefix + Component;
            }
        }

        private sealed class AsyncSuffixDecorator : AsyncDecoratorBase<string>
        {
            private readonly string _suffix;

            public AsyncSuffixDecorator(string component, string suffix) : base(component)
            {
                _suffix = suffix;
            }

            protected override async Task<string> ExecuteAsyncCore(CancellationToken cancellationToken)
            {
                await Task.Delay(1, cancellationToken).ConfigureAwait(false);
                return Component + _suffix;
            }
        }

        private sealed class MultiplyDecorator : DecoratorBase<int>
        {
            private readonly int _multiplier;

            public MultiplyDecorator(int component, int multiplier) : base(component)
            {
                _multiplier = multiplier;
            }

            protected override int ExecuteCore()
            {
                return Component * _multiplier;
            }
        }

        private sealed class AddDecorator : DecoratorBase<int>
        {
            private readonly int _addend;

            public AddDecorator(int component, int addend) : base(component)
            {
                _addend = addend;
            }

            protected override int ExecuteCore()
            {
                return Component + _addend;
            }
        }

        private sealed class CancellableAsyncDecorator : AsyncDecoratorBase<string>
        {
            public CancellableAsyncDecorator(string component) : base(component)
            {
            }

            protected override async Task<string> ExecuteAsyncCore(CancellationToken cancellationToken)
            {
                await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
                return Component;
            }
        }
    }
}
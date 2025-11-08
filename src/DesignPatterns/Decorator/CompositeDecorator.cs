using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MBUtils.DesignPatterns.Decorator
{
    /// <summary>
    /// Executes a chain of decorators in order, where each decorator wraps the result of the previous one.
    /// The decorators are applied from the innermost (first in the list) to the outermost (last in the list).
    /// </summary>
    /// <typeparam name="T">The type of the component being decorated.</typeparam>
    public sealed class CompositeDecorator<T> : IDecorator<T>, IAsyncDecorator<T>
    {
        private readonly T _component;
        private readonly IReadOnlyList<Func<T, IDecorator<T>>> _decoratorFactories;
        private readonly IReadOnlyList<Func<T, IAsyncDecorator<T>>> _asyncDecoratorFactories;

        /// <summary>
        /// Initializes a new instance of <see cref="CompositeDecorator{T}"/> with synchronous decorators.
        /// </summary>
        /// <param name="component">The initial component to decorate.</param>
        /// <param name="decoratorFactories">Functions that create decorators. Each factory receives the component from the previous layer.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="component"/> or <paramref name="decoratorFactories"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="decoratorFactories"/> is empty.</exception>
        public CompositeDecorator(T component, IReadOnlyList<Func<T, IDecorator<T>>> decoratorFactories)
        {
            _component = component ?? throw new ArgumentNullException(nameof(component));
            _decoratorFactories = decoratorFactories ?? throw new ArgumentNullException(nameof(decoratorFactories));
            _asyncDecoratorFactories = Array.Empty<Func<T, IAsyncDecorator<T>>>();

            if (_decoratorFactories.Count == 0)
            {
                throw new ArgumentException("At least one decorator factory must be provided.", nameof(decoratorFactories));
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="CompositeDecorator{T}"/> with asynchronous decorators.
        /// </summary>
        /// <param name="component">The initial component to decorate.</param>
        /// <param name="asyncDecoratorFactories">Functions that create async decorators. Each factory receives the component from the previous layer.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="component"/> or <paramref name="asyncDecoratorFactories"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="asyncDecoratorFactories"/> is empty.</exception>
        public CompositeDecorator(T component, IReadOnlyList<Func<T, IAsyncDecorator<T>>> asyncDecoratorFactories)
        {
            _component = component ?? throw new ArgumentNullException(nameof(component));
            _asyncDecoratorFactories = asyncDecoratorFactories ?? throw new ArgumentNullException(nameof(asyncDecoratorFactories));
            _decoratorFactories = Array.Empty<Func<T, IDecorator<T>>>();

            if (_asyncDecoratorFactories.Count == 0)
            {
                throw new ArgumentException("At least one async decorator factory must be provided.", nameof(asyncDecoratorFactories));
            }
        }

        /// <inheritdoc />
        T IDecorator<T>.Component => _component;

        /// <inheritdoc />
        T IAsyncDecorator<T>.Component => _component;

        /// <inheritdoc />
        T IDecorator<T>.Execute()
        {
            if (_decoratorFactories.Count == 0)
            {
                throw new InvalidOperationException("CompositeDecorator was initialized with async decorators. Use ExecuteAsync instead.");
            }

            T current = _component;

            foreach (Func<T, IDecorator<T>> factory in _decoratorFactories)
            {
                IDecorator<T> decorator = factory(current);
                current = decorator.Execute();
            }

            return current;
        }

        /// <inheritdoc />
        async Task<T> IAsyncDecorator<T>.ExecuteAsync(CancellationToken cancellationToken)
        {
            if (_asyncDecoratorFactories.Count == 0)
            {
                throw new InvalidOperationException("CompositeDecorator was initialized with synchronous decorators. Use Execute instead.");
            }

            T current = _component;

            foreach (Func<T, IAsyncDecorator<T>> factory in _asyncDecoratorFactories)
            {
                IAsyncDecorator<T> decorator = factory(current);
                current = await decorator.ExecuteAsync(cancellationToken).ConfigureAwait(false);
            }

            return current;
        }
    }
}
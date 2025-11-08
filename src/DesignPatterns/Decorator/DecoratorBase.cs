using System;

namespace MBUtils.DesignPatterns.Decorator
{
    /// <summary>
    /// Base class easing implementation of <see cref="IDecorator{T}"/>.
    /// Implements the public interface explicitly and exposes protected core methods for concrete types.
    /// </summary>
    /// <typeparam name="T">The type of the component being decorated.</typeparam>
    public abstract class DecoratorBase<T> : IDecorator<T>
    {
        /// <summary>
        /// The wrapped component.
        /// </summary>
        private readonly T _component;

        /// <summary>
        /// Initializes a new instance of the <see cref="DecoratorBase{T}"/> class.
        /// </summary>
        /// <param name="component">The component to decorate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="component"/> is null.</exception>
        protected DecoratorBase(T component)
        {
            _component = component ?? throw new ArgumentNullException(nameof(component));
        }

        /// <summary>
        /// Gets the wrapped component.
        /// </summary>
        protected T Component => _component;

        /// <summary>
        /// Executes the decorator's core implementation.
        /// Override this method to add behavior before, after, or around the component.
        /// </summary>
        /// <returns>The decorated result.</returns>
        protected abstract T ExecuteCore();

        /// <inheritdoc />
        T IDecorator<T>.Component => _component;

        /// <inheritdoc />
        T IDecorator<T>.Execute()
        {
            return ExecuteCore();
        }
    }
}
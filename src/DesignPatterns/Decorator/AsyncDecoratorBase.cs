using System;
using System.Threading;
using System.Threading.Tasks;

namespace MBUtils.DesignPatterns.Decorator
{
    /// <summary>
    /// Base class easing implementation of <see cref="IAsyncDecorator{T}"/>.
    /// Implements the public interface explicitly and exposes protected core methods for concrete types.
    /// </summary>
    /// <typeparam name="T">The type of the component being decorated.</typeparam>
    public abstract class AsyncDecoratorBase<T> : IAsyncDecorator<T>
    {
        /// <summary>
        /// The wrapped component.
        /// </summary>
        private readonly T _component;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncDecoratorBase{T}"/> class.
        /// </summary>
        /// <param name="component">The component to decorate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="component"/> is null.</exception>
        protected AsyncDecoratorBase(T component)
        {
            _component = component ?? throw new ArgumentNullException(nameof(component));
        }

        /// <summary>
        /// Gets the wrapped component.
        /// </summary>
        protected T Component => _component;

        /// <summary>
        /// Executes the decorator's core implementation asynchronously.
        /// Override this method to add behavior before, after, or around the component.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task that yields the decorated result.</returns>
        protected abstract Task<T> ExecuteAsyncCore(CancellationToken cancellationToken);

        /// <inheritdoc />
        T IAsyncDecorator<T>.Component => _component;

        /// <inheritdoc />
        Task<T> IAsyncDecorator<T>.ExecuteAsync(CancellationToken cancellationToken)
        {
            return ExecuteAsyncCore(cancellationToken);
        }
    }
}
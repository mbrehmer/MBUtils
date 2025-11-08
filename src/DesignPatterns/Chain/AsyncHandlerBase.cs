using System;
using System.Threading;
using System.Threading.Tasks;

namespace MBUtils.DesignPatterns.Chain
{
    /// <summary>
    /// Provides an abstract base implementation of the asynchronous Chain of Responsibility pattern.
    /// </summary>
    /// <remarks>
    /// This abstract class implements the <see cref="IAsyncHandler{TRequest}"/> interface
    /// and provides a common foundation for concrete asynchronous handler implementations.
    /// Derived classes must implement the <see cref="CanHandleAsync"/> and <see cref="HandleCoreAsync"/>
    /// methods to define their handling logic.
    /// The chain of responsibility allows passing requests along a chain of handlers,
    /// where each handler decides either to process the request or to pass it to the next handler.
    /// </remarks>
    /// <typeparam name="TRequest">The type of request to handle.</typeparam>
    public abstract class AsyncHandlerBase<TRequest> : IAsyncHandler<TRequest>
    {
        private IAsyncHandler<TRequest>? _next;

        /// <summary>
        /// Sets the next handler in the chain.
        /// </summary>
        /// <param name="next">The next handler to invoke if this handler cannot process the request.</param>
        /// <returns>The next handler, allowing for fluent chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="next"/> is null.</exception>
        public IAsyncHandler<TRequest> SetNext(IAsyncHandler<TRequest> next)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            _next = next;
            return next;
        }

        /// <summary>
        /// Asynchronously handles the request or passes it to the next handler in the chain.
        /// </summary>
        /// <param name="request">The request to handle.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result is <c>true</c> if the request was handled by this handler or a subsequent handler in the chain;
        /// <c>false</c> if no handler in the chain could process the request.
        /// </returns>
        public async Task<bool> HandleAsync(TRequest request, CancellationToken cancellationToken = default)
        {
            if (await CanHandleAsync(request, cancellationToken).ConfigureAwait(false))
            {
                await HandleCoreAsync(request, cancellationToken).ConfigureAwait(false);
                return true;
            }

            if (_next != null)
            {
                return await _next.HandleAsync(request, cancellationToken).ConfigureAwait(false);
            }

            return false;
        }

        /// <summary>
        /// Asynchronously determines whether this handler can process the specified request.
        /// </summary>
        /// <param name="request">The request to evaluate.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// The task result is <c>true</c> if this handler can process the request; otherwise, <c>false</c>.
        /// </returns>
        protected abstract Task<bool> CanHandleAsync(TRequest request, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously processes the request.
        /// This method is only called when <see cref="CanHandleAsync"/> returns <c>true</c>.
        /// </summary>
        /// <param name="request">The request to process.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected abstract Task HandleCoreAsync(TRequest request, CancellationToken cancellationToken);
    }
}
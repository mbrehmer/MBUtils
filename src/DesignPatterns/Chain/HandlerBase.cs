using System;

namespace MBUtils.DesignPatterns.Chain
{
    /// <summary>
    /// Provides an abstract base implementation of the Chain of Responsibility pattern.
    /// </summary>
    /// <remarks>
    /// This abstract class implements the <see cref="IHandler{TRequest}"/> interface
    /// and provides a common foundation for concrete handler implementations.
    /// Derived classes must implement the <see cref="CanHandle"/> and <see cref="HandleCore"/>
    /// methods to define their handling logic.
    /// The chain of responsibility allows passing requests along a chain of handlers,
    /// where each handler decides either to process the request or to pass it to the next handler.
    /// </remarks>
    /// <typeparam name="TRequest">The type of request to handle.</typeparam>
    public abstract class HandlerBase<TRequest> : IHandler<TRequest>
    {
        private IHandler<TRequest>? _next;

        /// <summary>
        /// Sets the next handler in the chain.
        /// </summary>
        /// <param name="next">The next handler to invoke if this handler cannot process the request.</param>
        /// <returns>The next handler, allowing for fluent chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="next"/> is null.</exception>
        public IHandler<TRequest> SetNext(IHandler<TRequest> next)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            _next = next;
            return next;
        }

        /// <summary>
        /// Handles the request or passes it to the next handler in the chain.
        /// </summary>
        /// <param name="request">The request to handle.</param>
        /// <returns>
        /// <c>true</c> if the request was handled by this handler or a subsequent handler in the chain;
        /// <c>false</c> if no handler in the chain could process the request.
        /// </returns>
        public bool Handle(TRequest request)
        {
            if (CanHandle(request))
            {
                HandleCore(request);
                return true;
            }

            if (_next != null)
            {
                return _next.Handle(request);
            }

            return false;
        }

        /// <summary>
        /// Determines whether this handler can process the specified request.
        /// </summary>
        /// <param name="request">The request to evaluate.</param>
        /// <returns>
        /// <c>true</c> if this handler can process the request; otherwise, <c>false</c>.
        /// </returns>
        protected abstract bool CanHandle(TRequest request);

        /// <summary>
        /// Processes the request.
        /// This method is only called when <see cref="CanHandle"/> returns <c>true</c>.
        /// </summary>
        /// <param name="request">The request to process.</param>
        protected abstract void HandleCore(TRequest request);
    }
}
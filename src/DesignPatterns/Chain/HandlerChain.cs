using System;
using System.Collections.Generic;

namespace MBUtils.DesignPatterns.Chain
{
    /// <summary>
    /// Provides a fluent builder for constructing chains of handlers.
    /// </summary>
    /// <typeparam name="TRequest">The type of request to handle.</typeparam>
    public sealed class HandlerChain<TRequest>
    {
        private readonly List<IHandler<TRequest>> _handlers = new List<IHandler<TRequest>>();

        /// <summary>
        /// Adds a handler to the end of the chain.
        /// </summary>
        /// <param name="handler">The handler to add to the chain.</param>
        /// <returns>This <see cref="HandlerChain{TRequest}"/> instance for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
        public HandlerChain<TRequest> Add(IHandler<TRequest> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            _handlers.Add(handler);
            return this;
        }

        /// <summary>
        /// Builds the chain by linking all handlers together.
        /// </summary>
        /// <returns>The first handler in the chain, or null if no handlers were added.</returns>
        public IHandler<TRequest>? Build()
        {
            if (_handlers.Count == 0)
            {
                return null;
            }

            for (int i = 0; i < _handlers.Count - 1; i++)
            {
                _handlers[i].SetNext(_handlers[i + 1]);
            }

            return _handlers[0];
        }
    }

    /// <summary>
    /// Provides a fluent builder for constructing chains of asynchronous handlers.
    /// </summary>
    /// <typeparam name="TRequest">The type of request to handle.</typeparam>
    public sealed class AsyncHandlerChain<TRequest>
    {
        private readonly List<IAsyncHandler<TRequest>> _handlers = new List<IAsyncHandler<TRequest>>();

        /// <summary>
        /// Adds a handler to the end of the chain.
        /// </summary>
        /// <param name="handler">The handler to add to the chain.</param>
        /// <returns>This <see cref="AsyncHandlerChain{TRequest}"/> instance for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
        public AsyncHandlerChain<TRequest> Add(IAsyncHandler<TRequest> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            _handlers.Add(handler);
            return this;
        }

        /// <summary>
        /// Builds the chain by linking all handlers together.
        /// </summary>
        /// <returns>The first handler in the chain, or null if no handlers were added.</returns>
        public IAsyncHandler<TRequest>? Build()
        {
            if (_handlers.Count == 0)
            {
                return null;
            }

            for (int i = 0; i < _handlers.Count - 1; i++)
            {
                _handlers[i].SetNext(_handlers[i + 1]);
            }

            return _handlers[0];
        }
    }
}
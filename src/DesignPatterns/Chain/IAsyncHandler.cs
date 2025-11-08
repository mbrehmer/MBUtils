using System.Threading;
using System.Threading.Tasks;

namespace MBUtils.DesignPatterns.Chain
{
    /// <summary>
    /// Defines the contract for an asynchronous handler in a chain of responsibility.
    /// Each handler can either process a request or pass it to the next handler in the chain.
    /// </summary>
    /// <typeparam name="TRequest">The type of request to handle.</typeparam>
    public interface IAsyncHandler<TRequest>
    {
        /// <summary>
        /// Sets the next handler in the chain.
        /// </summary>
        /// <param name="next">The next handler to invoke if this handler cannot process the request.</param>
        /// <returns>The next handler, allowing for fluent chaining.</returns>
        IAsyncHandler<TRequest> SetNext(IAsyncHandler<TRequest> next);

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
        Task<bool> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
    }
}
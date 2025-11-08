using System.Threading;
using System.Threading.Tasks;

namespace MBUtils.DesignPatterns.Facade
{
    /// <summary>
    /// Base class easing implementation of <see cref="IAsyncFacade"/>.
    /// Implements the public interface explicitly and exposes protected core methods for concrete types.
    /// </summary>
    /// <remarks>
    /// Inherit from this class to create an asynchronous facade that simplifies interaction with a complex subsystem.
    /// Override <see cref="ExecuteAsyncCore"/> to implement the coordination logic.
    /// </remarks>
    public abstract class AsyncFacadeBase : IAsyncFacade
    {
        /// <summary>
        /// Asynchronously executes the core facade logic, coordinating calls to the underlying subsystem.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// Implement this method to define how the facade interacts with the subsystem components asynchronously.
        /// This method should encapsulate the complexity and provide a simplified asynchronous workflow.
        /// </remarks>
        protected abstract Task ExecuteAsyncCore(CancellationToken cancellationToken);

        /// <inheritdoc />
        Task IAsyncFacade.ExecuteAsync(CancellationToken cancellationToken)
        {
            return ExecuteAsyncCore(cancellationToken);
        }
    }

    /// <summary>
    /// Base class easing implementation of <see cref="IAsyncFacade{TResult}"/>.
    /// Implements the public interface explicitly and exposes protected core methods for concrete types.
    /// </summary>
    /// <typeparam name="TResult">The type of the result returned by the facade operation.</typeparam>
    /// <remarks>
    /// Inherit from this class to create an asynchronous facade that simplifies interaction with a complex subsystem
    /// and returns a result. Override <see cref="ExecuteAsyncCore"/> to implement the coordination logic.
    /// </remarks>
    public abstract class AsyncFacadeBase<TResult> : IAsyncFacade<TResult>
    {
        /// <summary>
        /// Asynchronously executes the core facade logic, coordinating calls to the underlying subsystem.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A task representing the asynchronous operation with the result.</returns>
        /// <remarks>
        /// Implement this method to define how the facade interacts with the subsystem components asynchronously.
        /// This method should encapsulate the complexity and provide a simplified asynchronous workflow.
        /// </remarks>
        protected abstract Task<TResult> ExecuteAsyncCore(CancellationToken cancellationToken);

        /// <inheritdoc />
        Task<TResult> IAsyncFacade<TResult>.ExecuteAsync(CancellationToken cancellationToken)
        {
            return ExecuteAsyncCore(cancellationToken);
        }
    }
}
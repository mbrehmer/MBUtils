using System.Threading;
using System.Threading.Tasks;

namespace MBUtils.DesignPatterns.Facade
{
    /// <summary>
    /// Defines an asynchronous facade that provides a simplified interface to a complex subsystem.
    /// </summary>
    /// <remarks>
    /// The Facade pattern provides a unified, simplified interface to a set of interfaces in a subsystem.
    /// It defines a higher-level interface that makes the subsystem easier to use by hiding its complexity.
    /// Use this interface when you need to provide a simple asynchronous entry point to a complex system.
    /// </remarks>
    public interface IAsyncFacade
    {
        /// <summary>
        /// Asynchronously executes the facade operation, coordinating calls to the underlying subsystem.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method encapsulates the complexity of interacting with multiple subsystem components,
        /// presenting a single, simple asynchronous operation to the client.
        /// </remarks>
        Task ExecuteAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Defines an asynchronous facade that provides a simplified interface to a complex subsystem
    /// and returns a result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result returned by the facade operation.</typeparam>
    /// <remarks>
    /// The Facade pattern provides a unified, simplified interface to a set of interfaces in a subsystem.
    /// It defines a higher-level interface that makes the subsystem easier to use by hiding its complexity.
    /// Use this interface when you need to provide a simple asynchronous entry point to a complex system that produces a result.
    /// </remarks>
    public interface IAsyncFacade<TResult>
    {
        /// <summary>
        /// Asynchronously executes the facade operation, coordinating calls to the underlying subsystem.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A task representing the asynchronous operation with the result.</returns>
        /// <remarks>
        /// This method encapsulates the complexity of interacting with multiple subsystem components,
        /// presenting a single, simple asynchronous operation to the client.
        /// </remarks>
        Task<TResult> ExecuteAsync(CancellationToken cancellationToken = default);
    }
}
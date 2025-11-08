using System.Threading;
using System.Threading.Tasks;

namespace MBUtils.DesignPatterns.Builder
{
    /// <summary>
    /// Defines the interface for an asynchronous builder that constructs complex objects.
    /// </summary>
    /// <remarks>
    /// This interface extends the Builder pattern to support asynchronous construction
    /// processes, useful when building objects requires I/O operations, network calls,
    /// or other async operations.
    /// </remarks>
    /// <typeparam name="T">The type of object being built.</typeparam>
    public interface IAsyncBuilder<T>
    {
        /// <summary>
        /// Resets the builder to its initial state.
        /// </summary>
        /// <remarks>
        /// This method clears any previously set parts of the object being
        /// built, allowing the builder to start fresh for a new construction
        /// process.
        /// </remarks>
        void Reset();

        /// <summary>
        /// Asynchronously builds and returns the final constructed object.
        /// </summary>
        /// <remarks>
        /// This method asynchronously assembles the parts that have been set and returns
        /// the fully constructed object of type T. After calling this method,
        /// the builder may need to be reset before building another object.
        /// </remarks>
        /// <param name="cancellationToken">A cancellation token to observe while building.</param>
        /// <returns>A task that represents the asynchronous build operation. The task result contains the constructed object of type T.</returns>
        Task<T> BuildAsync(CancellationToken cancellationToken = default);
    }
}
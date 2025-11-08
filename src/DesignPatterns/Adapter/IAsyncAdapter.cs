namespace MBUtils.DesignPatterns.Adapter
{
    /// <summary>
    /// Defines an asynchronous adapter that converts an object of type <typeparamref name="TSource"/>
    /// to an object of type <typeparamref name="TTarget"/>.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object to adapt from.</typeparam>
    /// <typeparam name="TTarget">The type of the target object to adapt to.</typeparam>
    /// <remarks>
    /// Use this interface when the adaptation process requires asynchronous operations,
    /// such as I/O, network calls, or other long-running operations.
    /// </remarks>
    public interface IAsyncAdapter<in TSource, TTarget>
    {
        /// <summary>
        /// Asynchronously adapts the specified source object to the target type.
        /// </summary>
        /// <param name="source">The source object to adapt.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// an object of type <typeparamref name="TTarget"/> adapted from the source.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
        /// <exception cref="System.OperationCanceledException">Thrown when the operation is canceled via the <paramref name="cancellationToken"/>.</exception>
        System.Threading.Tasks.Task<TTarget> AdaptAsync(TSource source, System.Threading.CancellationToken cancellationToken = default);
    }
}
using System.Threading;
using System.Threading.Tasks;

namespace MBUtils.DesignPatterns.Adapter
{
    /// <summary>
    /// Provides an abstract base class for asynchronous adapters that convert objects of type
    /// <typeparamref name="TSource"/> to objects of type <typeparamref name="TTarget"/>.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object to adapt from.</typeparam>
    /// <typeparam name="TTarget">The type of the target object to adapt to.</typeparam>
    /// <remarks>
    /// <para>
    /// Inheritors must implement the <see cref="AdaptAsyncCore"/> method to define the actual
    /// asynchronous adaptation logic. This base class handles null checking and delegates to the core method.
    /// </para>
    /// <para>
    /// This class explicitly implements <see cref="IAsyncAdapter{TSource, TTarget}"/> to provide
    /// a clean public API surface.
    /// </para>
    /// </remarks>
    public abstract class AsyncAdapterBase<TSource, TTarget> : IAsyncAdapter<TSource, TTarget>
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
        async Task<TTarget> IAsyncAdapter<TSource, TTarget>.AdaptAsync(TSource source, CancellationToken cancellationToken)
        {
            if (source == null)
            {
                throw new System.ArgumentNullException(nameof(source));
            }

            return await AdaptAsyncCore(source, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// When overridden in a derived class, performs the actual asynchronous adaptation logic
        /// to convert the source object to the target type.
        /// </summary>
        /// <param name="source">The source object to adapt. Guaranteed to be non-null.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains
        /// an object of type <typeparamref name="TTarget"/> adapted from the source.
        /// </returns>
        /// <remarks>
        /// This method is called by <see cref="IAsyncAdapter{TSource, TTarget}.AdaptAsync"/> after null checking.
        /// Implementers can assume that <paramref name="source"/> is never null.
        /// Implementers should respect the <paramref name="cancellationToken"/> and throw
        /// <see cref="System.OperationCanceledException"/> when cancellation is requested.
        /// </remarks>
        protected abstract Task<TTarget> AdaptAsyncCore(TSource source, CancellationToken cancellationToken);
    }
}
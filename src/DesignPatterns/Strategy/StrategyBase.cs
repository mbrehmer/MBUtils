using System.Threading;
using System.Threading.Tasks;

namespace MBUtils.DesignPatterns.Strategy
{
    /// <summary>
    /// Base class easing implementation of <see cref="IStrategy{TContext, TResult}"/>.
    /// Implements the public interface explicitly and exposes protected core methods for concrete types.
    /// </summary>
    /// <typeparam name="TContext">Type of the context.</typeparam>
    /// <typeparam name="TResult">Type of the result.</typeparam>
    public abstract class StrategyBase<TContext, TResult> : IStrategy<TContext, TResult>
    {
        /// <summary>
        /// Synchronously executes the strategy core implementation.
        /// </summary>
        /// <param name="context">The context for the execution.</param>
        /// <returns>The result.</returns>
        protected abstract TResult ExecuteCore(TContext context);

        /// <summary>
        /// Asynchronously executes the strategy core implementation.
        /// Default implementation calls the synchronous <see cref="ExecuteCore(TContext)"/>.
        /// Override when asynchronous work is necessary.
        /// </summary>
        /// <param name="context">The context for the execution.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result task.</returns>
        protected virtual Task<TResult> ExecuteAsyncCore(TContext context, CancellationToken cancellationToken)
        {
            TResult result = ExecuteCore(context);
            return Task.FromResult(result);
        }

        /// <inheritdoc />
        TResult IStrategy<TContext, TResult>.Execute(TContext context)
        {
            return ExecuteCore(context);
        }

        /// <inheritdoc />
        Task<TResult> IStrategy<TContext, TResult>.ExecuteAsync(TContext context, CancellationToken cancellationToken)
        {
            return ExecuteAsyncCore(context, cancellationToken);
        }
    }
}
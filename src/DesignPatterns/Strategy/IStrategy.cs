using System.Threading;
using System.Threading.Tasks;

namespace MBUtils.DesignPatterns.Strategy
{
    /// <summary>
    /// Represents an interchangeable algorithm or policy that can be executed with a context producing a result.
    /// </summary>
    /// <typeparam name="TContext">The type of the context passed to the strategy.</typeparam>
    /// <typeparam name="TResult">The type of the result produced by the strategy.</typeparam>
    public interface IStrategy<TContext, TResult>
    {
        /// <summary>
        /// Execute the strategy synchronously.
        /// </summary>
        /// <param name="context">The context for the execution.</param>
        /// <returns>The result produced by the strategy.</returns>
        TResult Execute(TContext context);

        /// <summary>
        /// Execute the strategy asynchronously.
        /// </summary>
        /// <param name="context">The context for the execution.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task that yields the result produced by the strategy.</returns>
        Task<TResult> ExecuteAsync(TContext context, CancellationToken cancellationToken = default);
    }
}
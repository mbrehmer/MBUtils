using System.Threading;
using System.Threading.Tasks;

namespace MBUtils.DesignPatterns.Decorator
{
    /// <summary>
    /// Represents an asynchronous decorator that wraps a component and adds additional behavior.
    /// </summary>
    /// <typeparam name="T">The type of the component being decorated.</typeparam>
    public interface IAsyncDecorator<T>
    {
        /// <summary>
        /// Gets the wrapped component.
        /// </summary>
        T Component { get; }

        /// <summary>
        /// Executes the decorator's behavior asynchronously, typically wrapping the component's behavior.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task that yields the result after applying the decorator's behavior.</returns>
        Task<T> ExecuteAsync(CancellationToken cancellationToken = default);
    }
}
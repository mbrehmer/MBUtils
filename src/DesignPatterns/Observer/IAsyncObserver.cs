using System.Threading;
using System.Threading.Tasks;

namespace MBUtils.DesignPatterns.Observer
{
    /// <summary>
    /// Represents an observer that receives asynchronous notifications from a subject.
    /// </summary>
    /// <typeparam name="T">The type of data provided with the notification.</typeparam>
    /// <remarks>
    /// Implementations of this interface define an asynchronous response to notifications
    /// from subjects they observe. This supports scenarios where observers need to perform
    /// I/O-bound or long-running operations in response to state changes.
    /// </remarks>
    public interface IAsyncObserver<T>
    {
        /// <summary>
        /// Asynchronously receives a notification from a subject.
        /// </summary>
        /// <param name="data">The data associated with the notification.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous update operation.</returns>
        /// <remarks>
        /// This method is called by the subject when notifying observers of state changes.
        /// Implementations should handle the notification asynchronously and update their
        /// state accordingly. Exceptions thrown by this method may prevent other observers
        /// from being notified, depending on the subject's notification strategy.
        /// </remarks>
        Task UpdateAsync(T data, CancellationToken cancellationToken = default);
    }
}
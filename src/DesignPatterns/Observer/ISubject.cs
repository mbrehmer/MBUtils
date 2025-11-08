using System.Threading;
using System.Threading.Tasks;

namespace MBUtils.DesignPatterns.Observer
{
    /// <summary>
    /// Represents a subject that maintains a list of observers and notifies them of state changes.
    /// </summary>
    /// <typeparam name="T">The type of data provided with notifications.</typeparam>
    /// <remarks>
    /// Implementations of this interface manage a collection of observers and provide
    /// methods to attach, detach, and notify them. This supports the Observer design
    /// pattern, enabling a one-to-many dependency between objects where state changes
    /// in the subject are automatically propagated to all registered observers.
    /// </remarks>
    public interface ISubject<T>
    {
        /// <summary>
        /// Attaches an observer to the subject.
        /// </summary>
        /// <param name="observer">The observer to attach.</param>
        /// <remarks>
        /// Once attached, the observer will receive notifications when the subject's
        /// state changes. If the observer is already attached, the behavior depends
        /// on the implementation (may be ignored or added again).
        /// </remarks>
        void Attach(IObserver<T> observer);

        /// <summary>
        /// Detaches an observer from the subject.
        /// </summary>
        /// <param name="observer">The observer to detach.</param>
        /// <remarks>
        /// After detaching, the observer will no longer receive notifications from
        /// the subject. If the observer is not currently attached, this method has
        /// no effect.
        /// </remarks>
        void Detach(IObserver<T> observer);

        /// <summary>
        /// Notifies all attached observers of a state change.
        /// </summary>
        /// <param name="data">The data to send with the notification.</param>
        /// <remarks>
        /// This method calls the Update method on each attached synchronous observer.
        /// Exceptions thrown by observers may prevent subsequent observers from being
        /// notified, depending on the implementation.
        /// </remarks>
        void Notify(T data);

        /// <summary>
        /// Attaches an asynchronous observer to the subject.
        /// </summary>
        /// <param name="observer">The asynchronous observer to attach.</param>
        /// <remarks>
        /// Once attached, the observer will receive asynchronous notifications when
        /// the subject's state changes. If the observer is already attached, the
        /// behavior depends on the implementation (may be ignored or added again).
        /// </remarks>
        void AttachAsync(IAsyncObserver<T> observer);

        /// <summary>
        /// Detaches an asynchronous observer from the subject.
        /// </summary>
        /// <param name="observer">The asynchronous observer to detach.</param>
        /// <remarks>
        /// After detaching, the observer will no longer receive notifications from
        /// the subject. If the observer is not currently attached, this method has
        /// no effect.
        /// </remarks>
        void DetachAsync(IAsyncObserver<T> observer);

        /// <summary>
        /// Asynchronously notifies all attached observers of a state change.
        /// </summary>
        /// <param name="data">The data to send with the notification.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous notification operation.</returns>
        /// <remarks>
        /// This method calls the UpdateAsync method on each attached asynchronous observer
        /// and the Update method on each attached synchronous observer. Exceptions thrown
        /// by observers may prevent subsequent observers from being notified, depending
        /// on the implementation.
        /// </remarks>
        Task NotifyAsync(T data, CancellationToken cancellationToken = default);
    }
}
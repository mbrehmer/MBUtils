namespace MBUtils.DesignPatterns.Observer
{
    /// <summary>
    /// Represents an observer that receives notifications from a subject.
    /// </summary>
    /// <typeparam name="T">The type of data provided with the notification.</typeparam>
    /// <remarks>
    /// Implementations of this interface define the response to notifications
    /// from subjects they observe. This supports the Observer design pattern,
    /// enabling loose coupling between subjects and their observers.
    /// </remarks>
    public interface IObserver<T>
    {
        /// <summary>
        /// Receives a notification from a subject.
        /// </summary>
        /// <param name="data">The data associated with the notification.</param>
        /// <remarks>
        /// This method is called by the subject when notifying observers of state changes.
        /// Implementations should handle the notification and update their state accordingly.
        /// Exceptions thrown by this method may prevent other observers from being notified,
        /// depending on the subject's notification strategy.
        /// </remarks>
        void Update(T data);
    }
}
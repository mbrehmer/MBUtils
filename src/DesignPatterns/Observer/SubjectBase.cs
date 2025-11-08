using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MBUtils.DesignPatterns.Observer
{
    /// <summary>
    /// Abstract base class providing a thread-safe implementation of the Subject in the Observer pattern.
    /// </summary>
    /// <typeparam name="T">The type of data provided with notifications.</typeparam>
    /// <remarks>
    /// This class provides a complete implementation of <see cref="ISubject{T}"/> with thread-safe
    /// observer management. It maintains separate lists for synchronous and asynchronous observers
    /// and provides methods to attach, detach, and notify them. Derived classes can use this base
    /// to easily implement observable subjects without managing observer collections manually.
    ///
    /// The implementation uses locking to ensure thread-safe access to observer collections,
    /// making it safe to attach, detach, and notify observers from multiple threads concurrently.
    ///
    /// Notifications are sent to all observers in the order they were attached. If an observer
    /// throws an exception during notification, it will propagate to the caller and may prevent
    /// subsequent observers from being notified.
    /// </remarks>
    /// <example>
    /// <code>
    /// public class TemperatureSensor : SubjectBase&lt;double&gt;
    /// {
    ///     private double _temperature;
    ///
    ///     public double Temperature
    ///     {
    ///         get => _temperature;
    ///         set
    ///         {
    ///             _temperature = value;
    ///             Notify(_temperature);
    ///         }
    ///     }
    /// }
    ///
    /// // Usage:
    /// TemperatureSensor sensor = new TemperatureSensor();
    /// sensor.Attach(new DisplayObserver());
    /// sensor.Temperature = 25.5; // Notifies all observers
    /// </code>
    /// </example>
    public abstract class SubjectBase<T> : ISubject<T>
    {
        private readonly object _syncLock = new object();
        private readonly List<IObserver<T>> _observers = new List<IObserver<T>>();
        private readonly List<IAsyncObserver<T>> _asyncObservers = new List<IAsyncObserver<T>>();

        /// <summary>
        /// Attaches a synchronous observer to the subject.
        /// </summary>
        /// <param name="observer">The observer to attach.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="observer"/> is null.</exception>
        /// <remarks>
        /// The observer will be added to the internal list and will receive notifications
        /// when <see cref="Notify(T)"/> or <see cref="NotifyAsync(T, CancellationToken)"/> is called.
        /// This method is thread-safe.
        /// </remarks>
        void ISubject<T>.Attach(IObserver<T> observer)
        {
            if (observer == null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            lock (_syncLock)
            {
                _observers.Add(observer);
            }
        }

        /// <summary>
        /// Detaches a synchronous observer from the subject.
        /// </summary>
        /// <param name="observer">The observer to detach.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="observer"/> is null.</exception>
        /// <remarks>
        /// Removes the first occurrence of the observer from the internal list.
        /// If the observer is not attached, this method has no effect.
        /// This method is thread-safe.
        /// </remarks>
        void ISubject<T>.Detach(IObserver<T> observer)
        {
            if (observer == null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            lock (_syncLock)
            {
                _observers.Remove(observer);
            }
        }

        /// <summary>
        /// Notifies all attached synchronous observers of a state change.
        /// </summary>
        /// <param name="data">The data to send with the notification.</param>
        /// <remarks>
        /// This method calls <see cref="IObserver{T}.Update(T)"/> on each attached synchronous observer.
        /// Observers are notified in the order they were attached. If an observer throws an exception,
        /// it will propagate to the caller and subsequent observers will not be notified.
        /// A snapshot of the observer list is taken before notification to avoid issues if observers
        /// are attached or detached during notification.
        /// </remarks>
        void ISubject<T>.Notify(T data)
        {
            IObserver<T>[] snapshot;
            lock (_syncLock)
            {
                snapshot = _observers.ToArray();
            }

            foreach (IObserver<T> observer in snapshot)
            {
                observer.Update(data);
            }
        }

        /// <summary>
        /// Attaches an asynchronous observer to the subject.
        /// </summary>
        /// <param name="observer">The asynchronous observer to attach.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="observer"/> is null.</exception>
        /// <remarks>
        /// The observer will be added to the internal list and will receive asynchronous notifications
        /// when <see cref="NotifyAsync(T, CancellationToken)"/> is called.
        /// This method is thread-safe.
        /// </remarks>
        void ISubject<T>.AttachAsync(IAsyncObserver<T> observer)
        {
            if (observer == null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            lock (_syncLock)
            {
                _asyncObservers.Add(observer);
            }
        }

        /// <summary>
        /// Detaches an asynchronous observer from the subject.
        /// </summary>
        /// <param name="observer">The asynchronous observer to detach.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="observer"/> is null.</exception>
        /// <remarks>
        /// Removes the first occurrence of the observer from the internal list.
        /// If the observer is not attached, this method has no effect.
        /// This method is thread-safe.
        /// </remarks>
        void ISubject<T>.DetachAsync(IAsyncObserver<T> observer)
        {
            if (observer == null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            lock (_syncLock)
            {
                _asyncObservers.Remove(observer);
            }
        }

        /// <summary>
        /// Asynchronously notifies all attached observers of a state change.
        /// </summary>
        /// <param name="data">The data to send with the notification.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous notification operation.</returns>
        /// <remarks>
        /// This method notifies both synchronous and asynchronous observers. Synchronous observers
        /// are called via <see cref="IObserver{T}.Update(T)"/> and asynchronous observers via
        /// <see cref="IAsyncObserver{T}.UpdateAsync(T, CancellationToken)"/>.
        /// Observers are notified in the order they were attached. If an observer throws an exception,
        /// it will propagate to the caller and subsequent observers will not be notified.
        /// A snapshot of the observer lists is taken before notification to avoid issues if observers
        /// are attached or detached during notification.
        /// </remarks>
        async Task ISubject<T>.NotifyAsync(T data, CancellationToken cancellationToken)
        {
            IObserver<T>[] syncSnapshot;
            IAsyncObserver<T>[] asyncSnapshot;

            lock (_syncLock)
            {
                syncSnapshot = _observers.ToArray();
                asyncSnapshot = _asyncObservers.ToArray();
            }

            foreach (IObserver<T> observer in syncSnapshot)
            {
                observer.Update(data);
            }

            foreach (IAsyncObserver<T> asyncObserver in asyncSnapshot)
            {
                await asyncObserver.UpdateAsync(data, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Protected method to attach an observer, accessible to derived classes.
        /// </summary>
        /// <param name="observer">The observer to attach.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="observer"/> is null.</exception>
        protected void Attach(IObserver<T> observer)
        {
            ((ISubject<T>)this).Attach(observer);
        }

        /// <summary>
        /// Protected method to detach an observer, accessible to derived classes.
        /// </summary>
        /// <param name="observer">The observer to detach.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="observer"/> is null.</exception>
        protected void Detach(IObserver<T> observer)
        {
            ((ISubject<T>)this).Detach(observer);
        }

        /// <summary>
        /// Protected method to notify all synchronous observers, accessible to derived classes.
        /// </summary>
        /// <param name="data">The data to send with the notification.</param>
        protected void Notify(T data)
        {
            ((ISubject<T>)this).Notify(data);
        }

        /// <summary>
        /// Protected method to attach an asynchronous observer, accessible to derived classes.
        /// </summary>
        /// <param name="observer">The asynchronous observer to attach.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="observer"/> is null.</exception>
        protected void AttachAsync(IAsyncObserver<T> observer)
        {
            ((ISubject<T>)this).AttachAsync(observer);
        }

        /// <summary>
        /// Protected method to detach an asynchronous observer, accessible to derived classes.
        /// </summary>
        /// <param name="observer">The asynchronous observer to detach.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="observer"/> is null.</exception>
        protected void DetachAsync(IAsyncObserver<T> observer)
        {
            ((ISubject<T>)this).DetachAsync(observer);
        }

        /// <summary>
        /// Protected method to asynchronously notify all observers, accessible to derived classes.
        /// </summary>
        /// <param name="data">The data to send with the notification.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous notification operation.</returns>
        protected Task NotifyAsync(T data, CancellationToken cancellationToken = default)
        {
            return ((ISubject<T>)this).NotifyAsync(data, cancellationToken);
        }
    }
}
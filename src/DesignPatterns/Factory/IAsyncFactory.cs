using System.Threading;
using System.Threading.Tasks;

namespace MBUtils.DesignPatterns.Factory
{
    /// <summary>
    /// Defines an asynchronous factory that creates instances of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of object this factory creates.</typeparam>
    public interface IAsyncFactory<T>
    {
        /// <summary>
        /// Asynchronously creates a new instance of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a new instance of <typeparamref name="T"/>.</returns>
        Task<T> CreateAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Defines a keyed asynchronous factory that creates instances of type <typeparamref name="T"/> based on a key.
    /// </summary>
    /// <typeparam name="TKey">The type of key used to identify which concrete type to create.</typeparam>
    /// <typeparam name="T">The base type of object this factory creates.</typeparam>
    public interface IAsyncFactory<in TKey, T> where TKey : notnull
    {
        /// <summary>
        /// Asynchronously creates a new instance of <typeparamref name="T"/> based on the specified key.
        /// </summary>
        /// <param name="key">The key identifying which concrete type to create.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a new instance of <typeparamref name="T"/>.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="key"/> is null.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown when no creator is registered for the specified key.</exception>
        Task<T> CreateAsync(TKey key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Determines whether a creator is registered for the specified key.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns><c>true</c> if a creator is registered for the key; otherwise, <c>false</c>.</returns>
        bool IsRegistered(TKey key);
    }
}
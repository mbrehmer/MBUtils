namespace MBUtils.DesignPatterns.Factory
{
    /// <summary>
    /// Defines a factory that creates instances of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of object this factory creates.</typeparam>
    public interface IFactory<out T>
    {
        /// <summary>
        /// Creates a new instance of <typeparamref name="T"/>.
        /// </summary>
        /// <returns>A new instance of <typeparamref name="T"/>.</returns>
        T Create();
    }

    /// <summary>
    /// Defines a keyed factory that creates instances of type <typeparamref name="T"/> based on a key.
    /// </summary>
    /// <typeparam name="TKey">The type of key used to identify which concrete type to create.</typeparam>
    /// <typeparam name="T">The base type of object this factory creates.</typeparam>
    public interface IFactory<in TKey, out T> where TKey : notnull
    {
        /// <summary>
        /// Creates a new instance of <typeparamref name="T"/> based on the specified key.
        /// </summary>
        /// <param name="key">The key identifying which concrete type to create.</param>
        /// <returns>A new instance of <typeparamref name="T"/>.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="key"/> is null.</exception>
        /// <exception cref="System.InvalidOperationException">Thrown when no creator is registered for the specified key.</exception>
        T Create(TKey key);

        /// <summary>
        /// Determines whether a creator is registered for the specified key.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns><c>true</c> if a creator is registered for the key; otherwise, <c>false</c>.</returns>
        bool IsRegistered(TKey key);
    }
}
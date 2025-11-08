using System;
using System.Collections.Generic;

namespace MBUtils.DesignPatterns.Factory
{
    /// <summary>
    /// Provides a thread-safe base implementation for keyed factories that create instances of type <typeparamref name="T"/>.
    /// Concrete factories register creator functions for different keys and resolve them on demand.
    /// </summary>
    /// <typeparam name="TKey">The type of key used to identify which concrete type to create.</typeparam>
    /// <typeparam name="T">The base type of object this factory creates.</typeparam>
    public abstract class FactoryBase<TKey, T> : IFactory<TKey, T> where TKey : notnull
    {
        private readonly Dictionary<TKey, Func<T>> _creators;
        private readonly object _sync = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="FactoryBase{TKey, T}"/> class.
        /// </summary>
        protected FactoryBase()
        {
            _creators = new Dictionary<TKey, Func<T>>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FactoryBase{TKey, T}"/> class with a custom key comparer.
        /// </summary>
        /// <param name="comparer">The equality comparer to use for comparing keys.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="comparer"/> is null.</exception>
        protected FactoryBase(IEqualityComparer<TKey> comparer)
        {
            if (comparer == null)
            {
                throw new ArgumentNullException(nameof(comparer));
            }

            _creators = new Dictionary<TKey, Func<T>>(comparer);
        }

        /// <summary>
        /// Creates a new instance of <typeparamref name="T"/> based on the specified key.
        /// </summary>
        /// <param name="key">The key identifying which concrete type to create.</param>
        /// <returns>A new instance of <typeparamref name="T"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when no creator is registered for the specified key.</exception>
        public T Create(TKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            Func<T>? creator;
            lock (_sync)
            {
                if (!_creators.TryGetValue(key, out creator))
                {
                    throw new InvalidOperationException($"No creator registered for key: {key}");
                }
            }

            return creator();
        }

        /// <summary>
        /// Determines whether a creator is registered for the specified key.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns><c>true</c> if a creator is registered for the key; otherwise, <c>false</c>.</returns>
        public bool IsRegistered(TKey key)
        {
            if (key == null)
            {
                return false;
            }

            lock (_sync)
            {
                return _creators.ContainsKey(key);
            }
        }

        /// <summary>
        /// Registers a creator function for the specified key.
        /// </summary>
        /// <param name="key">The key to register the creator for.</param>
        /// <param name="creator">The function that creates instances.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> or <paramref name="creator"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when a creator is already registered for the specified key.</exception>
        protected void Register(TKey key, Func<T> creator)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (creator == null)
            {
                throw new ArgumentNullException(nameof(creator));
            }

            lock (_sync)
            {
                if (_creators.ContainsKey(key))
                {
                    throw new InvalidOperationException($"A creator is already registered for key: {key}");
                }

                _creators[key] = creator;
            }
        }

        /// <summary>
        /// Unregisters the creator function for the specified key.
        /// </summary>
        /// <param name="key">The key to unregister.</param>
        /// <returns><c>true</c> if the creator was successfully removed; otherwise, <c>false</c>.</returns>
        protected bool Unregister(TKey key)
        {
            if (key == null)
            {
                return false;
            }

            lock (_sync)
            {
                return _creators.Remove(key);
            }
        }

        /// <summary>
        /// Clears all registered creators.
        /// </summary>
        protected void ClearRegistrations()
        {
            lock (_sync)
            {
                _creators.Clear();
            }
        }

        /// <summary>
        /// Gets the number of registered creators.
        /// </summary>
        protected int RegistrationCount
        {
            get
            {
                lock (_sync)
                {
                    return _creators.Count;
                }
            }
        }
    }
}
using System;

namespace MBUtils.DesignPatterns.Singleton
{
    /// <summary>
    /// Provides a thread-safe abstract base implementation of the Singleton design pattern.
    /// </summary>
    /// <remarks>
    /// This abstract class provides a generic, thread-safe implementation of the Singleton pattern
    /// using double-check locking for lazy initialization. Derived classes must provide a parameterless
    /// constructor and can override <see cref="OnInstanceCreated"/> to perform initialization logic
    /// after the singleton instance is created.
    ///
    /// The implementation ensures that only one instance of the derived type is created throughout
    /// the application lifetime, and provides thread-safe access to that instance via the
    /// <see cref="Instance"/> property.
    ///
    /// Note: Derived classes should have a private or protected parameterless constructor to prevent
    /// direct instantiation outside of the singleton mechanism.
    /// </remarks>
    /// <typeparam name="T">The type of the singleton class. Must be a class with a parameterless constructor.</typeparam>
    /// <example>
    /// <code>
    /// public class DatabaseConnection : SingletonBase&lt;DatabaseConnection&gt;
    /// {
    ///     private DatabaseConnection()
    ///     {
    ///         // Private constructor to prevent direct instantiation
    ///     }
    ///
    ///     protected override void OnInstanceCreated()
    ///     {
    ///         // Perform initialization after instance creation
    ///         Connect();
    ///     }
    ///
    ///     public void Connect()
    ///     {
    ///         // Connection logic
    ///     }
    /// }
    ///
    /// // Usage:
    /// DatabaseConnection connection = DatabaseConnection.Instance;
    /// </code>
    /// </example>
    public abstract class SingletonBase<T> where T : class
    {
        private static Lazy<T> _lazyInstance = new Lazy<T>(CreateInstance, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// Gets the singleton instance of the type <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>
        /// This property uses <see cref="Lazy{T}"/> to ensure thread-safe lazy initialization.
        /// The first access to this property will create the singleton instance by invoking
        /// <see cref="CreateInstance"/>, and subsequent accesses will return the cached instance.
        /// After the instance is created, <see cref="OnInstanceCreated"/> is called to allow
        /// derived classes to perform additional initialization.
        /// </remarks>
        /// <returns>The singleton instance of type <typeparamref name="T"/>.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the type <typeparamref name="T"/> does not have a parameterless constructor
        /// or when instance creation fails.
        /// </exception>
        public static T Instance
        {
            get
            {
                return _lazyInstance.Value;
            }
        }

        /// <summary>
        /// Creates a new instance of the singleton type <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>
        /// This method uses reflection to invoke the parameterless constructor of the derived type.
        /// The constructor can be private, protected, or public. If the type does not have a
        /// parameterless constructor, this method will throw an <see cref="InvalidOperationException"/>.
        /// After the instance is created, <see cref="OnInstanceCreated"/> is called if the instance
        /// inherits from <see cref="SingletonBase{T}"/>.
        /// </remarks>
        /// <returns>A new instance of type <typeparamref name="T"/>.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the type <typeparamref name="T"/> does not have a parameterless constructor
        /// or when instance creation fails due to an exception in the constructor.
        /// </exception>
        private static T CreateInstance()
        {
            try
            {
                Type type = typeof(T);
                System.Reflection.ConstructorInfo? constructor = type.GetConstructor(
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public,
                    null,
                    Type.EmptyTypes,
                    null
                );

                if (constructor == null)
                {
                    throw new InvalidOperationException(
                        $"Type '{type.FullName}' does not have a parameterless constructor. " +
                        "Singleton types must provide a parameterless constructor (private, protected, or public)."
                    );
                }

                T instance = (T)constructor.Invoke(null);

                if (instance is SingletonBase<T> singleton)
                {
                    singleton.OnInstanceCreated();
                }

                return instance;
            }
            catch (Exception ex)
            {
                if (ex is InvalidOperationException)
                {
                    throw;
                }

                throw new InvalidOperationException(
                    $"Failed to create singleton instance of type '{typeof(T).FullName}'. " +
                    "See inner exception for details.",
                    ex
                );
            }
        }

        /// <summary>
        /// Called after the singleton instance is created.
        /// </summary>
        /// <remarks>
        /// Override this method in derived classes to perform initialization logic
        /// that should occur after the singleton instance is constructed. This method
        /// is called only once, immediately after the instance is created and before
        /// it is returned from the <see cref="Instance"/> property for the first time.
        ///
        /// The default implementation does nothing.
        /// </remarks>
        protected virtual void OnInstanceCreated()
        {
            // Default implementation does nothing
        }

        /// <summary>
        /// Resets the singleton instance for testing purposes.
        /// </summary>
        /// <remarks>
        /// This method is intended for use in unit tests to reset the singleton state
        /// between test cases. It should not be called in production code.
        ///
        /// Warning: Calling this method in a multi-threaded environment may lead to
        /// race conditions and unexpected behavior. Use with caution.
        /// </remarks>
        protected static void ResetInstance()
        {
            _lazyInstance = new Lazy<T>(CreateInstance, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MBUtils.DesignPatterns.Builder
{
    /// <summary>
    /// Provides an abstract base implementation of the asynchronous Builder design pattern with fluent interface support.
    /// </summary>
    /// <remarks>
    /// This abstract class implements the <see cref="IAsyncBuilder{T}"/> interface
    /// and provides a common foundation for concrete async builder implementations with additional
    /// features including validation, state tracking, and method chaining support.
    /// Derived classes must implement the <see cref="ResetCore"/>, <see cref="BuildAsyncCore"/>, and
    /// <see cref="ValidateAsync"/> methods to define how objects are constructed, reset, and validated asynchronously.
    /// </remarks>
    /// <typeparam name="TBuilder">The type of the concrete builder (for fluent interface support).</typeparam>
    /// <typeparam name="TProduct">The type of object being built.</typeparam>
    public abstract class AsyncBuilderBase<TBuilder, TProduct> : IAsyncBuilder<TProduct>
        where TBuilder : AsyncBuilderBase<TBuilder, TProduct>
    {
        private bool _isBuilt;

        /// <summary>
        /// Gets a value indicating whether an object has been built since the last reset.
        /// </summary>
        protected bool IsBuilt => _isBuilt;

        /// <summary>
        /// Asynchronously checks if the builder is in a valid state for building.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to observe while validating.</param>
        /// <returns>A task that represents the asynchronous validation operation. The task result is true if the builder is valid; otherwise, false.</returns>
        public async Task<bool> IsValidAsync(CancellationToken cancellationToken = default)
        {
            if (_isBuilt)
                return false;

            IReadOnlyCollection<string> errors = await ValidateAsync(cancellationToken).ConfigureAwait(false);
            return errors.Count == 0;
        }

        /// <summary>
        /// Resets the builder to its initial state.
        /// </summary>
        /// <remarks>
        /// This method clears any previously set parts of the object being built
        /// and resets the internal state, allowing the builder to start fresh for
        /// a new construction process. After calling this method, the builder can
        /// be reused to build another object.
        /// </remarks>
        public void Reset()
        {
            _isBuilt = false;
            ResetCore();
        }

        /// <summary>
        /// Resets the builder's internal state to its initial values.
        /// </summary>
        /// <remarks>
        /// This method must be implemented by derived classes to clear any
        /// builder-specific state and prepare for a new construction process.
        /// </remarks>
        protected abstract void ResetCore();

        /// <summary>
        /// Asynchronously builds and returns the final constructed object.
        /// </summary>
        /// <remarks>
        /// This method validates the builder's state, assembles the parts that have
        /// been set, and returns the fully constructed object. After calling this method,
        /// the builder must be reset before building another object.
        /// </remarks>
        /// <param name="cancellationToken">A cancellation token to observe while building.</param>
        /// <returns>A task that represents the asynchronous build operation. The task result contains the constructed object of type <typeparamref name="TProduct"/>.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the builder has already been used to build an object without being reset,
        /// or when the builder's state is invalid.
        /// </exception>
        public async Task<TProduct> BuildAsync(CancellationToken cancellationToken = default)
        {
            if (_isBuilt)
            {
                throw new InvalidOperationException(
                    "This builder has already been used. Call Reset() before building another object.");
            }

            IReadOnlyCollection<string> errors = await ValidateAsync(cancellationToken).ConfigureAwait(false);
            if (errors.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Cannot build object. Validation failed with the following errors: {string.Join(", ", errors)}");
            }
            _isBuilt = true;
            return await BuildAsyncCore(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously builds and returns the final constructed object.
        /// </summary>
        /// <remarks>
        /// This method must be implemented by derived classes to asynchronously assemble the parts
        /// and return the fully constructed object. This method is called after
        /// validation has passed.
        /// </remarks>
        /// <param name="cancellationToken">A cancellation token to observe while building.</param>
        /// <returns>A task that represents the asynchronous build operation. The task result contains the constructed object of type <typeparamref name="TProduct"/>.</returns>
        protected abstract Task<TProduct> BuildAsyncCore(CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously validates the current state of the builder.
        /// </summary>
        /// <remarks>
        /// This method must be implemented by derived classes to validate that
        /// all required parts have been set and the builder is in a valid state
        /// for building an object. Return an empty collection if validation passes.
        /// </remarks>
        /// <param name="cancellationToken">A cancellation token to observe while validating.</param>
        /// <returns>A task that represents the asynchronous validation operation. The task result contains a collection of validation error messages, or an empty collection if valid.</returns>
        protected abstract Task<IReadOnlyCollection<string>> ValidateAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Returns this builder instance cast to the derived builder type for method chaining.
        /// </summary>
        /// <returns>This builder instance as <typeparamref name="TBuilder"/>.</returns>
        protected TBuilder This() => (TBuilder)this;
    }
}
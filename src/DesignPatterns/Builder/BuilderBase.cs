using System;
using System.Collections.Generic;

namespace MBUtils.DesignPatterns.Builder
{
    /// <summary>
    /// Provides an abstract base implementation of the Builder design pattern with fluent interface support.
    /// </summary>
    /// <remarks>
    /// This abstract class implements the <see cref="IBuilder{T}"/> interface
    /// and provides a common foundation for concrete builder implementations with additional
    /// features including validation, state tracking, and method chaining support.
    /// Derived classes must implement the <see cref="Reset"/>, <see cref="Build"/>, and
    /// <see cref="Validate"/> methods to define how objects are constructed, reset, and validated.
    /// </remarks>
    /// <typeparam name="TBuilder">The type of the concrete builder (for fluent interface support).</typeparam>
    /// <typeparam name="TProduct">The type of object being built.</typeparam>
    public abstract class BuilderBase<TBuilder, TProduct> : IBuilder<TProduct>
        where TBuilder : BuilderBase<TBuilder, TProduct>
    {
        private bool _isBuilt;
        private readonly List<string> _validationErrors = new List<string>();

        /// <summary>
        /// Gets a value indicating whether the builder is in a valid state for building.
        /// </summary>
        public bool IsValid => !_isBuilt && Validate().Count == 0;

        /// <summary>
        /// Gets a value indicating whether an object has been built since the last reset.
        /// </summary>
        protected bool IsBuilt => _isBuilt;

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
            _validationErrors.Clear();
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
        /// Builds and returns the final constructed object.
        /// </summary>
        /// <remarks>
        /// This method validates the builder's state, assembles the parts that have
        /// been set, and returns the fully constructed object. After calling this method,
        /// the builder must be reset before building another object.
        /// </remarks>
        /// <returns>The constructed object of type <typeparamref name="TProduct"/>.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the builder has already been used to build an object without being reset,
        /// or when the builder's state is invalid.
        /// </exception>
        public TProduct Build()
        {
            if (_isBuilt)
            {
                throw new InvalidOperationException(
                    "This builder has already been used. Call Reset() before building another object.");
            }

            IReadOnlyCollection<string> errors = Validate();
            if (errors.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Cannot build object. Validation failed with the following errors: {string.Join(", ", errors)}");
            }
            _isBuilt = true;
            return BuildCore();
        }

        /// <summary>
        /// Builds and returns the final constructed object.
        /// </summary>
        /// <remarks>
        /// This method must be implemented by derived classes to assemble the parts
        /// and return the fully constructed object. This method is called after
        /// validation has passed.
        /// </remarks>
        /// <returns>The constructed object of type <typeparamref name="TProduct"/>.</returns>
        protected abstract TProduct BuildCore();

        /// <summary>
        /// Validates the current state of the builder.
        /// </summary>
        /// <remarks>
        /// This method must be implemented by derived classes to validate that
        /// all required parts have been set and the builder is in a valid state
        /// for building an object. Return an empty collection if validation passes.
        /// </remarks>
        /// <returns>A collection of validation error messages, or an empty collection if valid.</returns>
        protected abstract IReadOnlyCollection<string> Validate();

        /// <summary>
        /// Returns this builder instance cast to the derived builder type for method chaining.
        /// </summary>
        /// <returns>This builder instance as <typeparamref name="TBuilder"/>.</returns>
        protected TBuilder This() => (TBuilder)this;
    }
}
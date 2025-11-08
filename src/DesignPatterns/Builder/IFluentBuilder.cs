namespace MBUtils.DesignPatterns.Builder
{
    /// <summary>
    /// Defines the interface for a fluent builder that constructs complex objects with method chaining.
    /// </summary>
    /// <remarks>
    /// This interface extends <see cref="IBuilder{T}"/> to provide additional capabilities
    /// for fluent interface patterns, including validation state checking. Implementations
    /// should support method chaining by returning the builder instance from configuration methods.
    /// </remarks>
    /// <typeparam name="TProduct">The type of object being built.</typeparam>
    public interface IFluentBuilder<TProduct> : IBuilder<TProduct>
    {
        /// <summary>
        /// Gets a value indicating whether the builder is in a valid state for building.
        /// </summary>
        /// <remarks>
        /// This property should return true only if all required parts have been set
        /// and the builder can successfully build a valid object.
        /// </remarks>
        bool IsValid { get; }
    }
}
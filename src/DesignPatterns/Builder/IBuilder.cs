namespace MBUtils.DesignPatterns.Builder
{
    /// <summary>
    /// Defines the interface for a builder that constructs complex objects.
    /// </summary>
    /// <remarks>
    /// Implementations of this interface provide methods to incrementally
    /// build parts of a complex object and a method to retrieve the final
    /// constructed object. This supports the Builder design pattern,
    /// allowing for flexible and step-by-step object creation.
    /// </remarks>
    /// <typeparam name="T">The type of object being built.</typeparam>
    public interface IBuilder<T>
    {
        /// <summary>
        /// Resets the builder to its initial state.
        /// </summary>
        /// <remarks>
        /// This method clears any previously set parts of the object being
        /// built, allowing the builder to start fresh for a new construction
        /// process.
        /// </remarks>
        void Reset();

        /// <summary>
        /// Builds and returns the final constructed object.
        /// </summary>
        /// <remarks>
        /// This method assembles the parts that have been set and returns
        /// the fully constructed object of type T. After calling this method,
        /// the builder may need to be reset before building another object.
        /// </remarks>
        /// <returns>The constructed object of type T.</returns>
        T Build();
    }
}
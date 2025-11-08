namespace MBUtils.DesignPatterns.Adapter
{
    /// <summary>
    /// Defines a synchronous adapter that converts an object of type <typeparamref name="TSource"/>
    /// to an object of type <typeparamref name="TTarget"/>.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object to adapt from.</typeparam>
    /// <typeparam name="TTarget">The type of the target object to adapt to.</typeparam>
    /// <remarks>
    /// The Adapter pattern allows objects with incompatible interfaces to work together.
    /// Use this interface when you need to convert between types synchronously.
    /// </remarks>
    public interface IAdapter<in TSource, out TTarget>
    {
        /// <summary>
        /// Adapts the specified source object to the target type.
        /// </summary>
        /// <param name="source">The source object to adapt.</param>
        /// <returns>An object of type <typeparamref name="TTarget"/> adapted from the source.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
        TTarget Adapt(TSource source);
    }
}
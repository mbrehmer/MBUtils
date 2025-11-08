namespace MBUtils.DesignPatterns.Adapter
{
    /// <summary>
    /// Provides an abstract base class for synchronous adapters that convert objects of type
    /// <typeparamref name="TSource"/> to objects of type <typeparamref name="TTarget"/>.
    /// </summary>
    /// <typeparam name="TSource">The type of the source object to adapt from.</typeparam>
    /// <typeparam name="TTarget">The type of the target object to adapt to.</typeparam>
    /// <remarks>
    /// <para>
    /// Inheritors must implement the <see cref="AdaptCore"/> method to define the actual
    /// adaptation logic. This base class handles null checking and delegates to the core method.
    /// </para>
    /// <para>
    /// This class explicitly implements <see cref="IAdapter{TSource, TTarget}"/> to provide
    /// a clean public API surface.
    /// </para>
    /// </remarks>
    public abstract class AdapterBase<TSource, TTarget> : IAdapter<TSource, TTarget>
    {
        /// <summary>
        /// Adapts the specified source object to the target type.
        /// </summary>
        /// <param name="source">The source object to adapt.</param>
        /// <returns>An object of type <typeparamref name="TTarget"/> adapted from the source.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="source"/> is null.</exception>
        TTarget IAdapter<TSource, TTarget>.Adapt(TSource source)
        {
            if (source == null)
            {
                throw new System.ArgumentNullException(nameof(source));
            }

            return AdaptCore(source);
        }

        /// <summary>
        /// When overridden in a derived class, performs the actual adaptation logic
        /// to convert the source object to the target type.
        /// </summary>
        /// <param name="source">The source object to adapt. Guaranteed to be non-null.</param>
        /// <returns>An object of type <typeparamref name="TTarget"/> adapted from the source.</returns>
        /// <remarks>
        /// This method is called by <see cref="IAdapter{TSource, TTarget}.Adapt"/> after null checking.
        /// Implementers can assume that <paramref name="source"/> is never null.
        /// </remarks>
        protected abstract TTarget AdaptCore(TSource source);
    }
}
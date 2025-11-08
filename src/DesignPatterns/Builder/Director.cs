using System;

namespace MBUtils.DesignPatterns.Builder
{
    /// <summary>
    /// Directs the construction process using a builder to create complex objects.
    /// </summary>
    /// <remarks>
    /// The Director class encapsulates common construction algorithms and sequences,
    /// separating the construction logic from the builder implementations. This allows
    /// for reusable construction patterns that can work with any compatible builder.
    /// </remarks>
    /// <typeparam name="TBuilder">The type of builder that constructs the product.</typeparam>
    /// <typeparam name="TProduct">The type of object being built.</typeparam>
    public class Director<TBuilder, TProduct>
        where TBuilder : class, IBuilder<TProduct>
    {
        private TBuilder? _builder;

        /// <summary>
        /// Gets or sets the builder used by this director.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when setting a null builder.</exception>
        public TBuilder Builder
        {
            get => _builder ?? throw new InvalidOperationException("Builder has not been set.");
            set => _builder = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Director{TBuilder, TProduct}"/> class.
        /// </summary>
        public Director()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Director{TBuilder, TProduct}"/> class
        /// with the specified builder.
        /// </summary>
        /// <param name="builder">The builder to use for construction.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
        public Director(TBuilder builder)
        {
            Builder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        /// <summary>
        /// Constructs an object using the specified construction algorithm.
        /// </summary>
        /// <param name="constructionAlgorithm">The algorithm that defines how to use the builder.</param>
        /// <returns>The constructed object.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="constructionAlgorithm"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when no builder has been set.</exception>
        public TProduct Construct(Action<TBuilder> constructionAlgorithm)
        {
            if (constructionAlgorithm == null)
                throw new ArgumentNullException(nameof(constructionAlgorithm));

            Builder.Reset();
            constructionAlgorithm(Builder);
            return Builder.Build();
        }

        /// <summary>
        /// Constructs an object using a predefined construction strategy.
        /// </summary>
        /// <param name="strategy">The construction strategy to apply.</param>
        /// <returns>The constructed object.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="strategy"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when no builder has been set.</exception>
        public TProduct Construct(IConstructionStrategy<TBuilder, TProduct> strategy)
        {
            if (strategy == null)
                throw new ArgumentNullException(nameof(strategy));

            Builder.Reset();
            return strategy.Construct(Builder);
        }
    }

    /// <summary>
    /// Defines a strategy for constructing objects using a builder.
    /// </summary>
    /// <typeparam name="TBuilder">The type of builder that constructs the product.</typeparam>
    /// <typeparam name="TProduct">The type of object being built.</typeparam>
    public interface IConstructionStrategy<in TBuilder, out TProduct>
        where TBuilder : IBuilder<TProduct>
    {
        /// <summary>
        /// Constructs an object using the specified builder.
        /// </summary>
        /// <param name="builder">The builder to use for construction.</param>
        /// <returns>The constructed object.</returns>
        TProduct Construct(TBuilder builder);
    }
}
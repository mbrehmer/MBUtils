using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MBUtils.DesignPatterns.Strategy
{
    /// <summary>
    /// Executes a sequence of strategies in order. The output of each strategy becomes the input
    /// (context) of the next strategy. The final result is returned.
    /// <para>
    /// This composite requires that <typeparamref name="TContext"/> and <typeparamref name="TResult"/>
    /// are the same runtime type. If they differ, an <see cref="InvalidOperationException"/> is thrown
    /// during construction.
    /// </para>
    /// </summary>
    /// <typeparam name="TContext">Type of the context.</typeparam>
    /// <typeparam name="TResult">Type of the result.</typeparam>
    public sealed class CompositeStrategy<TContext, TResult> : IStrategy<TContext, TResult>
    {
        private readonly IReadOnlyCollection<IStrategy<TContext, TResult>> _strategies;
        private readonly bool _typesAreCompatible;

        /// <summary>
        /// Initializes a new instance of <see cref="CompositeStrategy{TContext, TResult}"/>.
        /// </summary>
        /// <param name="strategies">The strategies to execute in order. Must not be null or empty.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="strategies"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="strategies"/> is empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <typeparamref name="TContext"/> and <typeparamref name="TResult"/> are not the same runtime type.</exception>
        public CompositeStrategy(IReadOnlyCollection<IStrategy<TContext, TResult>> strategies)
        {
            if (strategies is null)
            {
                throw new ArgumentNullException(nameof(strategies));
            }

            if (strategies.Count == 0)
            {
                throw new ArgumentException("At least one strategy must be provided.", nameof(strategies));
            }

            _strategies = strategies;

            // Ensure we can safely feed results back as the next context without unsafe casts at runtime.
            _typesAreCompatible = typeof(TContext) == typeof(TResult);
            if (!_typesAreCompatible)
            {
                throw new InvalidOperationException("CompositeStrategy requires TContext and TResult to be the same type to chain outputs as inputs.");
            }
        }

        /// <summary>
        /// Executes all configured strategies in order, passing the result of each as the next context,
        /// and returns the final result.
        /// </summary>
        /// <param name="context">The initial context for execution.</param>
        /// <returns>The final result after executing all strategies.</returns>
        TResult IStrategy<TContext, TResult>.Execute(TContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // With the constructor check, this cast is safe at runtime.
            TContext currentContext = context;
            TResult lastResult = default!;

            foreach (IStrategy<TContext, TResult> strategy in _strategies)
            {
                lastResult = strategy.Execute(currentContext);
                // Feed output as next input
                currentContext = (TContext)(object)lastResult!;
            }

            return lastResult;
        }

        /// <summary>
        /// Asynchronously executes all configured strategies in order, passing the result of each as the next context,
        /// and returns the final result.
        /// </summary>
        /// <param name="context">The initial context for execution.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task that yields the final result after executing all strategies.</returns>
        async Task<TResult> IStrategy<TContext, TResult>.ExecuteAsync(TContext context, CancellationToken cancellationToken)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            TContext currentContext = context;
            TResult lastResult = default!;

            foreach (IStrategy<TContext, TResult> strategy in _strategies)
            {
                lastResult = await strategy.ExecuteAsync(currentContext, cancellationToken).ConfigureAwait(false);
                currentContext = (TContext)(object)lastResult!;
            }

            return lastResult;
        }
    }
}
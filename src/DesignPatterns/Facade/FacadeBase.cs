namespace MBUtils.DesignPatterns.Facade
{
    /// <summary>
    /// Base class easing implementation of <see cref="IFacade"/>.
    /// Implements the public interface explicitly and exposes protected core methods for concrete types.
    /// </summary>
    /// <remarks>
    /// Inherit from this class to create a facade that simplifies interaction with a complex subsystem.
    /// Override <see cref="ExecuteCore"/> to implement the coordination logic.
    /// </remarks>
    public abstract class FacadeBase : IFacade
    {
        /// <summary>
        /// Executes the core facade logic, coordinating calls to the underlying subsystem.
        /// </summary>
        /// <remarks>
        /// Implement this method to define how the facade interacts with the subsystem components.
        /// This method should encapsulate the complexity and provide a simplified workflow.
        /// </remarks>
        protected abstract void ExecuteCore();

        /// <inheritdoc />
        void IFacade.Execute()
        {
            ExecuteCore();
        }
    }

    /// <summary>
    /// Base class easing implementation of <see cref="IFacade{TResult}"/>.
    /// Implements the public interface explicitly and exposes protected core methods for concrete types.
    /// </summary>
    /// <typeparam name="TResult">The type of the result returned by the facade operation.</typeparam>
    /// <remarks>
    /// Inherit from this class to create a facade that simplifies interaction with a complex subsystem
    /// and returns a result. Override <see cref="ExecuteCore"/> to implement the coordination logic.
    /// </remarks>
    public abstract class FacadeBase<TResult> : IFacade<TResult>
    {
        /// <summary>
        /// Executes the core facade logic, coordinating calls to the underlying subsystem.
        /// </summary>
        /// <returns>The result of the facade operation.</returns>
        /// <remarks>
        /// Implement this method to define how the facade interacts with the subsystem components.
        /// This method should encapsulate the complexity and provide a simplified workflow.
        /// </remarks>
        protected abstract TResult ExecuteCore();

        /// <inheritdoc />
        TResult IFacade<TResult>.Execute()
        {
            return ExecuteCore();
        }
    }
}
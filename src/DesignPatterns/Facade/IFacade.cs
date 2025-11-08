namespace MBUtils.DesignPatterns.Facade
{
    /// <summary>
    /// Defines a synchronous facade that provides a simplified interface to a complex subsystem.
    /// </summary>
    /// <remarks>
    /// The Facade pattern provides a unified, simplified interface to a set of interfaces in a subsystem.
    /// It defines a higher-level interface that makes the subsystem easier to use by hiding its complexity.
    /// Use this interface when you need to provide a simple entry point to a complex system.
    /// </remarks>
    public interface IFacade
    {
        /// <summary>
        /// Executes the facade operation, coordinating calls to the underlying subsystem.
        /// </summary>
        /// <remarks>
        /// This method encapsulates the complexity of interacting with multiple subsystem components,
        /// presenting a single, simple operation to the client.
        /// </remarks>
        void Execute();
    }

    /// <summary>
    /// Defines a synchronous facade that provides a simplified interface to a complex subsystem
    /// and returns a result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result returned by the facade operation.</typeparam>
    /// <remarks>
    /// The Facade pattern provides a unified, simplified interface to a set of interfaces in a subsystem.
    /// It defines a higher-level interface that makes the subsystem easier to use by hiding its complexity.
    /// Use this interface when you need to provide a simple entry point to a complex system that produces a result.
    /// </remarks>
    public interface IFacade<out TResult>
    {
        /// <summary>
        /// Executes the facade operation, coordinating calls to the underlying subsystem.
        /// </summary>
        /// <returns>The result of the facade operation.</returns>
        /// <remarks>
        /// This method encapsulates the complexity of interacting with multiple subsystem components,
        /// presenting a single, simple operation to the client.
        /// </remarks>
        TResult Execute();
    }
}
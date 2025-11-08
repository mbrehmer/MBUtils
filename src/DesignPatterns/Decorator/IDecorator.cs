namespace MBUtils.DesignPatterns.Decorator
{
    /// <summary>
    /// Represents a decorator that wraps a component and adds additional behavior.
    /// </summary>
    /// <typeparam name="T">The type of the component being decorated.</typeparam>
    public interface IDecorator<T>
    {
        /// <summary>
        /// Gets the wrapped component.
        /// </summary>
        T Component { get; }

        /// <summary>
        /// Executes the decorator's behavior, typically wrapping the component's behavior.
        /// </summary>
        /// <returns>The result after applying the decorator's behavior.</returns>
        T Execute();
    }
}
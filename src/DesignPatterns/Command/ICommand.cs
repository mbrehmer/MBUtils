namespace MBUtils.DesignPatterns.Command
{
    /// <summary>
    /// Represents an executable command.
    /// </summary>
    /// <remarks>
    /// Implementations encapsulate an action and any necessary data so the
    /// action can be invoked through a common interface. This supports
    /// patterns such as queuing, logging, undo/redo, and remote execution.
    /// </remarks>
    public interface ICommand
    {
        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <remarks>
        /// When called, the implementation performs the command's operation.
        /// Implementations may throw exceptions to indicate execution failures.
        /// If commands may be retried, consider designing implementations to be
        /// idempotent or to safely handle repeated invocations.
        /// </remarks>
        void Execute();
    }
}
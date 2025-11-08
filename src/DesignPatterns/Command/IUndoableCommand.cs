namespace MBUtils.DesignPatterns.Command
{
    /// <summary>
    /// Represents a command that can be executed and undone.
    /// </summary>
    /// <remarks>
    /// Extends <see cref="ICommand"/> to allow reversal of the command's effects.
    /// Implementations should ensure that Undo restores the state to what it was
    /// before Execute was called.
    /// </remarks>
    public interface IUndoableCommand : ICommand
    {
        /// <summary>
        /// Reverts the command's operation.
        /// </summary>
        /// <remarks>
        /// When called, the implementation should undo the effect of the last Execute call.
        /// Implementations may throw exceptions if undo is not possible.
        /// </remarks>
        void Undo();
    }
}
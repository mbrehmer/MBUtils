using System.Threading.Tasks;

namespace MBUtils.DesignPatterns.Command
{
    /// <summary>
    /// Represents an asynchronously executable command that can also be undone.
    /// </summary>
    public interface IAsyncUndoableCommand : IAsyncCommand
    {
        /// <summary>
        /// Reverts the command's operation asynchronously.
        /// </summary>
        /// <returns>A task that represents the undo operation.</returns>
        Task UndoAsync();
    }
}
using System.Threading.Tasks;

namespace MBUtils.DesignPatterns.Command
{
    /// <summary>
    /// Represents an asynchronously executable command.
    /// </summary>
    public interface IAsyncCommand : ICommand
    {
        /// <summary>
        /// Executes the command asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ExecuteAsync();
    }
}
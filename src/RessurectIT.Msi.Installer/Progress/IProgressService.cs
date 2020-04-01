using System.Threading.Tasks;

namespace RessurectIT.Msi.Installer.Progress
{
    /// <summary>
    /// Used for displaying progress indicator
    /// </summary>
    public interface IProgressService
    {
        #region methods

        /// <summary>
        /// Shows progress message
        /// </summary>
        /// <param name="message">Message to be displayed</param>
        /// <param name="updateId">Id of update that is being updated</param>
        Task ShowProgressMessage(string message, string updateId);
        #endregion
    }
}
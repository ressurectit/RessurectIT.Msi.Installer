using System;

namespace RessurectIT.Msi.Installer.Progress
{
    /// <summary>
    /// Used for displaying progress window
    /// </summary>
    public interface IProgressWindow : IDisposable
    {
        #region methods

        /// <summary>
        /// Shows progress message in window
        /// </summary>
        /// <param name="message">Message to be displayed</param>
        /// <param name="updateId">Id of update that is being updated</param>
        void ShowProgressMessage(string message, string updateId);
        #endregion
    }
}
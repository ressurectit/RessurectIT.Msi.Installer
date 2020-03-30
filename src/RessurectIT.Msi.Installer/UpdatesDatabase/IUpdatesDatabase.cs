using System.Collections.Generic;
using RessurectIT.Msi.Installer.Installer.Dto;
using RessurectIT.Msi.Installer.UpdatesDatabase.Dto;

namespace RessurectIT.Msi.Installer.UpdatesDatabase
{
    /// <summary>
    /// Used for managing installed updates database
    /// </summary>
    public interface IUpdatesDatabase
    {
        #region methods

        /// <summary>
        /// Gets installed updates object
        /// </summary>
        /// <returns>Gets information about installed updates</returns>
        Dictionary<string, InstalledUpdateInfo> GetInstalledUpdates();        

        /// <summary>
        /// Sets installed update to stored json
        /// </summary>
        /// <param name="update">Information about installed update</param>
        void SetInstalledUpdates(IMsiUpdate update);
        #endregion
    }
}
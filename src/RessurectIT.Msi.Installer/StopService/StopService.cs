using System;

namespace RessurectIT.Msi.Installer.StopService
{
    /// <summary>
    /// Service used for stopping service in case of self update
    /// </summary>
    public class StopService
    {
        #region public methods

        /// <summary>
        /// Gets or sets stop callback used for stopping service
        /// </summary>
        public Action? StopCallback
        {
            get;
            set;
        }
        #endregion
    }
}
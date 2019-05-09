using System;
using Serilog;

namespace RessurectIT.Msi.Installer.Gatherer.Dto
{
    /// <summary>
    /// Information about installed update
    /// </summary>
    public class InstalledUpdateInfo
    {
        #region internal properties

        /// <summary>
        /// Gets version as object representing version
        /// </summary>
        internal Version VersionObj
        {
            get
            {
                try
                {
                    return new Version(Version);
                }
                catch (Exception e)
                {
                    Log.Warning(e,$"Version '{Version}' for '{ProductCode}' is in incorrect format!");

                    return new Version("0.0.0.0");
                }
            }
        }
        #endregion


        #region public properties

        /// <summary>
        /// Gets or sets last installed version
        /// </summary>
        public string Version
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets product code of last installed version
        /// </summary>
        public string ProductCode
        {
            get;
            set;
        }
        #endregion
    }
}
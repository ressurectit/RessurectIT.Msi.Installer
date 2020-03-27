namespace RessurectIT.Msi.Installer.Configuration
{
    /// <summary>
    /// Configuration for installer exe
    /// </summary>
    internal class AppConfig : ConfigBase
    {
        #region public properties

        /// <summary>
        /// Gets or sets request uri containing encoded msi update request
        /// </summary>
        public string Request
        {
            get;
            set;
        }
        #endregion
    }
}
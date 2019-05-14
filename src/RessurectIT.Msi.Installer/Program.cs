using System.ServiceProcess;

namespace RessurectIT.Msi.Installer
{
    /// <summary>
    /// Main entry for application
    /// </summary>
    public static class Program
    {
        #region public methods

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static void Main()
        {
            ServiceBase serviceToRun = new RessurectITMsiInstallerService();
            ServiceBase.Run(serviceToRun);
        }
        #endregion
    }
}

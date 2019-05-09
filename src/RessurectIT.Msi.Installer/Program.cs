using System.Reflection;
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
            //ServiceBase serviceToRun = new RessurectITMsiInstallerService();
            //ServiceBase.Run(serviceToRun);

            using (RessurectITMsiInstallerService svc = new RessurectITMsiInstallerService())
            {
                svc.GetType().GetMethod("OnStart", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(svc, new object[] {new string[0]});
            }
        }
        #endregion
    }
}

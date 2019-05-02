using System.ComponentModel;
using System.ServiceProcess;

namespace RessurectIT.Msi.Installer
{
    /// <summary>
    /// Installer for <see cref="RessurectITMsiInstallerService"/>
    /// </summary>
    [RunInstaller(true)]
    public class RessurectITMsiInstallerInstaller : System.Configuration.Install.Installer
    {
        #region private fields

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer _components;

        /// <summary>
        /// Service process installer
        /// </summary>
        private ServiceProcessInstaller _serviceProcessInstaller;
        
        /// <summary>
        /// Service installer
        /// </summary>
        private ServiceInstaller _serviceInstaller;
        #endregion


        #region constructors

        /// <summary>
        /// Creates instance of <see cref="RessurectITMsiInstallerInstaller"/>
        /// </summary>
        public RessurectITMsiInstallerInstaller()
        {
            InitializeComponent();
        }
        #endregion


        #region protected methods - overriden methods
        
        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _components?.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion


        #region private methods

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            _components = new Container();

            _serviceProcessInstaller = new ServiceProcessInstaller();
            _serviceInstaller = new ServiceInstaller();
            _serviceProcessInstaller.Account = ServiceAccount.User;
            _serviceInstaller.Description = "Automated MSI installer service.";
            _serviceInstaller.DisplayName = "RessurectIT MSI Installer";
            _serviceInstaller.ServiceName = "RessurectIT.Msi.Installer";
            Installers.AddRange(new System.Configuration.Install.Installer[]
            {
                _serviceProcessInstaller, _serviceInstaller
            });
        }
        #endregion
    }
}

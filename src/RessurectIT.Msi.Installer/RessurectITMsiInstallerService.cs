using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;
using System.ServiceProcess;
using RessurectIT.Msi.Installer.Checker;
using RessurectIT.Msi.Installer.Configuration;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace RessurectIT.Msi.Installer
{
    /// <summary>
    /// Windows service that is used for installing msi installers
    /// </summary>
    public class RessurectITMsiInstallerService : ServiceBase
    {
        #region private fields

        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private IContainer components;

        /// <summary>
        /// Update checker used for checking for updates
        /// </summary>
        private UpdateChecker _checker;
        #endregion


        #region public properties

        /// <summary>
        /// Gets instance of configuration for application
        /// </summary>
        internal static Config Config
        {
            get;
            private set;
        }
        #endregion


        #region constructor

        /// <summary>
        /// Creates instance of <see cref="RessurectITMsiInstallerService"/>
        /// </summary>
        public RessurectITMsiInstallerService()
        {
            InitializeComponent();
        }
        #endregion


        #region protected methods - overriden methods

        /// <summary>
        /// When implemented in a derived class, executes when a Start command is sent to the service by the Service Control Manager (SCM) or when the operating system starts (for a service that starts automatically). Specifies actions to take when the service starts.
        /// </summary>
        /// <param name="args">Data passed by the start command. </param>
        protected override void OnStart(string[] args)
        {
            Debugger.Launch();

            if (!IsAdministrator())
            {
                Log.Error($"User running this service '{Constants.ServiceName}' is not administrator!");

                Stop();

                return;
            }

            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            // ReSharper disable once AssignNullToNotNullAttribute
            Environment.CurrentDirectory = Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));

            IConfigurationRoot appConfig = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("RessurectIT.Msi.Installer.config.json", false, true)
#if DEBUG
                .AddJsonFile("RessurectIT.Msi.Installer.config.dev.json", true, true)
#endif
                .AddEnvironmentVariables("RESSURECTIT_MSI_INSTALLER")
                .AddCommandLine(args)
                .Build();

            Config config = Config = new Config();

            appConfig.Bind(config);

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(appConfig)
                .CreateLogger();

            Log.Information($"Service '{Constants.ServiceName}' is starting!");
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            _checker = new UpdateChecker();
            _checker.Start();
        }

        /// <summary>
        /// When implemented in a derived class, executes when a Stop command is sent to the service by the Service Control Manager (SCM). Specifies actions to take when a service stops running.
        /// </summary>
        protected override void OnStop()
        {
            Log.Information($"Service '{Constants.ServiceName}' is stopping!");
        }

        /// <summary>
        /// When implemented in a derived class, executes when a Pause command is sent to the service by the Service Control Manager (SCM). Specifies actions to take when a service pauses.
        /// </summary>
        protected override void OnPause()
        {
            OnStop();
        }

        /// <summary>
        /// When implemented in a derived class, <see cref="M:System.ServiceProcess.ServiceBase.OnContinue"/> runs when a Continue command is sent to the service by the Service Control Manager (SCM). Specifies actions to take when a service resumes normal functioning after being paused.
        /// </summary>
        protected override void OnContinue()
        {
            OnStart(null);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
                _checker?.Dispose();
                _checker = null;
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
            components = new Container();
            ServiceName = Constants.ServiceName;
        }

        /// <summary>
        /// Handles unhandled exceptions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Error(e.ExceptionObject as Exception, $"Unhandled exception occured in service '{Constants.ServiceName}'");

            OnStop();
        }

        /// <summary>
        /// Tests whether user that is running application has administrator role
        /// </summary>
        /// <returns>True if user has administrator role, otherwise false</returns>
        private bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);

            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        #endregion
    }
}

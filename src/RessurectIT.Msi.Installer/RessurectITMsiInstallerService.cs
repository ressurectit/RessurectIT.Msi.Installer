using System;
using System.ComponentModel;
using System.ServiceProcess;

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
            // Logger.LogMessage("Service is starting!", SharedConstants.TaskQueueProcessorName);
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        }

        /// <summary>
        /// When implemented in a derived class, executes when a Stop command is sent to the service by the Service Control Manager (SCM). Specifies actions to take when a service stops running.
        /// </summary>
        protected override void OnStop()
        {
            // Logger.LogMessage("Service is stopping!", SharedConstants.TaskQueueProcessorName);
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
            // Logger.LogException(e.ExceptionObject as Exception, SharedConstants.TaskQueueProcessorName);
            OnStop();
        }
        #endregion
    }
}

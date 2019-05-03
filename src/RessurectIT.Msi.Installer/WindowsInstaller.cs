using System;
using System.Diagnostics;
using System.IO;
using WindowsInstaller;
using MsiInstaller = WindowsInstaller.Installer;

namespace RessurectIT.Msi.Installer
{
    /// <summary>
    /// Class used for installing msi and reading data from them
    /// </summary>
    internal class WindowsInstaller
    {
        #region private fields

        /// <summary>
        /// Path to msi to be worked with
        /// </summary>
        private readonly string _msiPath;
        #endregion


        #region constructors

        /// <summary>
        /// Creates instance of <see cref="WindowsInstaller"/>
        /// </summary>
        /// <param name="msiPath">Path to msi to be worked with</param>
        public WindowsInstaller(string msiPath)
        {
            _msiPath = msiPath;
        }
        #endregion


        #region public methods

        /// <summary>
        /// Installs MSI that is specified by this <see cref="WindowsInstaller"/>
        /// </summary>
        public void Install()
        {
            try
            {
                Process process = new Process
                {
                    StartInfo =
                    {
                        FileName = "msiexec",
                        WorkingDirectory = Directory.GetCurrentDirectory(),
                        Arguments = $" /quiet /i {_msiPath} /L*V \"C:\\example.log\" HPRO_PLUGIN=1"
                    }
                };

                //process.StartInfo.WorkingDirectory = @"C:\temp\";
                process.Start();
                process.WaitForExit(60000);
            }
            catch (Exception e)
            {
                
            }
        }

        /// <summary>
        /// Checks whether is provided msi current application msi
        /// </summary>
        /// <returns>True if provided MSI is for RessurectIT.Msi.Installer</returns>
        public bool IsRessurectITMsiInstallerMsi()
        {
            string upgradeCode = GetMsiProperty("UpgradeCode");

            return "B074C4B8-5B0A-4221-83B3-7AEECB00FD61" == upgradeCode;
        }

        /// <summary>
        /// Gets version of MSI for this <see cref="WindowsInstaller"/> instance
        /// </summary>
        /// <returns>String number of version</returns>
        public string GetMsiVersion()
        {
            return GetMsiProperty("ProductVersion");
        }
        #endregion


        #region private methods

        /// <summary>
        /// Gets value of MSI property as string
        /// </summary>
        /// <param name="propertyName">Name of MSI property to be read</param>
        /// <returns>String representing MSI property value</returns>
        private string GetMsiProperty(string propertyName)
        {
            Type type = Type.GetTypeFromProgID("WindowsInstaller.Installer");
            MsiInstaller installer = (MsiInstaller)Activator.CreateInstance(type);

            Database db = installer.OpenDatabase(_msiPath, 0);
            View dv = db.OpenView($"SELECT `Value` FROM `Property` WHERE `Property`='{propertyName}'");
            dv.Execute();
            Record record = dv.Fetch();

            return record?.StringData[1];
        }        
        #endregion
    }
}
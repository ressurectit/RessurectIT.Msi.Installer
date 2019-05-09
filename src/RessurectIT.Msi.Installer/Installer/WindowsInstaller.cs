using System;
using System.Diagnostics;
using System.IO;
using WindowsInstaller;
using Serilog;
using MsiInstaller = WindowsInstaller.Installer;

namespace RessurectIT.Msi.Installer.Installer
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

        /// <summary>
        /// Product code that is used for uninstalling of previous version, GUID, not required
        /// </summary>
        private readonly string _productCode;
        #endregion


        #region constructors

        /// <summary>
        /// Creates instance of <see cref="WindowsInstaller"/>
        /// </summary>
        /// <param name="msiPath">Path to msi to be worked with</param>
        /// <param name="productCode">Product code that is used for uninstalling of previous version, GUID, not required</param>
        public WindowsInstaller(string msiPath, string productCode)
        {
            _msiPath = msiPath;
            _productCode = productCode;
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
                        Arguments = $" /q /i {_msiPath} /L*V \"C:\\example.log\" HPRO_PLUGIN=1"
                    }
                };

                process.Start();
                process.WaitForExit(90000);
            }
            catch (Exception e)
            {
                
            }
        }

        /// <summary>
        /// Uninstalls ProductCode that is specified by this <see cref="WindowsInstaller"/>, if no product code was specified, does nothing
        /// </summary>
        public void Uninstall()
        {
            if (string.IsNullOrEmpty(_productCode))
            {
                Log.Information("No product code was specified");

                return;
            }

            try
            {
                Process process = new Process
                {
                    StartInfo =
                    {
                        FileName = "msiexec",
                        WorkingDirectory = Directory.GetCurrentDirectory(),
                        Arguments = $" /q /x {{{_productCode}}} /L*V \"C:\\example.log\" HPRO_PLUGIN=1"
                    }
                };

                process.Start();
                process.WaitForExit(90000);
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
            string upgradeCode = GetMsiProperty("UpgradeCode", _msiPath);

            return "B074C4B8-5B0A-4221-83B3-7AEECB00FD61" == upgradeCode;
        }

        /// <summary>
        /// Gets version of MSI for this <see cref="WindowsInstaller"/> instance
        /// </summary>
        /// <returns>String number of version</returns>
        public string GetMsiVersion()
        {
            return GetMsiProperty("ProductVersion", _msiPath);
        }

        /// <summary>
        /// Gets product code from msi file
        /// </summary>
        /// <param name="msiPath">Path to msi file from which product code will be obtained</param>
        /// <returns>Obtained product code</returns>
        public static string GetProductCode(string msiPath)
        {
            return GetMsiProperty("ProductCode", msiPath);
        }
        #endregion


        #region private methods

        /// <summary>
        /// Gets value of MSI property as string
        /// </summary>
        /// <param name="propertyName">Name of MSI property to be read</param>
        /// <param name="msiPath">Path to msi file from which obtain property value</param>
        /// <returns>String representing MSI property value</returns>
        private static string GetMsiProperty(string propertyName, string msiPath)
        {
            Type type = Type.GetTypeFromProgID("WindowsInstaller.Installer");
            MsiInstaller installer = (MsiInstaller)Activator.CreateInstance(type);

            Database db = installer.OpenDatabase(msiPath, 0);
            View dv = db.OpenView($"SELECT `Value` FROM `Property` WHERE `Property`='{propertyName}'");
            dv.Execute();
            Record record = dv.Fetch();

            return record?.StringData[1];
        }        
        #endregion
    }
}
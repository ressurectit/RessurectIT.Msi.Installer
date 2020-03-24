﻿using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using WindowsInstaller;
using RessurectIT.Msi.Installer.Gatherer.Dto;
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
        /// Information about update that should be installed
        /// </summary>
        private readonly MsiUpdate _update;

        /// <summary>
        /// Callback called when there is need to stop installer itself
        /// </summary>
        private readonly Action _stopCallback;
        #endregion


        #region constructors

        /// <summary>
        /// Creates instance of <see cref="WindowsInstaller"/>
        /// </summary>
        /// <param name="update">Information about update that should be installed</param>
        /// <param name="stopCallback">Callback called when there is need to stop installer itself</param>
        public WindowsInstaller(MsiUpdate update, Action stopCallback)
        {
            _update = update;
            _stopCallback = stopCallback;
        }
        #endregion


        #region public methods

        /// <summary>
        /// Installs MSI that is specified by this <see cref="WindowsInstaller"/>
        /// </summary>
        public void Install()
        {
            string logPath = Path.Combine(Directory.GetCurrentDirectory(), "msiexec-install.log");

            try
            {
                //self update and same or older version
                if (IsRessurectITMsiInstallerMsi() && Assembly.GetExecutingAssembly().GetName().Version >= new Version(_update.Version))
                {
                    return;
                }

                Process process = new Process
                {
                    StartInfo =
                    {
                        FileName = "msiexec",
                        Arguments = $" /q /i {_update.MsiPath} /L*V \"{logPath}\" {_update.InstallParameters}"
                    }
                };

                process.Start();

                //self update initiated
                if (IsRessurectITMsiInstallerMsi())
                {
                    _stopCallback();
                    Process.GetCurrentProcess().Kill();
                }

                process.WaitForExit(90000);

                if (process.ExitCode != 0)
                {
                    throw new InstallationException($"Failed to install product! Process exited with code {process.ExitCode}!");
                }
            }
            catch (InstallationException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new InstallationException("Failed to install product!", e);
            }
            finally
            {
                LogLog(logPath);
            }
        }

        /// <summary>
        /// Uninstalls ProductCode that is specified by this <see cref="WindowsInstaller"/>, if no product code was specified, does nothing
        /// </summary>
        public void Uninstall()
        {
            if (string.IsNullOrEmpty(_update.UninstallProductCode))
            {
                Log.Information("No product code was specified");

                return;
            }

            string logPath = Path.Combine(Directory.GetCurrentDirectory(), "msiexec-uninstall.log");

            try
            {
                Process process = new Process
                {
                    StartInfo =
                    {
                        FileName = "msiexec",
                        Arguments = $" /q /x {_update.UninstallProductCode} /L*V \"{logPath}\" {_update.UninstallParameters}"
                    }
                };

                process.Start();
                process.WaitForExit(90000);

                if (process.ExitCode != 0)
                {
                    Log.Error($"Failed to uninstall product! Process exited with code {process.ExitCode}! Machine: '{{MachineName}}'");
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to uninstall product! Machine: '{MachineName}'");
            }
            finally
            {
                LogLog(logPath);
            }
        }

        /// <summary>
        /// Checks whether is provided msi current application msi
        /// </summary>
        /// <returns>True if provided MSI is for RessurectIT.Msi.Installer</returns>
        public bool IsRessurectITMsiInstallerMsi()
        {
            string upgradeCode = GetMsiProperty("UpgradeCode", _update.MsiPath);

            return "{369C83BF-7965-4AEE-B1DD-329BEB92D719}" == upgradeCode;
        }

        /// <summary>
        /// Gets version of MSI for this <see cref="WindowsInstaller"/> instance
        /// </summary>
        /// <param name="msiPath">Path to msi file from which product version will be obtained</param>
        /// <returns>String number of version</returns>
        public static string GetMsiVersion(string msiPath)
        {
            return GetMsiProperty("ProductVersion", msiPath);
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

        /// <summary>
        /// Logs msiexec install/uninstall log
        /// </summary>
        /// <param name="logPath">Path to log to be logged</param>
        private void LogLog(string logPath)
        {
            try
            {
                if (File.Exists(logPath))
                {
                    string contents = File.ReadAllText(logPath);

                    File.Delete(logPath);

                    Log.Information($"Machine: '{{MachineName}}' MSIEXEC LOG: {contents}");
                }
                else
                {
                    Log.Error("Failed to obtain msiexec log! No log found. Machine: '{MachineName}'");
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to obtain msiexec log! Machine: '{MachineName}'");
            }
        }
        #endregion
    }
}
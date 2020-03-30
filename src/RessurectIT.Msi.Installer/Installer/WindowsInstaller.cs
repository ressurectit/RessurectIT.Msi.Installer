using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Reflection;
using DryIocAttributes;
using Microsoft.Extensions.Logging;
using RessurectIT.Msi.Installer.Installer.Dto;

namespace RessurectIT.Msi.Installer.Installer
{
    /// <summary>
    /// Class used for installing msi and reading data from them
    /// </summary>
    [ExportEx]
    [CurrentScopeReuse]
    public class WindowsInstaller
    {
        #region private fields

        /// <summary>
        /// Logger used for logging
        /// </summary>
        private readonly ILogger<WindowsInstaller> _logger;        
        #endregion
        

        #region constructors

        /// <summary>
        /// Creates instance of <see cref="WindowsInstaller"/>
        /// </summary>
        /// <param name="logger">Logger used for logging</param>
        public WindowsInstaller(ILogger<WindowsInstaller> logger)
        {
            _logger = logger;
        }
        #endregion


        #region public methods

        /// <summary>
        /// Installs MSI that is specified by this <see cref="WindowsInstaller"/>
        /// </summary>
        /// <param name="update">Information about update that should be installed</param>
        /// <param name="stopCallback">Callback called when there is need to stop installer itself</param>
        public void Install(IMsiUpdate update, Action stopCallback)
        {
            string logPath = Path.Combine(Directory.GetCurrentDirectory(), "msiexec-install.log");

            try
            {
                //self update and same or older version
                if (IsRessurectITMsiInstallerMsi(update) && Assembly.GetExecutingAssembly().GetName().Version >= new Version(update.Version))
                {
                    return;
                }

                Process process = new Process
                {
                    StartInfo =
                    {
                        FileName = "msiexec",
                        Arguments = $" /q /i {update.MsiPath} /L*V \"{logPath}\" {update.InstallParameters}"
                    }
                };

                process.Start();

                //self update initiated
                if (IsRessurectITMsiInstallerMsi(update))
                {
                    stopCallback();
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
        /// <param name="update">Information about update that should be installed</param>
        public void Uninstall(IMsiUpdate update)
        {
            if (string.IsNullOrEmpty(update.UninstallProductCode))
            {
                _logger.LogInformation("No product code was specified");

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
                        Arguments = $" /q /x {update.UninstallProductCode} /L*V \"{logPath}\" {update.UninstallParameters}"
                    }
                };

                process.Start();
                process.WaitForExit(90000);

                if (process.ExitCode != 0)
                {
                    _logger.LogError($"Failed to uninstall product! Process exited with code {process.ExitCode}! Machine: '{{MachineName}}'");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to uninstall product! Machine: '{MachineName}'");
            }
            finally
            {
                LogLog(logPath);
            }
        }

        /// <summary>
        /// Checks whether is provided msi current application msi
        /// </summary>
        /// <param name="update">Information about update that should be installed</param>
        /// <returns>True if provided MSI is for RessurectIT.Msi.Installer</returns>
        public bool IsRessurectITMsiInstallerMsi(IMsiUpdate update)
        {
            string upgradeCode = GetMsiProperty("UpgradeCode", update.MsiPath);

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
            using PowerShell ps = PowerShell.Create();

            Collection<PSObject> result = ps.AddScript(@$"
            function Get-Property ($Object, $PropertyName, [object[]]$ArgumentList) 
            {{
                return $Object.GetType().InvokeMember($PropertyName, 'Public, Instance, GetProperty', $null, $Object, $ArgumentList)
            }}

            function Invoke-Method ($Object, $MethodName, $ArgumentList) 
            {{
                return $Object.GetType().InvokeMember($MethodName, 'Public, Instance, InvokeMethod', $null, $Object, $ArgumentList)
            }}

            $ErrorActionPreference = 'Stop'
            Set-StrictMode -Version Latest

            $msiOpenDatabaseModeReadOnly = 0
            $Installer = New-Object -ComObject WindowsInstaller.Installer
            $Database = Invoke-Method $Installer OpenDatabase  @(""{msiPath}"", $msiOpenDatabaseModeReadOnly)
            $View = Invoke-Method $Database OpenView  @(""SELECT Value FROM Property WHERE Property='{propertyName}'"")

            Invoke-Method $View Execute
            $Record = Invoke-Method $View Fetch

            if ($Record) 
            {{
                Write-Output (Get-Property $Record StringData 1)
            }}").Invoke();

            return result[1].ToString();
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

                    _logger.LogInformation($"Machine: '{{MachineName}}' MSIEXEC LOG: {contents}");
                }
                else
                {
                    _logger.LogError("Failed to obtain msiexec log! No log found. Machine: '{MachineName}'");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to obtain msiexec log! Machine: '{MachineName}'");
            }
        }
        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Deployment.WindowsInstaller;

namespace RessurectIT.Msi.Installer.Installer.CustomActions
{
    /// <summary>
    /// Class that holds custom actions
    /// </summary>
    public class CustomActions
    {
        #region constants - shared

        /// <summary>
        /// Name of msi property storing path to config file
        /// </summary>
        private const string ConfigFolder = "INSTALLFOLDER";
        #endregion


        #region constants - UpdateConfig

        /// <summary>
        /// Name of UPDATE_JSON_URL property
        /// </summary>
        private const string PropUpdatesJsonUrl = "UPDATE_JSON_URL";

        /// <summary>
        /// Name of CHECK_INTERVAL property
        /// </summary>
        private const string PropCheckInterval = "CHECK_INTERVAL";

        /// <summary>
        /// Name of REMOTE_LOG_REST_URL property
        /// </summary>
        private const string PropRemoteLogRestUrl = "REMOTE_LOG_REST_URL";

        /// <summary>
        /// Name of ALLOW_SAME_VERSION property
        /// </summary>
        private const string PropAllowSameVersion = "ALLOW_SAME_VERSION";
        #endregion


        #region constants - UpdateConfigPrepareData

        /// <summary>
        /// Name of UpdateConfig property
        /// </summary>
        private const string PropUpdateConfig = "UpdateConfig";
        #endregion


        #region public methods - UpdateConfig

        /// <summary>
        /// Updates configuration file
        /// </summary>
        /// <param name="session">Session of current installer</param>
        /// <returns>Indication that operation was successful or not</returns>
        [CustomAction]
        public static ActionResult UpdateConfig(Session session)
        {
            string configPath = $"{session.CustomActionData[ConfigFolder]}RessurectIT.Msi.Installer.config.json";
            string updatesJsonUrl = session.CustomActionData[PropUpdatesJsonUrl];
            string checkInterval = session.CustomActionData[PropCheckInterval];
            string remoteLogRestUrl = session.CustomActionData[PropRemoteLogRestUrl];
            string allowSameVersion = session.CustomActionData[PropAllowSameVersion];

            session.Log("Begin UpdateConfig");
            session.Log($"Configuration path is '{configPath}'");
            session.Log($"Update json URL: '{updatesJsonUrl}'");
            session.Log($"Check interval: '{checkInterval}'");
            session.Log($"Remote log rest URL: '{remoteLogRestUrl}'");

            try
            {
                if (!File.Exists(configPath))
                {
                    session.Log($"Missing configuration file '{configPath}'");

                    return ActionResult.Failure;
                }

                string configContent = File.ReadAllText(configPath);

                if (!string.IsNullOrEmpty(updatesJsonUrl))
                {
                    session.Log($"Setting update json URL to value '{updatesJsonUrl}'");

                    configContent = Regex.Replace(configContent, @"""UpdatesJsonUrl"": "".*?""", $@"""UpdatesJsonUrl"": ""{updatesJsonUrl}""", RegexOptions.Singleline);
                }

                if (!string.IsNullOrEmpty(checkInterval))
                {
                    session.Log($"Setting check interval to value '{checkInterval}'");

                    configContent = Regex.Replace(configContent, @"""CheckInterval"": .*?,", $@"""CheckInterval"": {checkInterval},", RegexOptions.Singleline);
                }

                if (!string.IsNullOrEmpty(remoteLogRestUrl))
                {
                    session.Log($"Setting remote log rest URL to value '{remoteLogRestUrl}'");

                    configContent = Regex.Replace(configContent, @"""RemoteLogRestUrl"": "".*?""", $@"""RemoteLogRestUrl"": ""{remoteLogRestUrl}""", RegexOptions.Singleline);
                }

                if (!string.IsNullOrEmpty(allowSameVersion))
                {
                    session.Log("Setting allow same version to true.");

                    configContent = Regex.Replace(configContent, @"""Serilog"":", @"""AllowSameVersion"": true,
    ""Serilog"":", RegexOptions.Singleline);
                }

                File.WriteAllText(configPath, configContent);
            }
            catch (Exception e)
            {
                session.Log($"UpdateConfig failed '{e.Message}'");
                
                return ActionResult.Failure;
            }

            return ActionResult.Success;
        }
        #endregion


        #region public methods - UpdateConfigPrepareData

        /// <summary>
        /// Prepares property for deferred updates of configuration file
        /// </summary>
        /// <param name="session">Session of current installer</param>
        /// <returns>Indication that operation was successful or not</returns>
        [CustomAction]
        public static ActionResult UpdateConfigPrepareData(Session session)
        {
            List<string> configProps = new List<string>
            {
                $"INSTALLFOLDER={session.GetTargetPath(ConfigFolder)}",
                $"UPDATE_JSON_URL={session[PropUpdatesJsonUrl]}",
                $"CHECK_INTERVAL={session[PropCheckInterval]}",
                $"REMOTE_LOG_REST_URL={session[PropRemoteLogRestUrl]}",
                $"ALLOW_SAME_VERSION={session[PropAllowSameVersion]}"
            };

            session[PropUpdateConfig] = string.Join(";", configProps);

            return ActionResult.Success;
        }
        #endregion
    }
}

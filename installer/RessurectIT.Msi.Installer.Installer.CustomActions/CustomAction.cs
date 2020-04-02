using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Deployment.WindowsInstaller;

namespace RessurectIT.Msi.Installer.Installer.CustomActions
{
    /// <summary>
    /// Class that holds custom actions
    /// </summary>
    public class CustomActions
    {
        #region constants

        /// <summary>
        /// Name of msi property storing path to config file
        /// </summary>
        private const string ConfigFolder = "INSTALLFOLDER";

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

        /// <summary>
        /// Name of PROGRESS_TYPE property
        /// </summary>
        private const string PropProgressType = "PROGRESS_TYPE";

        /// <summary>
        /// Name of UpdateConfig property
        /// </summary>
        private const string PropUpdateConfig = "UpdateConfig";

        /// <summary>
        /// Name of default property suffix
        /// </summary>
        private const string PropDefaultName = "_DEFAULT";

        /// <summary>
        /// Default value for unset property
        /// </summary>
        private const string DefaultDefault = "__DEFAULT__";

        /// <summary>
        /// Default value for updates Json Url
        /// </summary>
        private const string DefaultUpdatesJsonUrl = "http://localhost:8888/updates.json";

        /// <summary>
        /// Default value for check Interval
        /// </summary>
        private const string DefaultCheckInterval = "300000";

        /// <summary>
        /// Default value for remote Log Rest Url
        /// </summary>
        private const string DefaultRemoteLogRestUrl = "";

        /// <summary>
        /// Default value for allow Same Version
        /// </summary>
        private const string DefaultAllowSameVersion = "";

        /// <summary>
        /// Default value for progress Type
        /// </summary>
        private const string DefaultProgressType = "App";
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
            string configPath = $"{session.CustomActionData[ConfigFolder]}config.json";
            string updatesJsonUrl = session.CustomActionData[PropUpdatesJsonUrl];
            string checkInterval = session.CustomActionData[PropCheckInterval];
            string remoteLogRestUrl = session.CustomActionData[PropRemoteLogRestUrl];
            string allowSameVersion = session.CustomActionData[PropAllowSameVersion];
            string progressType = session.CustomActionData[PropProgressType];

            session.Log("Begin UpdateConfig");
            session.Log($"Configuration path is '{configPath}'");
            session.Log($"Update json URL: '{updatesJsonUrl}'");
            session.Log($"Check interval: '{checkInterval}'");
            session.Log($"Progress type: '{progressType}'");
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

                    configContent = Regex.Replace(configContent, @"""AllowSameVersion"": .*?,", $@"""AllowSameVersion"": true,", RegexOptions.Singleline);
                }

                if (!string.IsNullOrEmpty(progressType))
                {
                    session.Log($"Setting progress type to value '{progressType}'");

                    configContent = Regex.Replace(configContent, @"""Progress"": "".*?""", $@"""Progress"": ""{progressType}""", RegexOptions.Singleline);
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


        #region public methods - ObtainDefaultConfiguration

        /// <summary>
        /// Obtains default values of configuration properties
        /// </summary>
        /// <param name="session">Session of current installer</param>
        /// <returns>Indication that operation was successful or not</returns>
        [CustomAction]
        public static ActionResult ObtainDefaultConfiguration(Session session)
        {
            string updatesJsonUrl = session[PropUpdatesJsonUrl];
            string checkInterval = session[PropCheckInterval];
            string remoteLogRestUrl = session[PropRemoteLogRestUrl];
            string allowSameVersion = session[PropAllowSameVersion];
            string progressType = session[PropProgressType];

            string defaultUpdatesJsonUrl = session[$"{PropUpdatesJsonUrl}-{PropDefaultName}"];
            string defaultCheckInterval = session[$"{PropCheckInterval}-{PropDefaultName}"];
            string defaultRemoteLogRestUrl = session[$"{PropRemoteLogRestUrl}-{PropDefaultName}"];
            string defaultAllowSameVersion = session[$"{PropAllowSameVersion}-{PropDefaultName}"];
            string defaultProgressType = session[$"{PropProgressType}-{PropDefaultName}"];

            if (string.IsNullOrEmpty(defaultUpdatesJsonUrl))
            {
                defaultUpdatesJsonUrl = session[$"{PropUpdatesJsonUrl}-{PropDefaultName}"] = string.IsNullOrEmpty(updatesJsonUrl) ? DefaultDefault : updatesJsonUrl;
            }

            if (string.IsNullOrEmpty(defaultCheckInterval))
            {
                defaultCheckInterval = session[$"{PropCheckInterval}-{PropDefaultName}"] = string.IsNullOrEmpty(checkInterval) ? DefaultDefault : checkInterval;
            }

            if (string.IsNullOrEmpty(defaultRemoteLogRestUrl))
            {
                defaultRemoteLogRestUrl = session[$"{PropRemoteLogRestUrl}-{PropDefaultName}"] = string.IsNullOrEmpty(remoteLogRestUrl) ? DefaultDefault : remoteLogRestUrl;
            }

            if (string.IsNullOrEmpty(defaultAllowSameVersion))
            {
                defaultAllowSameVersion = session[$"{PropAllowSameVersion}-{PropDefaultName}"] = string.IsNullOrEmpty(allowSameVersion) ? DefaultDefault : allowSameVersion;
            }

            if (string.IsNullOrEmpty(defaultProgressType))
            {
                defaultProgressType = session[$"{PropProgressType}-{PropDefaultName}"] = string.IsNullOrEmpty(progressType) ? DefaultDefault : progressType;
            }

            string defaultConfigContent;

            using(Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RessurectIT.Msi.Installer.Installer.CustomActions.config.json"))
            // ReSharper disable once AssignNullToNotNullAttribute
            using (StreamReader reader = new StreamReader(stream))
            {
                defaultConfigContent = reader.ReadToEnd();
            }

            if (defaultUpdatesJsonUrl == DefaultDefault)
            {
                Match match = Regex.Match(defaultConfigContent, @"""UpdatesJsonUrl"":\s?""(?<value>.*?)""", RegexOptions.Singleline | RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    session[PropUpdatesJsonUrl] = match.Groups["value"].Value;
                }
                else
                {
                    session[PropUpdatesJsonUrl] = DefaultUpdatesJsonUrl;                    
                }
            }

            if (defaultCheckInterval == DefaultDefault)
            {
                Match match = Regex.Match(defaultConfigContent, @"""CheckInterval"":\s?(?<value>\d+)", RegexOptions.Singleline | RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    session[PropCheckInterval] = match.Groups["value"].Value;
                }
                else
                {
                    session[PropCheckInterval] = DefaultCheckInterval;                    
                }
            }

            if (defaultRemoteLogRestUrl == DefaultDefault)
            {
                Match match = Regex.Match(defaultConfigContent, @"""RemoteLogRestUrl"":\s?""(?<value>.*?)""", RegexOptions.Singleline | RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    session[PropRemoteLogRestUrl] = match.Groups["value"].Value;
                }
                else
                {
                    session[PropRemoteLogRestUrl] = DefaultRemoteLogRestUrl;                    
                }
            }

            if (defaultAllowSameVersion == DefaultDefault)
            {
                Match match = Regex.Match(defaultConfigContent, @"""AllowSameVersion"":\s?(?<value>true|false)", RegexOptions.Singleline | RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    session[PropAllowSameVersion] = match.Groups["value"].Value.ToLower() == "true" ? "1" : "";
                }
                else
                {
                    session[PropAllowSameVersion] = DefaultAllowSameVersion;                    
                }
            }

            if (defaultProgressType == DefaultDefault)
            {
                Match match = Regex.Match(defaultConfigContent, @"""Progress"":\s?""(?<value>App|None|MsiExec)""", RegexOptions.Singleline | RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    session[PropProgressType] = match.Groups["value"].Value;
                }
                else
                {
                    session[PropProgressType] = DefaultProgressType;                    
                }
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
                $"{ConfigFolder}={session.GetTargetPath(ConfigFolder)}"
            };

            string[] configPropsArr =
            {
                PropUpdatesJsonUrl,
                PropCheckInterval,
                PropRemoteLogRestUrl,
                PropAllowSameVersion,
                PropProgressType
            };

            foreach (string prop in configPropsArr)
            {
                configProps.Add($"{prop}={session[prop]}");
            }

            session[PropUpdateConfig] = string.Join(";", configProps);

            return ActionResult.Success;
        }
        #endregion
    }
}

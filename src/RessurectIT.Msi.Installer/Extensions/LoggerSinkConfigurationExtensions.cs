using System;
using RessurectIT.Msi.Installer.Configuration;
using Serilog.Configuration;
using Serilog.Events;
using RestLoggerClass = RessurectIT.Msi.Installer.Logger.RestLogger;

// ReSharper disable once CheckNamespace
namespace Serilog
{
    /// <summary>
    /// Extensions methods for <see cref="LoggerSinkConfiguration"/>
    /// </summary>
    public static class LoggerSinkConfigurationExtensions
    {
        #region public methods

        /// <summary>
        /// Registers <see cref="RestLoggerClass"/> sink for storing logs using rest service
        /// </summary>
        /// <param name="loggerConfiguration">Logger configuration object to be extended with new sink</param>
        /// <param name="formatProvider">Format provider used for obtaining format for various types</param>
        /// <param name="config">Application configuration object</param>
        /// <returns><see cref="LoggerConfiguration"/> itself for fluent API</returns>
        public static LoggerConfiguration RestLogger(this LoggerSinkConfiguration loggerConfiguration, ConfigBase config, IFormatProvider formatProvider = null)
        {
            return loggerConfiguration.Conditional(logEvent => config.RestLoggerEnabled && 
                                                                        (logEvent.Level == LogEventLevel.Error || logEvent.Level == LogEventLevel.Fatal) &&
                                                                        logEvent.MessageTemplate.Text.Contains("MSIEXEC LOG:"),
                                                   sinkConfig => sinkConfig.Sink(new RestLoggerClass(formatProvider, config)));
        }
        #endregion
    }
}
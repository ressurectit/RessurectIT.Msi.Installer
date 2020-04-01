using System;
using System.Net.Http;
using System.Text;
using RessurectIT.Msi.Installer.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace RessurectIT.Msi.Installer.Logger
{
    /// <summary>
    /// Class used for logging errors and msiexec logs using rest service
    /// </summary>
    
    internal class RestLogger : ILogEventSink
    {
        #region private fields

        /// <summary>
        /// Format provider used for providing format for various types
        /// </summary>
        private readonly IFormatProvider _formatProvider;

        /// <summary>
        /// Configuration containing remote log rest url
        /// </summary>
        private readonly ConfigBase _config;
        #endregion


        #region constructors

        /// <summary>
        /// Creates instance of <see cref="RestLogger"/>
        /// </summary>
        /// <param name="formatProvider">Format provider used for providing format for various types</param>
        public RestLogger(IFormatProvider formatProvider)
        {
            _formatProvider = formatProvider;
            _config = App.Config;
        }
        #endregion

            
        #region public methods - Implementation of ILogEventSink

        /// <inheritdoc />
        public void Emit(LogEvent logEvent)
        {
            if (string.IsNullOrEmpty(_config.RemoteLogRestUrl))
            {
                return;
            }

            string message = logEvent.RenderMessage(_formatProvider);
            using HttpClient client = new HttpClient();

            try
            {
                client.PostAsync(_config.RemoteLogRestUrl, new StringContent($@"""{message}""", Encoding.UTF8, "application/json")).Wait();
                client.PostAsync("", new StringContent($@"""{message}""", Encoding.UTF8, "application/json")).Wait();
            }
            //this loggers should do nothing if URL endpoint is not listening
            catch
            {
            }
        }
        #endregion
    }
}
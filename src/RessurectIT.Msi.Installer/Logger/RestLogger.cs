using System;
using System.Net.Http;
using System.Text;
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
        #endregion


        #region constructors

        /// <summary>
        /// Creates instance of <see cref="RestLogger"/>
        /// </summary>
        /// <param name="formatProvider">Format provider used for providing format for various types</param>
        public RestLogger(IFormatProvider formatProvider)
        {
            _formatProvider = formatProvider;
        }
        #endregion
        

        #region public methods - Implementation of ILogEventSink

        /// <inheritdoc />
        public void Emit(LogEvent logEvent)
        {
            if (string.IsNullOrEmpty(RessurectITMsiInstallerService.Config.RemoteLogRestUrl))
            {
                return;
            }

            string message = logEvent.RenderMessage(_formatProvider);

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    client.PostAsync(RessurectITMsiInstallerService.Config.RemoteLogRestUrl, new StringContent(message, Encoding.UTF8, "text/plain")).Wait();
                }
                //this loggers should do nothing if URL endpoint is not listening
                catch
                {
                }
            }
        }
        #endregion
    }
}
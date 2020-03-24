using System;
using System.Runtime.Serialization;

namespace RessurectIT.Msi.Installer.Installer
{
    /// <summary>
    /// Exception thrown when there is msiexec problem
    /// </summary>
    internal class InstallationException : Exception
    {
        #region constructors
        
        /// <inheritdoc />
        public InstallationException()
        {
        }

        /// <inheritdoc />
        public InstallationException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public InstallationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <inheritdoc />
        protected InstallationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
        #endregion
    }
}
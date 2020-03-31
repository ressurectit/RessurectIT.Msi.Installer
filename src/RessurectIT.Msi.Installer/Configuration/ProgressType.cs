namespace RessurectIT.Msi.Installer.Configuration
{
    /// <summary>
    /// Available types of installation progress
    /// </summary>
    internal enum ProgressType
    {
        /// <summary>
        /// No progress displayed during installation
        /// </summary>
        None,

        /// <summary>
        /// Displays default minimal msiexec install progress
        /// </summary>
        MsiExec,

        /// <summary>
        /// Displays WPF application install progress window
        /// </summary>
        App
    }
}
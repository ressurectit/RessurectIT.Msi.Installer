# Changelog

## Version 2.0.0

- complete refactoring of application
- application split into two projects
   - two new executables *RessurectIT.Msi.Installer* and *RessurectIT.Msi.Installer.Service*
   - *RessurectIT.Msi.Installer* works as executable which performs installation of msi itself (including running process handling, uninstall, install and starting new process)
   - *RessurectIT.Msi.Installer.Service* works as http server, which can be hosted as *Windows Service*, which can automatically install msi updates or be used for obtaining updates install URI using rest
- *RessurectIT.Msi.Installer*
   - added support for waiting for process end before installation
   - added support for confirimation of process stop
   - added support for restarting application with elevated permissions
   - added support for starting new process after installation ends
   - added support for displaying installation progress
   - added support for running installer using *msiinstall://* protocol
   - fully supports *self-upgrade*
- *RessurectIT.Msi.Installer.Service*
   - supports hosting as *Windows Service*
   - supports running installer as logged user
   - supports REST service for obtaining installation URI for *update id*

## Version 1.0.0

- *RessurectIT.Msi.Installer*
   - supports installing others MSI
   - supports reporting of installation progress
   - supports logging
   - supports *msi* installer for this application
   - supports *self-upgrade* (experimental feature)
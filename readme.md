[![Build status](https://ci.appveyor.com/api/projects/status/6auxqwixtu6axe6s?svg=true)](https://ci.appveyor.com/project/kukjevov/ressurectit-msi-installer)

# RessurectIT.Msi.Installer

RessurectIT tool used for installing other MSI installers automaticaly.

## Configuration options

Configuration is stored in `config.json`. Also configuration can be provided from environment variables (prefixed by *RIT_MSI_INSTALLER_*) or using command line arguments.

Configuration consist of 3 types of configurations. You can provide *default* **Asp.Net.Core** configuration here, **RessurectIT.Msi.Installer** configuration and **Serilog** logger configuration.

### Asp.Net.Core configuration

- `Urls` - Semicolon separater array of URLs at which is running server listenening.

### RessurectIT.Msi.Installer configuration

- `UpdatesJsonUrl` - URL used for obtaining json containing available updates - *Default* `"http://localhost:8888/updates.json"`
- `AllowSameVersion` - indication whether is same version allowed to be reinstalled. Same version, but different MSI must be provided, used during development. - *Default* `false`
- `CheckInterval` - interval when to check for updates in ms - *Default* `300000`
- `AutoInstall` - indication whether to automatically install update when discovered - *Default* `false`
- `LocalServer` - indication whether run service as local, not windows service - *Default* `false`
- `RemoteLogRestUrl` - URL that is used for POSTing logs to centralized store, if not set this logging do nothing - *Default* `null`
- `MsiInstallTimeout` - timeout for *msiexec* install, or uninstall operations in ms - *Default* `90000`
- `WaitForProcessEnd` - timeout for waiting of process end in milliseconds, when terminating running process - *Default* `15000`
- `Progress` - stores type of installation progress that is displayed during processing, available values (`"None"`, `"MsiExec"`, `"App"`) - *Default* `"App"`
- `Debugging` - indication which enables `Debugger.Launch()` at start of application, available only for `DEBUG` build - *Default* `false`

## Available Updates structure

Available updates is json which contains object, where *property* name is **id** of update and *value* contains data about update for installator.

Data can contain following properties:

- `MsiDownloadUrl` - URL where installator can find available update MSI for download **required**
- `InstallParameters` - string of parameters used during installation, format of parameters is as if you call *msiexec* manualy **optional**, can be omitted or null
- `UninstallParameters` - string of parameters used during uninstallation of previous version, if not provided and previous version was installed by installer, installed version install parameters are used as uninstall parameters, format of parameters is as if you call *msiexec* manualy **optional**, can be omitted or null
- `UninstallProductCode` - product code of version that should be uninstalled before installation, if not provided and previous version was installed by installer, installed version production code is used as uninstall product code, format of code is `{ProductCodeGuid}` **optional**, can be omitted or null
- `StopProcessName` - name of process that should be stopped, user is asked whether want to continue by stopping running process (this should prevent any data loss) **optional**, can be omitted or null
- `ForceStop` - if this is set to `true`, user is not asked about stopping process and process is stopped forcibly (can lead to data loss), **optional**, can be omitted or null
- `WaitForProcessNameEnd` - name of process which should end before installation can continue, if process is not stopped during specified timeout time, process is stopped as if `StopProcessName` was specified **optional**, can be omitted or null
- `StartProcessPath` - path to process which should be started after installation was finished **optional**, can be omitted or null
- `AdminPrivilegesRequired` - if this is set to true, installation process will be run with elevated permissions **optional**, can be omitted or null
- `AutoInstall` - indication whether to automatically install update when discovered **optional**, can be omitted or null

## Usage

Application provides several types of usage.

- use it as *Windows Service* which automaticaly installs updates (if configured so)
- use it as remote http server which allows you to obtain available update for specified `id` using *REST* `<hostUrl>/update/<updateId>` which returns URI `msiinstall://<base64encodedupdatedata>`, this URI can be then opened as new process and it will automatically installs update, this allows your application to use it after user confirms update installation and gracefuly shut down application

## Installation

### Installation using *msiexec*

When using *msiexec.exe* for installation of application, (for example: msiexec /i RessurectIT.Msi.Installer-*.msi /q PARAMETER_NAME=PARAMETER_VALUE PARAMETER_NAME2=PARAMETER_VALUE2) where you can pass parameters for installation. Parameters are optional. If parameters are not provided, defaults will be used as mentioned in section **RessurectIT.Msi.Installer configuration**.

Available parameters:
 - INSTALLFOLDER - Path where should be application installed (example INSTALLFOLDER="C:\YourFolderPreference")
 - UPDATE_JSON_URL - Sets `UpdatesJsonUrl` configuration parameter (example UPDATE_JSON_URL=http://localhost/updates.json)
 - CHECK_INTERVAL - Sets `CheckInterval` configuration parameter (example CHECK_INTERVAL=900000)
 - REMOTE_LOG_REST_URL - Sets `RemoteLogRestUrl` configuration parameter (example REMOTE_LOG_REST_URL=http://localhost/api/logs)
 - ALLOW_SAME_VERSION - Sets `AllowSameVersion` configuration parameter (example ALLOW_SAME_VERSION=1)
 - PROGRESS_TYPE - Sets `Progress` configuration parameter (example PROGRESS_TYPE=None)
 - MIN_LOG_LEVEL - Sets minimal log level for logger possible values are Verbose|Debug|Information|Warning|Error|Fatal (example MIN_LOG_LEVEL=Debug)

 ### Installation using gui

 When installing application using setup, you can set all values there.
@SET "plugins=%~2"

@ECHO "Platform: '%~5'"

@CALL dotnet build %~dp0..\RessurectIT.Desktop.Client.sln -c %~1 -r win-x86
@powershell -ExecutionPolicy Bypass %~dp0updateInstaller.ps1
@CALL %~dp0fullCleanObj.bat
@CALL dotnet restore %~dp0../RessurectIT.Desktop.Client.sln --no-cache
@CALL msbuild %~dp0..\RessurectIT.Desktop.Client.sln /t:Build /p:"Configuration=%~1Installer";"Platform=Any CPU";"InstallerPlatform=%~5";"JenkinsUserName=%~3";"JenkinsPassword=%~4"%Plugins%
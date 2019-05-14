@powershell -ExecutionPolicy Bypass %~dp0updateInstaller.ps1
@CALL nuget restore %~dp0../RessurectIT.Msi.Installer.sln
@CALL msbuild %~dp0..\RessurectIT.Msi.Installer.sln /t:Build /p:"Configuration=%~1Installer";"Platform=Any CPU";"JenkinsUserName=%~2";"JenkinsPassword=%~3"
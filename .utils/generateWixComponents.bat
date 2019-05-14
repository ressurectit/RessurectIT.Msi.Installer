@IF "%CurrentDirectory%"=="" SET CurrentDirectory=%~dp0
@CD "%CurrentDirectory%"

heat dir "..\src\RessurectIT.Msi.Installer\bin" -gg -scom -sreg -sfrag -g1 -suid -svb6 -out RessurectITMsiInstallerFiles.wxs
@powershell -ExecutionPolicy Bypass ".\generateWixComponents.ps1"

PAUSE
@IF "%CurrentDirectory%"=="" SET CurrentDirectory=%~dp0
@CD "%CurrentDirectory%"

heat dir "..\src\RessurectIT.Desktop.Client\bin\net461\win-x86" -gg -scom -sreg -sfrag -g1 -suid -svb6 -out RessurectITDesktopClientFiles.wxs
@powershell -ExecutionPolicy Bypass ".\generateWixComponents.ps1"

PAUSE
@IF "%CurrentDirectory%"=="" SET CurrentDirectory=%~dp0
@CD ..

@powershell -ExecutionPolicy Bypass %~dp0fullCleanObj.ps1
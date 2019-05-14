param
(
    [parameter(Mandatory=$true)]
    [string]
    $version
)

$versionDotNet = [System.Text.RegularExpressions.Regex]::Replace($version, "^(\d+\.\d+\.\d+).*?`$", "`$1", [System.Text.RegularExpressions.RegexOptions]::Singleline)

$versionInfo = "using System.Reflection;

[assembly: AssemblyVersion(""$versionDotNet.0"")]
[assembly: AssemblyFileVersion(""$version"")]"

Set-Content (Join-Path $PSScriptRoot "../.include/VersionInfo.cs") $versionInfo
$content = (Get-Content "RessurectITMsiInstallerFiles.wxs" -Encoding UTF8 | Out-String).Trim()

$content = [System.Text.RegularExpressions.Regex]::Replace($content, "</Component>(?:\r\n|\n)(\s+<Component)", "</Component>`n`n`${1}", [System.Text.RegularExpressions.RegexOptions]::Singleline)
$content = [System.Text.RegularExpressions.Regex]::Replace($content, "(<File Id=""(.*?)"" KeyPath=""yes"") Source="".*?"" />", "`${1} Name=""`${2}"" />", [System.Text.RegularExpressions.RegexOptions]::Singleline)
$content = [System.Text.RegularExpressions.Regex]::Replace($content, "(<File Id=""((?:(?!(?:/>|libuv|MDProxy)).)*\.(?:dll|exe))"" KeyPath=""yes"" Name="".*?"") />", "`${1} Assembly="".net"" AssemblyApplication=""`${2}"" />", [System.Text.RegularExpressions.RegexOptions]::Singleline)

Set-Content "RessurectITMsiInstallerFiles.wxs" -Value $content -Encoding UTF8
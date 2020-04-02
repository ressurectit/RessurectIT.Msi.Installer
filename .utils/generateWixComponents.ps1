$content = (Get-Content "RessurectITMsiInstallerFiles.wxs" -Encoding UTF8 | Out-String).Trim()
$originalContent = $content


# $content = [System.Text.RegularExpressions.Regex]::Replace($content, "\s+<Directory Id=""refs"" Name=""refs"">.*?</Directory>", "", [System.Text.RegularExpressions.RegexOptions]::Singleline)
$content = [System.Text.RegularExpressions.Regex]::Replace($content, "</Component>(?:\r\n|\n)(\s+<Component)", "</Component>`n`n`${1}", [System.Text.RegularExpressions.RegexOptions]::Singleline)
$content = [System.Text.RegularExpressions.Regex]::Replace($content, "(<File Id="".*?"" KeyPath=""yes"") Source=""SourceDir\\((?:(?:(?!/>).)*\\)?(.*?))"" />", "`${1} Source=""`$(sys.CURRENTDIR)..\..\src\RessurectIT.Msi.Installer.Service\out\`${2}"" Name=""`${3}"" />", [System.Text.RegularExpressions.RegexOptions]::Singleline)
$content = [System.Text.RegularExpressions.Regex]::Replace($content, "</Directory>", "</ComponentGroup>", [System.Text.RegularExpressions.RegexOptions]::Singleline)
$content = [System.Text.RegularExpressions.Regex]::Replace($content, "<Directory Id=""(.*?)"" Name=""(.*?)"">", "<ComponentGroup Id=""`${1}Files"" Directory=""`${1}"">", [System.Text.RegularExpressions.RegexOptions]::Singleline)

$originalContent = [System.Text.RegularExpressions.Regex]::Replace($originalContent, "\s+<Component.*?</Component>", "", [System.Text.RegularExpressions.RegexOptions]::Singleline)
$originalContent = [System.Text.RegularExpressions.Regex]::Replace($originalContent, "(<Directory.*?>)\s+</Dir", "`$1</Dir", [System.Text.RegularExpressions.RegexOptions]::Singleline)

Set-Content "RessurectITMsiInstallerFiles.wxs" -Value $content -Encoding UTF8
Set-Content "RessurectITMsiInstallerFilesOriginal.wxs" -Value $originalContent -Encoding UTF8
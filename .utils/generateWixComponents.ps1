$content = (Get-Content "RessurectITMsiInstallerFiles.wxs" -Encoding UTF8 | Out-String).Trim()
$content = [System.Text.RegularExpressions.Regex]::Replace($content, "<Wix xmlns="".*?"">", "<Wix>", [System.Text.RegularExpressions.RegexOptions]::Singleline)

$originalContent = $content

$content = [System.Text.RegularExpressions.Regex]::Replace($content, "</Component>(?:\r\n|\n)(\s+<Component)", "</Component>`n`n`${1}", [System.Text.RegularExpressions.RegexOptions]::Singleline)
$content = [System.Text.RegularExpressions.Regex]::Replace($content, "(<File Id="".*?"" KeyPath=""yes"") Source=""SourceDir\\((?:(?:(?!/>).)*\\)?(.*?))"" />", "`${1} Source=""`$(sys.CURRENTDIR)..\..\src\RessurectIT.Msi.Installer.Service\out\`${2}"" Name=""`${3}"" />", [System.Text.RegularExpressions.RegexOptions]::Singleline)
$content = [System.Text.RegularExpressions.Regex]::Replace($content, "</Directory>", "</ComponentGroup>", [System.Text.RegularExpressions.RegexOptions]::Singleline)
$content = [System.Text.RegularExpressions.Regex]::Replace($content, "<Directory Id=""(.*?)"" Name=""(.*?)"">", "<ComponentGroup Id=""`${1}Files"" Directory=""`${1}"">", [System.Text.RegularExpressions.RegexOptions]::Singleline)

$originalContent = [System.Text.RegularExpressions.Regex]::Replace($originalContent, "\s+<Component.*?</Component>", "", [System.Text.RegularExpressions.RegexOptions]::Singleline)
$originalContent = [System.Text.RegularExpressions.Regex]::Replace($originalContent, "(<Directory.*?>)\s+</Dir", "`$1</Dir", [System.Text.RegularExpressions.RegexOptions]::Singleline)

function write-wxi($id, $content)
{
    Set-Content "test" -Value ("$($id).xsi") -Encoding UTF8
}

$xml = [xml]$content;

function write-wxi($node, $id)
{
    $id = $id -replace "Files",""

    Set-Content "$($id).wxi" -Value "<?xml version=""1.0"" encoding=""utf-8""?>
    <Include>
    $($node.OuterXml)
    </Include>" -Encoding UTF8
}

function find-write-wxi($xml, $id)
{
    $node = $xml.DocumentElement.SelectSingleNode("//ComponentGroup[@Id='$id']")
    
    write-wxi $node $id
    
    $node.RemoveAll()
}

find-write-wxi $xml "ModulesFiles"
find-write-wxi $xml "Modules_1Files"
$xml.DocumentElement.SelectSingleNode("//ComponentGroup[@Id='runtimesFiles']").RemoveAll()
$nodes = $xml.DocumentElement.SelectSingleNode("//ComponentGroup[@Id='outFiles']").SelectNodes("//ComponentGroup")

foreach ($node in $nodes) 
{
    write-wxi $node $node.attributes['Id'].value

    $node.RemoveAll()
}

write-wxi $xml "outFile"

Set-Content "RessurectITMsiInstallerFiles.wxs" -Value $content -Encoding UTF8
Set-Content "RessurectITMsiInstallerFilesOriginal.wxs" -Value $originalContent -Encoding UTF8
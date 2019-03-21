
[CmdletBinding()]
param(
    [string]$Project=$PWD,
    [switch]$CopyToPlugins
)

$projectFile = [System.IO.Path]::Combine($PWD, $Project)
if ([System.IO.Directory]::Exists($projectFile)) {
    $projectFile = Get-ChildItem "$Project/*.csproj"
    if ($projectFile.Count -gt 1) {
        Write-Error "Too many project files -- qualify specific project file path with -Project"
        return
    }
    if ($projectFile.Count -lt 1) {
        Write-Error "Could not find project file -- qualify specific project file path with -Project"
        return
    }
}
if (-not [System.IO.File]::Exists($projectFile)) {
    Write-Error "Could not resolve project file"
    return
}
$projectRoot = [System.IO.Path]::GetDirectoryName($projectFile)
Write-Verbose "Project file resolved as [$projectFile]"
Write-Verbose "Project root resolved as [$projectRoot]"

$xmlNode = Select-Xml -Path $projectFile "/Project/PropertyGroup/TargetFramework/text()"
if (-not $xmlNode) {
    Write-Error "Project file doesn't define <TargetFramework> node"
    return
}
$targetFramework = $xmlNode.Node.Value
Write-Verbose "Target Framewwork resolved as [$targetFramework]"

$dotnet = Get-Command dotnet
Write-Verbose "DotNet CLI resolved as [$($dotnet.Path)][v$($dotnet.Version)]"

& $($dotnet.Path) publish -c Release $projectRoot
if (-not $?) {
    Write-Warning "Last command to publish failed, aborting"
    return
}

if ($CopyToPlugins) {
    $publishDir = "$projectRoot/bin/Release/$targetFramework/publish"

    #$tfPluginsDir = [System.IO.Path]::GetFullPath("$($env:APPDATA)/terraform.d/plugins/windows_amd64")
    $tfPluginsDir = [System.IO.Path]::GetFullPath("$PSScriptRoot/tf-plugins")

    if (-not [System.IO.Directory]::Exists($tfPluginsDir)) {
        mkdir $tfPluginsDir
    }

    Copy-Item -Recurse -Path "$pubDir/*" -Destination $tfPluginsDir -Force
}

[CmdletBinding(DefaultParameterSetName="Default")]
param(
    [string]$Project=$PWD,

    [Parameter(ParameterSetName="Copy", Mandatory)]
    [switch]$CopyToPluginsFolder,
    [Parameter(ParameterSetName="Copy")]
    [string]$TargetPluginsFolder=$null,

    [string]$Configuration="Release"
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

& $($dotnet.Path) publish -c $Configuration $projectRoot
if (-not $?) {
    Write-Warning "Last command to publish failed, aborting"
    return
}

if ($CopyToPluginsFolder) {
    $publishDir = "$projectRoot/bin/Release/$targetFramework/publish"
    $pluginsDir = $TargetPluginsFolder

    #$pluginsDir = [System.IO.Path]::GetFullPath("$($env:APPDATA)/terraform.d/plugins/windows_amd64")

    if (-not $pluginsDir) {
        $pluginsDir = [System.IO.Path]::Combine($projectRoot, "tf-plugins")
    }

    if (-not [System.IO.Directory]::Exists($pluginsDir)) {
        Write-Verbose "Creating Target Plugins Folder [$pluginsDir]"
        mkdir $pluginsDir
    }

    Write-Verbose "Copying published plugin assets..."
    Write-Verbose "  from [$publishDir]"
    Write-Verbose "    to [$pluginsDir]"

    Copy-Item -Recurse -Path "$publishDir/*" -Destination $pluginsDir -Force
}
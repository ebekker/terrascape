
[CmdletBinding(DefaultParameterSetName="Default")]
param(
    [string]$Project=$PWD,

    [Parameter(ParameterSetName="Copy", Mandatory)]
    [switch]$CopyToPluginsFolder,
    [Parameter(ParameterSetName="Copy")]
    [string]$TargetPluginsFolder=$null,

    [string]$Configuration="Release",
    [string]$Runtime=$null,
    [string]$DotNetCli="dotnet"
)

if (-not $Runtime) {
    if ($IsWindows) {
        $Runtime = "win-x64"
    }
    elseif ($IsLinux) {
        $Runtime = "linux-x64"
    }
    elseif ($IsMacOs) {
        $Runtime = "osx-x64"
    }
    else {
        Write-Error "Could not resolve current runtime platform automatically, override with -Runtime parameter"
        return
    }
}
Write-Verbose "Resolved project configuration as [$Configuration]"
Write-Verbose "Resolved project runtime as [$Runtime]"

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

$dotnet = Get-Command $DotNetCli
Write-Verbose "DotNet CLI resolved as [$($dotnet.Path)][v$($dotnet.Version)]"

& $($dotnet.Path) publish -c $Configuration -r $Runtime $projectRoot
if (-not $?) {
    Write-Warning "Last command to publish failed, aborting"
    return
}

if ($CopyToPluginsFolder) {
    $publishDir = "$projectRoot/bin/Release/$targetFramework/$Runtime/publish"
    $pluginsDir = $TargetPluginsFolder

    #$pluginsDir = [System.IO.Path]::GetFullPath("$($env:APPDATA)/terraform.d/plugins/windows_amd64")

    if (-not $pluginsDir) {
        $pluginsDir = [System.IO.Path]::Combine($projectRoot, "tf-plugins")
        if ($IsWindows) {
            $pluginsDir += "/windows_amd64"
        }
        elseif ($IsLinux) {
            $pluginsDir += "/linux_amd64"
        }
        elseif ($IsMacOs) {
            $pluginsDir += "/darwin_amd64"
        }
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
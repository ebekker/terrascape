
param(
    [switch]$CopyToPlugins
)

$projectRoot = $PSScriptRoot

& dotnet publish -c Release $projectRoot

if ($CopyToPlugins) {
    $pubDir = "$PSScriptRoot/bin/Release/netcoreapp2.2/publish"
    $pubDir = "$PSScriptRoot/bin/Release/netcoreapp3.0/publish"

    #$tfPluginsDir = [System.IO.Path]::GetFullPath("$($env:APPDATA)/terraform.d/plugins/windows_amd64")
    $tfPluginsDir = [System.IO.Path]::GetFullPath("$PSScriptRoot/tf-plugins")

    if (-not [System.IO.Directory]::Exists($tfPluginsDir)) {
        mkdir $tfPluginsDir
    }

    Copy-Item -Recurse -Path "$pubDir/*" -Destination $tfPluginsDir -Force
}
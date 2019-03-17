
param(
    [switch]$CopyToPlugins
)


& dotnet publish -c Release 

if ($CopyToPlugins) {
    $pubDir = "$PWD/bin/Release/netcoreapp2.2/publish"
    $pubDir = "$PWD/bin/Release/netcoreapp3.0/publish"

    #$tfPluginsDir = [System.IO.Path]::GetFullPath("$($env:APPDATA)/terraform.d/plugins/windows_amd64")
    $tfPluginsDir = [System.IO.Path]::GetFullPath("$PWD/tf-plugins")

    if (-not [System.IO.Directory]::Exists($tfPluginsDir)) {
        mkdir $tfPluginsDir
    }

    Copy-Item -Recurse -Path "$pubDir/*" -Destination $tfPluginsDir -Force
}
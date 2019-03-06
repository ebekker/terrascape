
param(
    [switch]$CopyToPlugins=$true,
    [string]$PluginsPath="go-plugins",
    [string]$ProjConfig="Release",
    [string]$ProjPubDir='bin/$ProjConfig/netcoreapp3.0/publish'
)

Write-Host "Building & Publishing with [$ProjConfig] Configuration"
& dotnet publish -c $ProjConfig

if ($LASTEXITCODE -ne 0) {
    Write-Error "Build/Publish failed"
    return
}

if ($CopyToPlugins) {
    $pubDir = [System.IO.Path]::GetFullPath(
        [System.IO.Path]::Combine($PWD, (Invoke-Expression `"$ProjPubDir`")))
    Write-Host "Copying published project from [$pubDir]"

    $plugDir = [System.IO.Path]::GetFullPath(
        [System.IO.Path]::Combine($PWD, $PluginsPath))
    Write-Host "Copying to plugins folder [$plugDir]"

    if (-not [System.IO.Directory]::Exists($plugDir)) {
        Write-Host "Creating plugins folder..."
        mkdir $plugDir
    }

    Copy-Item -Recurse -Path "$pubDir/*" -Destination $plugDir -Force
}

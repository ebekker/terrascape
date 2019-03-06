
[CmdletBinding(DefaultParameterSetName="Get")]
param(
    ## We start by _assuming_ the go-plugin folder is a peer of this project
    [string]$GoPluginProgram="$PWD/../../../go-plugin/examples/grpc/kv.exe",
    ## This mimics what the do=publish.ps1 script uses
    [string]$PluginsPath="go-plugins",

    ## Use these two when PUTting a value
    [Parameter(ParameterSetName="Put", Mandatory, Position=0)]
    [string]$Put,
    [Parameter(ParameterSetName="Put", Mandatory, Position=1)]
    [string]$Value,
    
    ## Use this one when GETting a value
    [Parameter(ParameterSetName="Get", Mandatory, Position=0)]
    [string]$Get
)

$plugDir = [System.IO.Path]::GetFullPath(
    [System.IO.Path]::Combine($PWD, $PluginsPath))
Write-Host "Using plugins folder [$plugDir]"

## This ENV VAR is how the main KV CLI knows to find and invoke our plugin
$env:KV_PLUGIN = [System.IO.Path]::Combine($plugDir, "HC.GoPlugin.KVExample.exe")

if (-not [System.IO.File]::Exists($GoPluginProgram)) {
    Write-Error "Unable to find go-plugin grpc sample main program at [$GoPluginProgram]"
    return
}

if ($Put) {
    & $GoPluginProgram put $Put $Value
}
else {
    & $GoPluginProgram get $Get
}

# NOTES

## Invoke Plugin as TF Core

```pwsh
$env:PLUGIN_MIN_PORT = '10000'
$env:PLUGIN_MAX_PORT = '15000'
$env:TF_PLUGIN_MAGIC_COOKIE = 'd602bf8f470bc67ca7faa0386276bbdd4330efaf76d1a219cb4d6991ca9872b2'
$env:PLUGIN_PROTOCOL_VERSIONS = '5'

<TF_CONFIG_DIR>\.terraform\plugins\windows_amd64\terraform-provider-aws_v2.43.0_x4.exe
```

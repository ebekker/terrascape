
## We set this magic ENV VAR which is used by our TSCAPE Plugin
## Server runtime to mark the start of a unique, collective TF session
$env:TF_RUN_ID="$([DateTime]::Now.ToString('yyyyMMdd_HH_mm_ss'))"

## Resolve the TF CLI binary:
## ...first opt for a Terrascape-specific selection
$tfBin = $env:TSCAPE_TF_BIN
if (-not $tfBin) {
    ## ...then choose a general Terraform selection
    $tfBin = $env:TF_BIN
}
if (-not $tfBin) {
    ## ...finally just default to TF in the current PATH
    $tfBin = 'terraform'
}

& $tfBin @args

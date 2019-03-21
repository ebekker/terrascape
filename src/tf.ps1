$env:TF_RUN_ID="$([DateTime]::Now.ToString('yyyyMMdd_HH_mm_ss'))"
& C:\local\bin\terraform012.exe @args

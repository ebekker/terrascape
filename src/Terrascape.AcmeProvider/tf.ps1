$env:TSCAPE_SESSION_START="$([DateTime]::Now.ToString('yyyyMMdd_HHmmss'))"
& C:\local\bin\terraform012.exe @args

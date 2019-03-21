## Based on:
##  https://stackoverflow.com/a/37739265/5428506

## Set this if not already set
#$env:OPENSSL_CONF="C:\Program Files\OpenSSL\bin\openssl.cfg"

$csrCN = "localhost"    ## $env:COMPUTERNAME
$csrOU = "YourApp"      ## "YourApp"
$csrO  = "YourOrg"      ## "YourCompany"
$csrL  = "YourCity"     ## "Cupertino"
$csrST = "Maryland"     ## "CA"
$csrC  = "US"           ## "US"

$certDir = [System.IO.Path]::GetFullPath("$PWD/_cert")

if (-not [System.IO.Directory]::Exists($certDir)) {
    mkdir $certDir
}

$caKey = [System.IO.Path]::GetFullPath("$certDir/ca.key")
if (-not [System.IO.File]::Exists($caKey)) {
    Write-Information "Generate CA key:"
    & openssl genrsa -passout pass:1111 -des3 -out $caKey 4096
}

$caCrt = [System.IO.Path]::GetFullPath("$certDir/ca.crt")
if (-not [System.IO.File]::Exists($caCrt)) {
    Write-Information "Generate CA certificate:"
    & openssl req -passin pass:1111 -new -x509 -days 365 -key $caKey -out $caCrt -subj "/C=US/ST=CA/L=Cupertino/O=YourCompany/OU=YourApp/CN=MyRootCA"
}

$serverKey = [System.IO.Path]::GetFullPath("$certDir/server.key")
if (-not [System.IO.File]::Exists($serverKey)) {
    Write-Information "Generate server key:"
    & openssl genrsa -passout pass:1111 -des3 -out $serverKey 4096
}

$serverCsr = [System.IO.Path]::GetFullPath("$certDir/server.csr")
if (-not [System.IO.File]::Exists($serverCsr)) {
    Write-Information "Generate server signing request:"
    & openssl req -passin pass:1111 -new -key $serverKey -out $serverCsr -subj  "/C=$csrC/ST=$csrST/L=$csrL/O=$csrO/OU=$csrOU/CN=$csrCN"
}

$serverCrt = [System.IO.Path]::GetFullPath("$certDir/server.crt")
if (-not [System.IO.File]::Exists($serverCrt)) {
    Write-Information "Self-sign server certificate:"
    & openssl x509 -req -passin pass:1111 -days 365 -in $serverCsr -CA $caCrt -CAkey $caKey -set_serial 01 -out $serverCrt

    Write-Information "Remove passphrase from server key:"
    & openssl rsa -passin pass:1111 -in $serverKey -out $serverKey
}


# echo Generate client key
# openssl genrsa -passout pass:1111 -des3 -out client.key 4096

# echo Generate client signing request:
# openssl req -passin pass:1111 -new -key client.key -out client.csr -subj  "/C=US/ST=CA/L=Cupertino/O=YourCompany/OU=YourApp/CN=%CLIENT-COMPUTERNAME%"

# echo Self-sign client certificate:
# openssl x509 -passin pass:1111 -req -days 365 -in client.csr -CA ca.crt -CAkey ca.key -set_serial 01 -out client.crt

# echo Remove passphrase from client key:
# openssl rsa -passin pass:1111 -in client.key -out client.key
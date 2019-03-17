using System;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using HC.GoPlugin;
using PKISharp.SimplePKI;

namespace HC.GoPlugin
{
    public class TLSConfigSimple : ITLSConfig
    {
        private TLSConfigSimple()
        { }

        public string CaKey { get; set; }

        public string CaCert { get; set; }

        public string ServerKey { get; set; }

        public string ServerCert { get; set; }

        public byte[] ServerCertRaw { get; set; }

        public static ITLSConfig GenerateSelfSignedRSA(
            string cn = "localhost", int keySize = 2048)
        {
            var caKeys = PkiKeyPair.GenerateRsaKeyPair(keySize);
            var loKeys = PkiKeyPair.GenerateRsaKeyPair(keySize);

            var caCsr = new PkiCertificateSigningRequest("CN=FMRL-CA",
                caKeys, PkiHashAlgorithm.Sha256);
            var loCsr = new PkiCertificateSigningRequest($"CN={cn}",
                loKeys, PkiHashAlgorithm.Sha256);
            
            var notBefore = DateTimeOffset.Now.AddMinutes(-30);
            var notAfter = DateTimeOffset.Now.AddHours(24);
            var caCert = caCsr.CreateCa(notBefore, notAfter);
            var loCert = loCsr.Create(caCert, caKeys.PrivateKey,
                notBefore, notAfter, new[] { (byte)100 });

            // Convenience local wrapper function
            string ToUTF8(byte[] b) => Encoding.UTF8.GetString(b);

            return new TLSConfigSimple
            {
                CaKey = ToUTF8(caKeys.PrivateKey.Export(PkiEncodingFormat.Pem)),
                CaCert = ToUTF8(caCert.Export(PkiEncodingFormat.Pem)),
                ServerKey = ToUTF8(loKeys.PrivateKey.Export(PkiEncodingFormat.Pem)),
                ServerCert = ToUTF8(loCert.Export(PkiEncodingFormat.Pem)),
                ServerCertRaw = loCert.Export(PkiEncodingFormat.Der),
            };
        }
    }
}
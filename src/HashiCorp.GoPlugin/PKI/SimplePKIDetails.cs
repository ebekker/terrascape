using System;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using PKISharp.SimplePKI;

namespace HashiCorp.GoPlugin.PKI
{
    /// <summary>
    /// A <i>simple</i> implementation of PKI Details using the SimplePKI library.
    /// </summary>
    public class SimplePKIDetails : IServerPKIDetails
    {
        private PkiKeyPair _issuerKeyPair;
        private PkiKeyPair _serverKeyPair;

        private PkiCertificateSigningRequest _issuerCsr;
        private PkiCertificateSigningRequest _serverCsr;

        private PkiCertificate _issuerCert;
        private PkiCertificate _serverCert;

        public string IssuerCertificate { get; private set; }

        public string ServerCertificate { get; private set; }

        public string ServerPrivateKey { get; private set; }

        public X509Certificate2 ToCertificate()
        {
            var pfx = _serverCert.Export(PkiArchiveFormat.Pkcs12,
                _serverKeyPair.PrivateKey, new[] { _issuerCert });
            return new X509Certificate2(pfx);
        }

        public static IServerPKIDetails GenerateRSA(
            SimplePKIConfig serverConfig = null,
            SimplePKIConfig issuerConfig = null,
            DateTime? notBefore = null,
            DateTime? notAfter = null)
        {
            if (serverConfig == null)
                serverConfig = SimplePKIConfig.DefaultServerConfig;
            if (issuerConfig == null)
                issuerConfig = SimplePKIConfig.DefaultIssuerConfig;

            if (notBefore == null)
                notBefore = DateTime.Now.AddMinutes(-30);
            if (notAfter == null)
                notAfter = DateTime.Now.AddHours(24);

            var details = new SimplePKIDetails();

            details._issuerKeyPair = PkiKeyPair.GenerateRsaKeyPair(issuerConfig.KeySize);
            details._serverKeyPair = PkiKeyPair.GenerateRsaKeyPair(serverConfig.KeySize);

            details._issuerCsr = new PkiCertificateSigningRequest(
                $"CN={issuerConfig.CommonName}",
                details._issuerKeyPair, issuerConfig.HashAlgorithm);
            details._serverCsr = new PkiCertificateSigningRequest(
                $"CN={serverConfig.CommonName}",
                details._serverKeyPair, serverConfig.HashAlgorithm);

            details._issuerCert = details._issuerCsr.CreateCa(notBefore.Value, notAfter.Value);
            details._serverCert = details._serverCsr.Create(details._issuerCert, details._issuerKeyPair.PrivateKey,
                notBefore.Value, notAfter.Value,
                new[] { (byte)100 });

            // Convenience local wrapper function
            string ToUTF8(byte[] b) => Encoding.UTF8.GetString(b);

            details.IssuerCertificate = ToUTF8(details._issuerCert.Export(PkiEncodingFormat.Pem));
            details.ServerCertificate = ToUTF8(details._serverCert.Export(PkiEncodingFormat.Pem));
            details.ServerPrivateKey = ToUTF8(details._serverKeyPair.PrivateKey.Export(PkiEncodingFormat.Pem));
            
            return details;
        } 
    }
}
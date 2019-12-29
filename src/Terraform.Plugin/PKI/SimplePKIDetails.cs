using System;
using System.Text;
using PKISharp.SimplePKI;

namespace Terraform.Plugin.PKI
{
    /// <summary>
    /// A <i>simple</i> implementation of PKI Details using the SimplePKI library.
    /// </summary>
    public class SimplePKIDetails : IServerPKIDetails
    {
        public string IssuerCertificate { get; private set; }

        public string ServerCertificate { get; private set; }

        public string ServerPrivateKey { get; private set; }

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

            var issuerKeyPair = PkiKeyPair.GenerateRsaKeyPair(issuerConfig.KeySize);
            var serverKeyPair = PkiKeyPair.GenerateRsaKeyPair(serverConfig.KeySize);

            var issuerCsr = new PkiCertificateSigningRequest(
                $"CN={issuerConfig.CommonName}",
                issuerKeyPair, issuerConfig.HashAlgorithm);
            var serverCsr = new PkiCertificateSigningRequest(
                $"CN={serverConfig.CommonName}",
                serverKeyPair, serverConfig.HashAlgorithm);

            var issuerCert = issuerCsr.CreateCa(notBefore.Value, notAfter.Value);
            var serverCert = serverCsr.Create(issuerCert, issuerKeyPair.PrivateKey,
                notBefore.Value, notAfter.Value,
                new[] { (byte)100 });

            // Convenience local wrapper function
            string ToUTF8(byte[] b) => Encoding.UTF8.GetString(b);

            return new SimplePKIDetails
            {
                IssuerCertificate = ToUTF8(issuerCert.Export(PkiEncodingFormat.Pem)),
                ServerCertificate = ToUTF8(serverCert.Export(PkiEncodingFormat.Pem)),
                ServerPrivateKey = ToUTF8(serverKeyPair.PrivateKey.Export(PkiEncodingFormat.Pem)),
            };
        } 
    }
}
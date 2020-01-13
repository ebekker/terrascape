using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Grpc.Core;

namespace HC.GoPlugin
{
    /// <summary>
    /// Encapsulates server-side (plugin server) certificate details when
    /// the plugin interface needs to support mutual-TLS (MTLS).
    /// </summary>
    public class TLSConfig : ITLSConfig
    {
        /// PEM content representing a CA certificate.
        public string CaCert { get; set; }

        /// PEM content representing plugin server's private key.
        public string ServerKey { get; set; }

        /// PEM content representing plugin server's public certificate.
        public string ServerCert { get; set; }

        /// Raw bytes of the content representing plugin server's public certificate.
        public byte[] ServerCertRaw { get; set; }

        /// <summary>
        /// Transforms the PKI components captured in this <c>TLSConfig</c> instance into a
        /// server credential object that can be used with a gRPC service endpoint (server port).
        /// </summary>
        public static SslServerCredentials ToCredentials(ITLSConfig tls,
            bool authenticateClient = false)
        {
            if (authenticateClient)
            {
                return new SslServerCredentials(
                    new List<KeyCertificatePair>() {
                        new KeyCertificatePair(tls.ServerCert, tls.ServerKey)
                    },
                    rootCertificates: tls.CaCert,
                    forceClientAuth: false
                );
            }
            else
            {
                return new SslServerCredentials(
                    new List<KeyCertificatePair>() {
                        new KeyCertificatePair(tls.ServerCert, tls.ServerKey)
                    }
                );
            }
        }

        /// <summary>
        /// Convenience routine to build a <c>TLSConfig</c> instance from
        /// file paths to PEM files for each of the PKI components.
        /// </summary>
        public static ITLSConfig FromFiles(string caCertPath,
            string serverKeyPath, string serverCertPath,
            bool asmRelative = false)
        {
            var relRoot = Directory.GetCurrentDirectory();
            if (asmRelative)
            {
                relRoot = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            }

            caCertPath = Path.Combine(relRoot, caCertPath);
            serverKeyPath = Path.Combine(relRoot, serverKeyPath);
            serverCertPath = Path.Combine(relRoot, serverCertPath);

            var c = new TLSConfig();
            c.CaCert = File.ReadAllText(caCertPath);
            c.ServerKey = File.ReadAllText(serverKeyPath);
            c.ServerCert = File.ReadAllText(serverCertPath);
            using (var x509 = new X509Certificate2(serverCertPath))
            {
                c.ServerCertRaw = x509.RawData;
            }
            return c;
        }
    }
}
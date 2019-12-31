using System.Security.Cryptography.X509Certificates;

namespace HashiCorp.GoPlugin.PKI
{
    /// <summary>
    /// Defines the configuration for a set of PKI components that
    /// can be used to secure a plugin communication channel.
    /// </summary>
    public interface IServerPKIDetails
    {
        /// <summary>
        /// PEM-encoded PKI certificate of the Issuer (Certificate Authority)
        /// that generated the associated Server Certificate.
        /// </summary>
        string IssuerCertificate { get; }

        /// <summary>
        /// PEM-encoded Server Certificate.
        /// </summary>
        string ServerCertificate { get; }

        /// <summary>
        /// PEM-encoded private key of the associated Server Certificate.
        /// </summary>
        string ServerPrivateKey { get; }

        X509Certificate2 ToCertificate();
    }
}

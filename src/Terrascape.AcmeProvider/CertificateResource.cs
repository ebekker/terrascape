using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using HC.TFPlugin;
using HC.TFPlugin.Attributes;

namespace Terrascape.AcmeProvider
{
    /// <summary>
    /// Can be used to create and manage an ACME TLS certificate.
    /// </summary>
    /// <remarks>
    /// This resource mimics the Terraform
    /// <see cref="https://www.terraform.io/docs/providers/acme/r/certificate.html"
    /// >ACME certificate_resource</see> whenever possible.
    [TFResource("acmelo_certificate")]
    public class CertificateResource
    {
        /** Argument Reference **/

        /// Private key of the account that is requesting the certificate.
        [TFAttribute("account_key_pem",
            Required = true,
            ForceNew = true,
            Sensitive = true)]
        public string AccountKeyPem { get; set; }

        /// Certificate's common name, the primary domain that the certificate
        /// will be recognized for. Required when not specifying a CSR.
        [TFAttribute("common_name",
            Optional = true,
            ForceNew = true,
            ConflictsWith = new[] { nameof(CertificateRequestPem) })]
        public string CommonName { get; set; }

        /// Certificate's subject alternative names, domains that this certificate
        /// will also be recognized for. Only valid when not specifying a CSR.
        [TFAttribute("subject_alternative_names",
            Optional = true,
            ForceNew = true,
            ConflictsWith = new[] { nameof(CertificateRequestPem) })]
        public string[] SubjectAlternativeNames { get; set; }

        /// <summary>
        /// Key type for the certificate's private key.
        /// </summary>
        /// <remarks>
        /// Can be one of: P256 and P384 (for ECDSA keys of respective length)
        /// or 2048, 4096, and 8192 (for RSA keys of respective length). Required
        /// when not specifying a CSR. The default is 2048 (RSA key of 2048 bits).
        /// </remarks>
        [TFAttribute("key_type",
            Optional = true,
            ForceNew = true,
            ConflictsWith = new[] { nameof(CertificateRequestPem) })]
        public string KeyType { get; set; } = "2048";

        public static readonly IEnumerable<string> ValidKeyTypes = new[]
        {
            "P256", "P384",
            "2048", "4096", "8192",
        };

        public IEnumerable<ValidationResult> KeyType_Validate()
        {
            if (!ValidKeyTypes.Contains(KeyType))
            {
                return new[] { new ValidationResult(
                    "Certificate key type must be one of the following: P256, P384, 2048, 4096, or 8192"
                )};
            }

            return null;
        }

        /// Pre-created certificate request, such as one from tls_cert_request,
        /// or one from an external source, in PEM format. Either this, or the
        /// in-resource request options (<c>common_name</c>, <c>key_type</c>,
        /// and optionally <c>subject_alternative_names</c>) need to be specified.
        [TFAttribute("certificate_request_pem",
            Optional = true,
            ForceNew = true,
            ConflictsWith = new[] {
                nameof(CommonName),
                nameof(SubjectAlternativeNames),
                nameof(KeyType),
            })]
        public string CertificateRequestPem { get; set; }

        /// Minimum amount of days remaining on the expiration of a certificate before
        /// a renewal is attempted. The default is 7. A value of less than 0 means that
        /// the certificate will never be renewed.
        [TFAttribute("min_days_remaining",
            Optional = true)]
        public int MinDaysRemaining { get; set; } = 7;

        /// File path where the challenge response to an <c>http-01</c> challenge
        /// will be written.  Tjhis would typically be the root path of a web site
        /// folder that will be responding to the ACME challenge.
        [TFAttribute("challenge_response_path")]
        public string ChallengeResponsePath { get; set; }

/*
dns_challenge (Required) - The DNS challenge to use in fulfilling the request.
must_staple (Optional) Enables the OCSP Stapling Required TLS Security Policy extension. Certificates with this extension must include a valid OCSP Staple in the TLS handshake for the connection to succeed. Defaults to false. Note that this option has no effect when using an external CSR - it must be enabled in the CSR itself.
NOTE: OCSP stapling requires specific webserver configuration to support the downloading of the staple from the CA's OCSP endpoints, and should be configured to tolerate prolonged outages of the OCSP service. Consider this when using must_staple, and only enable it if you are sure your webserver or service provider can be configured correctly.
 */
        /** Attribute Reference **/

        /// Full URL of the certificate within the ACME CA. Same as id.
        [TFAttribute("certificate_url",
            Computed = true)]
        public string CertificateUrl { get; set; }

        /// Common name of the certificate.
        [TFAttribute("certificate_domain",
            Computed = true)]
        public string CertificateDomain { get; set; }

        /// Certificate's private key, in PEM format, if the certificate was generated
        /// from scratch and not with certificate_request_pem. If certificate_request_pem
        /// was used, this will be blank.
        [TFAttribute("private_key_pem",
            Computed = true,
            Sensitive = true)]
        public string PrivateKeyPem { get; set; }

        /// Certificate in PEM format.
        [TFAttribute("certificate_pem",
            Computed = true)]
        public string CertificatePem { get; set; }

        /// Intermediate certificate of the issuer.
        [TFAttribute("issuer_pem",
            Computed = true)]
        public string IssuerPem { get; set; }

        /// Password to be used to secure the PKCS12 (PFX) archive content
        /// made available in the <c>certificate_p12</c> attribute.  If
        /// empty or unspecified, the PKCS12 archive will not be protected
        /// with a password.
        [TFAttribute("certificate_p12_password",
            Optional = true,
            Sensitive = true)]
        public string CertificateP12Password { get; set; }

        /// Certificate, intermediate, and the private key archived as a PFX
        /// file (PKCS12 format, generally used by Microsoft products). The
        /// data is base64 encoded and has no password unless one was specified
        /// with the <c>certificate_p12_password</c> argument.
        /// This field is empty if creating a certificate from a CSR.
        [TFAttribute("certificate_p12",
            Computed = true,
            Sensitive = true)]
        public string CertificateP12 { get; set; }
/*
The following attributes are exported:

id - The full URL of the certificate within the ACME CA.
certificate_url - The f
certificate_domain - The c
private_key_pem - The c
certificate_pem - The certificate in PEM format.
issuer_pem - The intermediate certificate of the issuer.
certificate_p12 - The c
 */

    }
}

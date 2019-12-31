using PKISharp.SimplePKI;

namespace HashiCorp.GoPlugin.PKI
{
    public class SimplePKIConfig
    {
        public static readonly SimplePKIConfig DefaultServerConfig = new SimplePKIConfig();

        public static readonly SimplePKIConfig DefaultIssuerConfig = new SimplePKIConfig
        {
            CommonName = "FMRL-CA",
        };

        public string CommonName { get; set; } = "localhost";

        public int KeySize { get; set; } = 2048;

        public PkiHashAlgorithm HashAlgorithm { get; set; } = PkiHashAlgorithm.Sha256;
    }
}
namespace HC.GoPlugin
{
    public interface ITLSConfig
    {
        string CaCert { get; }

        string ServerKey { get; }
        
        string ServerCert { get; }

        byte[] ServerCertRaw { get; }
    }        
}

namespace Identity
{
    using System.Security.Cryptography.X509Certificates;

    public class CertificateOptions
    {
        public string Thumbprint { get; set; }

        public StoreName StoreName { get; set; }

        public StoreLocation StoreLocation { get; set; }
    }
}

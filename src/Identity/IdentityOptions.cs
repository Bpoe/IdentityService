namespace Identity;

using System.Security.Cryptography.X509Certificates;

public class IdentityOptions
{
    public string CertificateSubject { get; set; } = string.Empty;

    public StoreName CertificateStoreName { get; set; } = StoreName.My;

    public StoreLocation CertificateStoreLocation { get; set; } = StoreLocation.LocalMachine;
}

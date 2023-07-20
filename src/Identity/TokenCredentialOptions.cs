namespace Identity;

using System.Security.Cryptography.X509Certificates;

public class TokenCredentialOptions
{
    public string? ClientId { get; set; }

    public string? ClientSecret { get; set; }

    public StoreName CertificateStoreName { get; set; } = StoreName.My;

    public StoreLocation CertificateStoreLocation { get; set; } = StoreLocation.CurrentUser;

    public string? CertificateSubject { get; set; }

    public string? CertificatePath { get; set; }

    public string? TenantId { get; set; }

    public TokenCredentialOptions Clone()
        => new()
        {
            ClientId = ClientId,
            ClientSecret = ClientSecret,
            CertificateStoreName = CertificateStoreName,
            CertificateStoreLocation = CertificateStoreLocation,
            CertificateSubject = CertificateSubject,
            CertificatePath = CertificatePath,
            TenantId = TenantId,
        };
}
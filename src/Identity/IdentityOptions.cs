namespace Identity;

using System;
using System.Security.Cryptography.X509Certificates;

public class IdentityOptions
{
    public bool IsArc { get; set; } = true;

    public string? ClientId { get; set; }

    public string? ClientSecret { get; set; }

    public StoreName? CertificateStoreName { get; set; }

    public StoreLocation? CertificateStoreLocation { get; set; }

    public string? CertificateSubject { get; set; }

    public string? CertificatePath { get; set; }

    public string? TenantId { get; set; }

    public Uri? AuthorityHost { get; set; }

    public string? ClientCertificatePassword { get; set; }

    public bool SendCertificateChain { get; set; } = false;
}
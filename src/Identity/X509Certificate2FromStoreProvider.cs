namespace Identity;

using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Azure.Identity;

public class X509Certificate2FromStoreProvider
{
    private readonly StoreName storeName;
    private readonly StoreLocation storeLocation;
    private readonly string subject;

    public X509Certificate2FromStoreProvider(StoreName storeName, StoreLocation storeLocation, string subject)
    {
        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new ArgumentException($"'{nameof(subject)}' cannot be null or whitespace.", nameof(subject));
        }

        this.storeName = storeName;
        this.storeLocation = storeLocation;
        this.subject = subject;
    }

    public ValueTask<X509Certificate2> GetCertificateAsync(CancellationToken cancellationToken = default)
    {
        using var x509Store = new X509Store(this.storeName, this.storeLocation);
        x509Store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
        try
        {
            var certificate = x509Store.Certificates.Find(X509FindType.FindBySubjectName, this.subject, false).FirstOrDefault();

            return certificate is null
                ? throw new CredentialUnavailableException($"Could not load certificate with subject {this.subject}")
                : new ValueTask<X509Certificate2>(certificate);
        }
        finally
        {
            x509Store.Close();
        }
    }
}

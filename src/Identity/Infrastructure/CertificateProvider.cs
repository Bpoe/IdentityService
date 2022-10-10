namespace Identity.Infrastructure;

using System;
using System.Security.Cryptography.X509Certificates;
using Interfaces;

public class CertificateProvider : ICertificateProvider
{
    private readonly StoreName storeName;
    private readonly StoreLocation storeLocation;

    public CertificateProvider(StoreName storeName, StoreLocation storeLocation)
    {
        this.storeName = storeName;
        this.storeLocation = storeLocation;
    }

    public X509Certificate2? FindCertificate(string thumbprint)
    {
        if (string.IsNullOrWhiteSpace(thumbprint)) throw new ArgumentNullException(nameof(thumbprint));

        using var x509Store = new X509Store(this.storeName, this.storeLocation);
        x509Store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
        try
        {
            var collection = x509Store.Certificates.Find(X509FindType.FindBySubjectName, thumbprint, false);
            return collection.Count == 0 ? null : collection[0];
        }
        finally
        {
            x509Store.Close();
        }
    }
}

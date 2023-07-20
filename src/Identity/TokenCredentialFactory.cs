namespace Identity;

using System;
using Azure.Core;
using Azure.Identity;

public static class TokenCredentialFactory
{
    public static TokenCredential GetTokenCredential(TokenCredentialOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.CertificateSubject))
        {
            // ClientCertificateCredential needs a constructor that retrieve certs from the cert store :(
            var certProvider = new X509Certificate2FromStoreProvider(
                    options.CertificateStoreName,
                    options.CertificateStoreLocation,
                    options.CertificateSubject);

            return new ClientCertificateCredential(
                    options.TenantId,
                    options.ClientId,
                    certProvider.GetCertificateAsync().Result);
        }

        if (!string.IsNullOrWhiteSpace(options.CertificatePath))
        {
            return new ClientCertificateCredential(
                    options.TenantId,
                    options.ClientId,
                    options.CertificatePath);
        }

        if (!string.IsNullOrWhiteSpace(options.ClientSecret))
        {
            return new ClientSecretCredential(
                    options.TenantId,
                    options.ClientId,
                    options.ClientSecret);
        }

        throw new ArgumentException("Unable to determine how to get a TokenCredential from the options provided.");
    }
}

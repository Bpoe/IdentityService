namespace Identity;

using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;

public static class TokenCredentialFactory
{
    public static async Task<TokenCredential> GetTokenCredential(TokenCredentialOptions options, CancellationToken cancellationToken = default)
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
                    await certProvider.GetCertificateAsync(cancellationToken));
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

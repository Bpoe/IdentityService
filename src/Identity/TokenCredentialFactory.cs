namespace Identity;

using System;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;

public static class TokenCredentialFactory
{
    public static async Task<TokenCredential> GetTokenCredential(IdentityOptions options)
    {
        ArgumentException.ThrowIfNullOrEmpty(options.TenantId, nameof(options.TenantId));
        ArgumentException.ThrowIfNullOrEmpty(options.ClientId, nameof(options.ClientId));

        // EnvironmentCredential does support Certificates in the Windows Certificate Store
        if (!string.IsNullOrWhiteSpace(options.CertificateSubject)
            && options.CertificateStoreName.HasValue
            && options.CertificateStoreLocation.HasValue)
        {
            var certProvider = new X509Certificate2FromStoreProvider(
                    options.CertificateStoreName.Value,
                    options.CertificateStoreLocation.Value,
                    options.CertificateSubject);

            return new ClientCertificateCredential(
                    options.TenantId,
                    options.ClientId,
                    await certProvider.GetCertificateAsync(),
                    new ClientCertificateCredentialOptions()
                    {
                        AuthorityHost = options.AuthorityHost,
                        SendCertificateChain = options.SendCertificateChain
                    });
        }

        Environment.SetEnvironmentVariable("AZURE_TENANT_ID", options.TenantId);
        Environment.SetEnvironmentVariable("AZURE_CLIENT_ID", options.ClientId);
        Environment.SetEnvironmentVariable("AZURE_CLIENT_SECRET", options.ClientSecret);
        Environment.SetEnvironmentVariable("AZURE_CLIENT_CERTIFICATE_PATH", options.CertificatePath);
        Environment.SetEnvironmentVariable("AZURE_CLIENT_CERTIFICATE_PASSWORD", options.ClientCertificatePassword);
        Environment.SetEnvironmentVariable("AZURE_CLIENT_SEND_CERTIFICATE_CHAIN", options.SendCertificateChain.ToString());
        Environment.SetEnvironmentVariable("AZURE_AUTHORITY_HOST", options.AuthorityHost?.ToString());

        return new EnvironmentCredential();
    }
}

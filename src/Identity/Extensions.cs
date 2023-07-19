namespace Identity;

using System.Security.Cryptography.X509Certificates;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class Extensions
{
    public static IServiceCollection AddTokenCredentialFromConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var certificateSubject = configuration["CertificateSubject"];
        var certificatePath = configuration["CertificatePath"];
        if (!string.IsNullOrWhiteSpace(certificateSubject))
        {
            // ClientCertificateCredential needs a constructor that retrieve certs from the cert store :(
            var certProvider = new X509Certificate2FromStoreProvider(
                    configuration.GetValue<StoreName>("CertificateStoreName"),
                    configuration.GetValue<StoreLocation>("CertificateStoreLocation"),
                    certificateSubject);

            services
                .AddSingleton<TokenCredential>(new ClientCertificateCredential(
                    configuration["TenantId"],
                    configuration["ClientId"],
                    certProvider.GetCertificateAsync().Result));
        }
        else if (!string.IsNullOrWhiteSpace(certificatePath))
        {
            services
                .AddSingleton<TokenCredential>(new ClientCertificateCredential(
                    configuration["TenantId"],
                    configuration["ClientId"],
                    certificatePath));
        }
        else
        {
            services
                .AddSingleton<TokenCredential>(new ClientSecretCredential(
                    configuration["TenantId"],
                    configuration["ClientId"],
                    configuration["ClientSecret"]));
        }

        return services;
    }
}
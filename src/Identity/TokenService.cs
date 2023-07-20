namespace Identity;

using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;

public class TokenService
{
    private const string DefaultForScope = "/.default";

    private readonly TokenCredentialOptions options;
    private readonly TenantIdResolver tenantIdResolver;

    public TokenService(TokenCredentialOptions options, TenantIdResolver tenantIdResolver)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.tenantIdResolver = tenantIdResolver ?? throw new ArgumentNullException(nameof(tenantIdResolver));
    }

    public async Task<TokenResponse> GetTokenAsync(string resource, string? contextResource = default)
    {
        var requestOptions = this.options.Clone();

        if (!string.IsNullOrEmpty(contextResource))
        {
            requestOptions.TenantId = await this.tenantIdResolver.GetTenantIdAsync(contextResource);
        }

        var credential = TokenCredentialFactory.GetTokenCredential(requestOptions);

        var context = new TokenRequestContext(new[] { resource + DefaultForScope });
        var accessToken = await credential.GetTokenAsync(context, CancellationToken.None);

        return TokenResponse.FromAccessToken(accessToken);
    }
}

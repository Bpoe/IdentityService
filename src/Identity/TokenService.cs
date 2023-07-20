namespace Identity;

using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.Extensions.Caching.Distributed;

public class TokenService
{
    private const string DefaultForScope = "/.default";

    private readonly TokenCredentialOptions options;
    private readonly TenantIdResolver tenantIdResolver;
    private readonly IDistributedCache cache;

    public TokenService(TokenCredentialOptions options, TenantIdResolver tenantIdResolver, IDistributedCache cache)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.tenantIdResolver = tenantIdResolver ?? throw new ArgumentNullException(nameof(tenantIdResolver));
        this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task<TokenResponse> GetTokenAsync(string resource, string? contextResource = default)
    {
        var requestOptions = this.options.Clone();

        if (!string.IsNullOrEmpty(contextResource))
        {
            requestOptions.TenantId = await this.tenantIdResolver.Resolve(contextResource);
        }

        // Ensure that we have a TenantId
        if (string.IsNullOrEmpty(requestOptions.TenantId))
        {
            throw new ArgumentException("tenantId cannot be null or emtpty", nameof(requestOptions.TenantId));
        }

        // Check for cached access token
        var key = $"{requestOptions.TenantId.ToLowerInvariant()}_{resource.ToLowerInvariant()}";
        var cached = await this.cache.GetStringAsync(key);
        if (cached != null)
        {
            return TokenResponse.FromString(cached);
        }

        var credential = TokenCredentialFactory.GetTokenCredential(requestOptions);

        var context = new TokenRequestContext(new[] { resource + DefaultForScope });
        var accessToken = await credential.GetTokenAsync(context, CancellationToken.None);

        // Add token to cache
        await this.cache.SetStringAsync(key, accessToken.Token, new DistributedCacheEntryOptions { AbsoluteExpiration = accessToken.ExpiresOn - TimeSpan.FromMinutes(10) });

        return TokenResponse.FromAccessToken(accessToken);
    }
}

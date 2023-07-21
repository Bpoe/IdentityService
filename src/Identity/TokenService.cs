namespace Identity;

using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

public class TokenService
{
    private const string DefaultForScope = "/.default";
    
    private static readonly TimeSpan CacheBuffer = TimeSpan.FromMinutes(10);

    private readonly TokenCredentialOptions options;
    private readonly TenantIdResolver tenantIdResolver;
    private readonly IDistributedCache cache;
    private readonly ILogger<TokenService> logger;

    public TokenService(TokenCredentialOptions options, TenantIdResolver tenantIdResolver, IDistributedCache cache, ILogger<TokenService> logger)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.tenantIdResolver = tenantIdResolver ?? throw new ArgumentNullException(nameof(tenantIdResolver));
        this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        this.logger = logger;
    }

    public async Task<TokenResponse> GetTokenAsync(string resource, string? challengeResource = default)
    {
        var requestOptions = this.options.Clone();

        if (!string.IsNullOrEmpty(challengeResource))
        {
            this.logger?.LogDebug("Attempting to resolve tenantId using challenge resource {resource}", challengeResource);
            requestOptions.TenantId = await this.tenantIdResolver.Resolve(challengeResource);
        }

        // Ensure that we have a TenantId
        ArgumentException.ThrowIfNullOrEmpty(requestOptions.TenantId, nameof(requestOptions.TenantId));

        // Check for cached access token
        var key = $"{requestOptions.TenantId.ToLowerInvariant()}_{resource.ToLowerInvariant()}";
        var cached = await this.cache.GetStringAsync(key);
        if (cached != null)
        {
            this.logger?.LogDebug("Found cached access token.");
            return TokenResponse.FromString(cached);
        }

        this.logger?.LogDebug("Retrieving new access token.");
        var credential = TokenCredentialFactory.GetTokenCredential(requestOptions);

        var context = new TokenRequestContext(new[] { resource + DefaultForScope });
        var accessToken = await credential.GetTokenAsync(context, CancellationToken.None);

        // Add token to cache
        this.logger?.LogDebug("Adding access token to cache.");
        await this.cache.SetStringAsync(
           key,
           accessToken.Token,
           new DistributedCacheEntryOptions
           {
               AbsoluteExpiration = accessToken.ExpiresOn - CacheBuffer,
           });

        return TokenResponse.FromString(accessToken.Token);
    }
}

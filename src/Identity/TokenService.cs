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

    public async Task<TokenResponse> GetTokenAsync(string resource, string? challengeResource = default, CancellationToken cancellationToken = default)
    {
        var requestOptions = this.options.Clone();

        if (!string.IsNullOrEmpty(challengeResource))
        {
            this.logger?.LogDebug("Attempting to resolve tenantId using challenge resource {resource}", challengeResource);
            requestOptions.TenantId = await this.tenantIdResolver.Resolve(challengeResource, cancellationToken);
        }

        // Ensure that we have a TenantId
        ArgumentException.ThrowIfNullOrEmpty(requestOptions.TenantId, nameof(requestOptions.TenantId));

        // Check for cached access token
        var key = $"{requestOptions.TenantId.ToLowerInvariant()}_{resource.ToLowerInvariant()}";
        var cached = await this.GetCachedStringAsync(key, cancellationToken);
        if (cached is not null)
        {
            this.logger?.LogDebug("Found cached access token.");
            return TokenResponse.FromString(cached);
        }

        this.logger?.LogDebug("Retrieving new access token.");
        var credential = await TokenCredentialFactory.GetTokenCredential(requestOptions, cancellationToken);

        var context = new TokenRequestContext(new[] { resource + DefaultForScope });
        var accessToken = await credential.GetTokenAsync(context, cancellationToken);

        // Add token to cache
        await this.SetCachedAccessTokenAsync(key, accessToken, cancellationToken);

        return TokenResponse.FromString(accessToken.Token);
    }

    public async Task<string?> GetCachedStringAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            return await this.cache.GetStringAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            // Redis will throw if the connection fails. If this happens,
            // we shouldn't crash, we should just get a new token.
            this.logger?.LogError(ex, "An exception occurred while reading the cache.");
            return null;
        }
    }

    public async Task SetCachedAccessTokenAsync(string key, AccessToken accessToken, CancellationToken cancellationToken = default)
    {
        this.logger?.LogDebug("Adding access token to cache.");

        try
        {
            await this.cache.SetStringAsync(
               key,
               accessToken.Token,
               new DistributedCacheEntryOptions
               {
                   AbsoluteExpiration = accessToken.ExpiresOn - CacheBuffer,
               },
               cancellationToken);
        }
        catch (Exception ex)
        {
            this.logger?.LogError(ex, "An exception occurred while setting the cache.");
        }
    }
}

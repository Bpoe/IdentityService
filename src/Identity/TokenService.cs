namespace Identity;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

public class TokenService
{
    private const string DefaultForScope = "/.default";

    private static string TokensPath = OperatingSystem.IsWindows()
        ? Environment.ExpandEnvironmentVariables("%ProgramData%\\AzureConnectedMachineAgent\\tokens")
        : "/var/lib/waagent/tokens";

    private readonly TokenCredential tokenCredential;
    private readonly IdentityOptions options;
    private readonly ILogger<TokenService> logger;

    public TokenService(TokenCredential tokenCredential, IdentityOptions options, ILogger<TokenService> logger)
    {
        this.tokenCredential = tokenCredential ?? throw new ArgumentNullException(nameof(tokenCredential));
        this.options = options;
        this.logger = logger;
    }

    public async Task<IResult> GetTokenAsync(string resource, string? authorizationKey = null)
    {
        // If this is an Arc Identity Service and no authorization key is provided,
        // return a challenge response with an authorization key file path.
        if (this.options.IsArc && authorizationKey is null)
        {
            var keys = await this.GetAuthorizationKeys();
            if (!keys.Any())
            {
                // Create a new authorization key file if none exist.
                var file = await this.AddNewAuthorizationKeyFile();
                return new ArcChallengeHttpResult(file);
            }

            return new ArcChallengeHttpResult(keys.Keys.First());
        }

        // If this is an Arc Identity Service and an authorization key is provided,
        // validate the key and remove the file.
        if (this.options.IsArc && authorizationKey is not null)
        {
            authorizationKey = authorizationKey.Remove(0, "Basic ".Length).Trim();
            var keys = await this.GetAuthorizationKeys();
            var kvp = keys.FirstOrDefault(kv => string.Equals(kv.Value, authorizationKey, StringComparison.Ordinal));
            if (kvp.Equals(default(KeyValuePair<string, string>)))
            {
                return Results.Unauthorized();
            }

            // Remove the authorization key file after successful authorizing.
            File.Delete(kvp.Key);
        }

        var context = new TokenRequestContext(new[] { resource + DefaultForScope });
        var accessToken = await this.tokenCredential.GetTokenAsync(context, CancellationToken.None);

        return Results.Ok(TokenResponse.FromString(accessToken.Token));
    }

    private async Task<string> AddNewAuthorizationKeyFile()
    {
        if (!Directory.Exists(TokensPath))
        {
            Directory.CreateDirectory(TokensPath);
        }

        var keyFileName = $"{Guid.NewGuid()}.key";
        var tokenFile = Path.Combine(TokensPath, keyFileName);
        await File.WriteAllTextAsync(tokenFile, Guid.NewGuid().ToString());
        return tokenFile;
    }

    private async Task<IDictionary<string, string>> GetAuthorizationKeys()
    {
        var existingFiles = Directory.GetFiles(TokensPath, "*.key");

        var keys = new Dictionary<string, string>();
        foreach (var file in existingFiles)
        {
            keys.Add(file, await File.ReadAllTextAsync(file));
        }

        return keys;
    }
}

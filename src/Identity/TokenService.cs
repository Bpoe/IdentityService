namespace Identity;

using System;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;

public class TokenService
{
    private const string BearerTokenType = "Bearer";
    private const string DefaultForScope = "/.default";

    private readonly TokenCredential tokenCredential;

    public TokenService(TokenCredential tokenCredential)
        => this.tokenCredential = tokenCredential ?? throw new ArgumentNullException(nameof(tokenCredential));

    public static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [HttpGet]
    public async Task<TokenResponse> Token([Required] string resource)
    {
        var request = new TokenRequestContext(new[] { resource + DefaultForScope });
        var accessToken = await tokenCredential.GetTokenAsync(request, CancellationToken.None);

        var jwt = new JwtSecurityToken(accessToken.Token);
        var oid = jwt.Claims.First(c => c.Type == "oid")?.Value ?? string.Empty;

        return new TokenResponse
        {
            AccessToken = accessToken.Token,
            TokenType = BearerTokenType,
            ExpiresOn = ToSecondsString(accessToken.ExpiresOn - Epoch),
            ExpiresIn = ToSecondsString(accessToken.ExpiresOn - DateTime.UtcNow),
            NotBefore = ToSecondsString(DateTime.UtcNow - Epoch),
            Resource = resource,
            ClientId = oid,
        };
    }

    private static string ToSecondsString(TimeSpan timeSpan)
        => ((long)timeSpan.TotalSeconds).ToString();
}

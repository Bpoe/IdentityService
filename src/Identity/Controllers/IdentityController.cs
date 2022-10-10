namespace Identity.Controllers;

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Models;

[Route("metadata/identity/oauth2/token")]
[ApiController]
public class TokenController : ControllerBase
{
    private const string BearerTokenType = "Bearer";
    private const string DefaultForScope = "/.default";

    private readonly IConfidentialClientApplication clientApplication;

    public static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public TokenController(IConfidentialClientApplication clientApplication)
        => this.clientApplication = clientApplication ?? throw new ArgumentNullException(nameof(clientApplication));

    [HttpGet]
    public async Task<ActionResult<TokenResponse>> Token([Required]string resource)
    {
        var result = await this.clientApplication
            .AcquireTokenForClient(new [] { resource + DefaultForScope })
            .ExecuteAsync();

        var tokenResponse = new TokenResponse()
        {
            AccessToken = result.AccessToken,
            TokenType = BearerTokenType,
            ExpiresOn = (result.ExpiresOn - Epoch).ToSecondsString(),
            ExpiresIn = (result.ExpiresOn - DateTime.UtcNow).ToSecondsString(),
            NotBefore = (DateTime.UtcNow - Epoch).ToSecondsString(),
            Resource = resource,
            ClientId = this.clientApplication.AppConfig.ClientId,
        };

        return tokenResponse;
    }
}

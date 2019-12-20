namespace Identity.Controllers
{
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

        public TokenController(IConfidentialClientApplication clientApplication)
        {
            this.clientApplication = clientApplication ?? throw new ArgumentNullException(nameof(clientApplication));
        }

        [HttpGet]
        public async Task<ActionResult<TokenResponse>> Token([Required]string resource)
        {
            var scope = new string[] { resource + DefaultForScope };
            var result = await this.clientApplication.AcquireTokenForClient(scope).ExecuteAsync();

            var tokenResponse = new TokenResponse()
            {
                AccessToken = result.AccessToken,
                TokenType = BearerTokenType,
                ExpiresOn = (result.ExpiresOn - Epoch.DateTime).ToSecondsString(),
                ExpiresIn = (result.ExpiresOn - DateTime.UtcNow).ToSecondsString(),
                NotBefore = (DateTime.UtcNow - Epoch.DateTime).ToSecondsString(),
                Resource = resource,
                ClientId = this.clientApplication.AppConfig.ClientId,
            };

            return tokenResponse;
        }
    }
}

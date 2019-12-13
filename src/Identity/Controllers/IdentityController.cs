namespace Identity.Controllers
{
    using System;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Models;

    [Route("metadata/identity/oauth2/token")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private readonly IOptions<MsiOptions> options;

        public TokenController(IOptions<MsiOptions> options)
        {
            if (options?.Value == null) throw new ArgumentNullException(nameof(options));

            this.options = options;
        }

        [HttpGet]
        public async Task<ActionResult<TokenResponse>> Token(string resource)
        {
            var tenantAuthority = $"{this.options.Value.Authority}{this.options.Value.TenantId}";

            //X509Certificate2 cert = null;
            //var clientAssert = new ClientAssertionCertificate(this.msiOptions.Value.ClientId, cert);
            var credential = new ClientCredential(this.options.Value.ClientId, this.options.Value.ClientSecret);
            var authContext = new AuthenticationContext(tenantAuthority);
            var result = await authContext.AcquireTokenAsync(resource, credential);

            var tokenResponse = new TokenResponse()
            {
                AccessToken = result.AccessToken,
                TokenType = result.AccessTokenType,
                ExpiresOn = (result.ExpiresOn - Epoch).ToSecondsString(),
                ExpiresIn = (result.ExpiresOn - DateTime.UtcNow).ToSecondsString(),
                NotBefore = (DateTime.UtcNow - Epoch).ToSecondsString(),
                Resource = resource,
                RefreshToken = string.Empty,
            };

            return tokenResponse;
        }
    }
}

namespace Identity.Controllers
{
    using System;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Models;

    [Route("metadata/identity/oauth2/token")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private static DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private IConfiguration configuration;

        public TokenController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [HttpGet]
        public async Task<ActionResult<TokenResponse>> Token(string resource)
        {
            var authority = this.configuration.GetValue<string>("MSI:Authority");
            var tenantId = this.configuration.GetValue<string>("MSI:TenantId");
            var clientId = this.configuration.GetValue<string>("MSI:ClientId");
            var clientSecret = this.configuration.GetValue<string>("MSI:ClientSecret");
            var clientCertificateCriteria = this.configuration.GetValue<string>("MSI:ClientCertificateCriteria");

            var tenantAuthority = $"{authority}{tenantId}";

            //X509Certificate2 cert = null;
            //var clientAssert = new ClientAssertionCertificate(this.clientId, cert);
            var credential = new ClientCredential(clientId, clientSecret);
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

namespace Identity;

using System;
using System.Net.Http;
using System.Threading.Tasks;

public class TenantIdResolver
{
    private readonly HttpClient httpClient;

    public TenantIdResolver(HttpClient httpClient)
        => this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

    public async Task<string> Resolve(string resource)
    {
        var response = await httpClient.GetAsync(resource);
        var challenge = HttpBearerChallenge.GetBearerChallengeFromResponse(response);
        if (challenge == null)
        {
            return string.Empty;
        }

        var uri = new Uri(challenge.AuthorizationServer);
        return uri.AbsolutePath.Trim('/');
    }
}
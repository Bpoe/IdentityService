namespace Identity;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

[DataContract]
public class TokenResponse
{
    private const string BearerTokenType = "Bearer";

    private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("client_id")]
    public string ClientId { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public string ExpiresIn { get; set; } = string.Empty;

    [JsonPropertyName("expires_on")]
    public string ExpiresOn { get; set; } = string.Empty;

    [JsonPropertyName("not_before")]
    public string NotBefore { get; set; } = string.Empty;

    [JsonPropertyName("resource")]
    public string Resource { get; set; } = string.Empty;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    public static TokenResponse FromString(string accessToken)
    {
        var jwt = new JwtSecurityToken(accessToken);
        var oid = jwt.Claims.First(c => c.Type == "oid")?.Value ?? string.Empty;

        return new TokenResponse
        {
            AccessToken = accessToken,
            TokenType = BearerTokenType,
            ExpiresOn = ToSecondsString(jwt.ValidTo - Epoch),
            ExpiresIn = ToSecondsString(jwt.ValidTo - jwt.ValidFrom),
            NotBefore = ToSecondsString(jwt.ValidFrom - Epoch),
            Resource = jwt?.Audiences?.FirstOrDefault() ?? string.Empty,
            ClientId = oid,
        };
    }

    private static string ToSecondsString(TimeSpan timeSpan)
        => ((long)timeSpan.TotalSeconds).ToString();
}
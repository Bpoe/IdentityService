namespace Identity;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Runtime.Serialization;

[DataContract]
public class TokenResponse
{
    private const string BearerTokenType = "Bearer";

    private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [DataMember(Name = "access_token", IsRequired = false)]
    public string AccessToken { get; set; } = string.Empty;

    [DataMember(Name = "client_id", IsRequired = false)]
    public string ClientId { get; set; } = string.Empty;

    [DataMember(Name = "expires_in", IsRequired = false)]
    public string ExpiresIn { get; set; } = string.Empty;

    [DataMember(Name = "expires_on", IsRequired = false)]
    public string ExpiresOn { get; set; } = string.Empty;

    [DataMember(Name = "not_before", IsRequired = false)]
    public string NotBefore { get; set; } = string.Empty;

    [DataMember(Name = "resource", IsRequired = false)]
    public string Resource { get; set; } = string.Empty;

    [DataMember(Name = "token_type", IsRequired = false)]
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
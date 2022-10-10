namespace Identity.Models;

using System.Runtime.Serialization;

[DataContract]
public class TokenResponse
{
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
}
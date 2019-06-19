namespace Identity.Models
{
    using System.Runtime.Serialization;

    [DataContract]
    public class TokenResponse
    {
        [DataMember(Name = "access_token", IsRequired = false)]
        public string AccessToken { get; set; }

        [DataMember(Name = "expires_on", IsRequired = false)]
        public string ExpiresOn { get; set; }

        [DataMember(Name = "expires_in", IsRequired = false)]
        public string ExpiresIn { get; set; }

        [DataMember(Name = "not_before", IsRequired = false)]
        public string NotBefore { get; set; }

        [DataMember(Name = "resource", IsRequired = false)]
        public string Resource { get; set; }

        [DataMember(Name = "token_type", IsRequired = false)]
        public string TokenType { get; set; }

        [DataMember(Name = "refresh_token", IsRequired = false)]
        public string RefreshToken { get; set; }
    }
}
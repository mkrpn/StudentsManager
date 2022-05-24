using Newtonsoft.Json;

namespace StudentsManager.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AccessTokenData
    {
        [JsonProperty("expires_in")]
        public int Expiration { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
    }
}

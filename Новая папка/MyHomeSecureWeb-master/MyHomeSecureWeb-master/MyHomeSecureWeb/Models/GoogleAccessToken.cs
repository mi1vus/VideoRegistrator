using Newtonsoft.Json;

namespace MyHomeSecureWeb.Models
{
    public class GoogleAccessToken
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
    }
}

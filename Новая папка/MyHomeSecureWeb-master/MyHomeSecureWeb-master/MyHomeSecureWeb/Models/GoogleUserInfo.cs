using Newtonsoft.Json;

namespace MyHomeSecureWeb.Models
{
    public class GoogleUserInfo
    {
        [JsonProperty("email")]
        public string Email { get; set; }
    }
}

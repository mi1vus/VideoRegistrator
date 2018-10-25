using Newtonsoft.Json;

namespace MyHomeSecureWeb.Models
{
    public class GoogleFileMetadata
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("parents")]
        public GoogleFileParent[] Parents { get; set; }

        [JsonProperty("mimeType")]
        public string MimeType { get; set; }
    }

    public class GoogleFileParent
    {
        [JsonProperty("kind")]
        public string Kind { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}

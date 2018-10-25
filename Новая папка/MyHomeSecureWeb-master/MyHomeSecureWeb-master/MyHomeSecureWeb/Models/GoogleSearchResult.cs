using Newtonsoft.Json;
using System;

namespace MyHomeSecureWeb.Models
{
    public class GoogleSearchResult
    {
        [JsonProperty("items")]
        public GoogleSearchItem[] Items { get; set; }
    }

    public class GoogleSearchItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("alternateLink")]
        public string AlternateLink { get; set; }

        [JsonProperty("createdDate")]
        public DateTime CreatedDate { get; set; }

        [JsonProperty("modifiedDate")]
        public DateTime ModifiedDate { get; set; }
    }
}

namespace Jellyfin.Plugin.Lastfm.Models
{
    using System.Text.Json.Serialization;
    
    public class MobileSession
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("subscriber")]
        public int Subscriber { get; set; }
    }
}

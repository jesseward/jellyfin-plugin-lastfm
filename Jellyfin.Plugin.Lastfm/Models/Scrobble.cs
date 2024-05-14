namespace Jellyfin.Plugin.Lastfm.Models
{
    using System.Text.Json.Serialization;

    public class Scrobbles
    {
        [JsonPropertyName("@attr")]
        public ScrobbleAttributes Attributes { get; set; }
    }

    public class ScrobbleAttributes
    {
        // https://www.last.fm/api/show/track.scrobble
        // accepted : Number of accepted scrobbles
        [JsonPropertyName("accepted")]
        public int Accepted { get; set; }

        // https://www.last.fm/api/show/track.scrobble
        // ignored : Number of ignored scrobbles (see ignoredMessage for details)
        [JsonPropertyName("ignored")]
        public int Ignored { get; set; }
    }
}

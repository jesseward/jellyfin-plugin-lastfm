namespace Jellyfin.Plugin.Lastfm.Models
{
    using System.Text.Json.Serialization;

    public class BaseLastfmTrack
    {
        [JsonPropertyName("artist")]
        public LastfmArtist Artist { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("mbid")]
        public string MusicBrainzId { get; set; }
    }

    public class LastfmArtist
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("mbid")]
        public string MusicBrainzId { get; set; }
    }

    public class LastfmLovedTrack : BaseLastfmTrack
    {
    }


    public class LastfmTrack : BaseLastfmTrack
    {
        [JsonPropertyName("playcount")]
        public int PlayCount { get; set; }
    }
}

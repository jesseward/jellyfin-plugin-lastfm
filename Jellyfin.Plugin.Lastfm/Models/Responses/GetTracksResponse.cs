namespace Jellyfin.Plugin.Lastfm.Models.Responses
{
    using System.Text.Json.Serialization;
    using System.Collections.Generic;
    
    public class GetTracksResponse : BaseResponse
    {
        [JsonPropertyName("tracks")]
        public GetTracksTracks Tracks { get; set; }

        public bool HasTracks()
        {
            return Tracks != null && Tracks.Tracks != null && Tracks.Tracks.Count > 0;
        }
    }

    public class GetTracksTracks
    {
        [JsonPropertyName("track")]
        public List<LastfmTrack> Tracks { get; set; }

        [JsonPropertyName("@attr")]
        public GetTracksMeta Metadata { get; set; }
    }

    public class GetTracksMeta
    {
        [JsonPropertyName("totalPages")]
        public int TotalPages { get; set; }

        [JsonPropertyName("total")]
        public int TotalTracks { get; set; }

        [JsonPropertyName("page")]
        public int Page { get; set; }

        public bool IsLastPage()
        {
            return Page.Equals(TotalPages);
        }
    }
}

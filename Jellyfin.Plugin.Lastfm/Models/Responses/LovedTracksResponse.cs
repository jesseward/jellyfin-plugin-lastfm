namespace Jellyfin.Plugin.Lastfm.Models.Responses
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class LovedTracksResponse : BaseResponse
    {
        [JsonPropertyName("lovedtracks")]
        public LovedTracks LovedTracks { get; set; }

        public bool HasLovedTracks()
        {
            return LovedTracks != null && LovedTracks.Tracks != null && LovedTracks.Tracks.Count > 0;
        }
    }

    public class LovedTracks
    {
        [JsonPropertyName("track")]
        public List<LastfmLovedTrack> Tracks { get; set; }


        [JsonPropertyName("@attr")]
        public LovedTracksMeta Metadata { get; set; }
    }


    public class LovedTracksMeta
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
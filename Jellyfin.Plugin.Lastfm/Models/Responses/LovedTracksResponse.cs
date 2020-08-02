namespace Jellyfin.Plugin.Lastfm.Models.Responses
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class LovedTracksResponse : BaseResponse
    {
        [DataMember(Name = "lovedtracks")]
        public LovedTracks LovedTracks { get; set; }

        public bool HasLovedTracks()
        {
            return LovedTracks != null && LovedTracks.Tracks != null && LovedTracks.Tracks.Count > 0;
        }
    }

    [DataContract]
    public class LovedTracks
    {
        [DataMember(Name = "track")]
        public List<LastfmLovedTrack> Tracks { get; set; }


        [DataMember(Name = "@attr")]
        public LovedTracksMeta Metadata { get; set; }
    }


    [DataContract]
    public class LovedTracksMeta
    {
        [DataMember(Name = "totalPages")]
        public int TotalPages { get; set; }

        [DataMember(Name = "total")]
        public int TotalTracks { get; set; }

        [DataMember(Name = "page")]
        public int Page { get; set; }

        public bool IsLastPage()
        {
            return Page.Equals(TotalPages);
        }
    }
}
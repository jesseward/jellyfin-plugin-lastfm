namespace Jellyfin.Plugin.Lastfm.Models.Requests
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class NowPlayingRequest : BaseAuthedRequest
    {
        public string Track { get; set; }
        public string Album { get; set; }
        public string Artist { get; set; }
        public int Duration { get; set; }
        public string MbId { get; set; }

        public override Dictionary<string, string> ToDictionary()
        {
            var nowPlaying = new Dictionary<string, string>(base.ToDictionary())
            {
                { "track",    Track  },
                { "artist",   Artist },
                { "duration", Duration.ToString() },
            };

            if (!string.IsNullOrWhiteSpace(Album))
            {
                nowPlaying.Add("album", Album);
            }
            if (!string.IsNullOrWhiteSpace(MbId))
            {
                nowPlaying.Add("mbid", MbId);
            }

            return nowPlaying;
        }
    }
}

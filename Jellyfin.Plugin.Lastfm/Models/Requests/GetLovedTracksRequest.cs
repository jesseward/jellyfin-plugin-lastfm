namespace Jellyfin.Plugin.Lastfm.Models.Requests
{
    using System.Collections.Generic;

    public class GetLovedTracksRequest : BaseRequest
    {
        public string User { get; set; }
        public int Limit { get; set; }
        public int Page { get; set; }

        public override Dictionary<string, string> ToDictionary()
        {
            return new Dictionary<string, string>(base.ToDictionary()) 
            {
                { "user", User },
                { "limit" , Limit.ToString() },
                { "page"  , Page.ToString()  }
            };
        }
    }
}

namespace Jellyfin.Plugin.Lastfm.Models.Responses
{
    using System.Text.Json.Serialization;

    public class BaseResponse
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("error")]
        public int ErrorCode { get; set; }

        public bool IsError()
        {
            return ErrorCode > 0;
        }
    }
}

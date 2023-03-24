using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.Lastfm.Api
{

    [ApiController]
    [Route("Lastfm/Login")]
    public class RestApi : ControllerBase
    {
        private readonly LastfmApiClient _apiClient;
        private readonly ILogger<RestApi> _logger;

        public RestApi(IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<RestApi>();
            _apiClient = new LastfmApiClient(httpClientFactory, _logger);
        }

        [HttpPost]
        [Consumes("application/json")]
        public object CreateMobileSession([FromBody] LastFMUser lastFMUser)
        {
            _logger.LogInformation("Fetching LastFM mobilesession auth for Username={0}", lastFMUser.Username);
            return _apiClient.RequestSession(lastFMUser.Username, lastFMUser.Password).Result;
        }
    }

    public class LastFMUser
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}

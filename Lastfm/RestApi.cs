namespace Lastfm
{
    using Api;
    using MediaBrowser.Common.Net;
    using MediaBrowser.Model.Serialization;
    using MediaBrowser.Model.Services;

    [Route("/Lastfm/Login", "POST")]
    public class Login
    {
        [ApiMember(Name = "Username", IsRequired = false, DataType = "string", ParameterType = "query", Verb = "POST")]
        public string Username { get; set; }
        [ApiMember(Name = "Password", IsRequired = false, DataType = "string", ParameterType = "query", Verb = "POST")]
        public string Password { get; set; }
    }

    public class RestApi : IService
    {
        private readonly LastfmApiClient _apiClient;

        public RestApi(IJsonSerializer jsonSerializer, IHttpClient httpClient)
        {
            _apiClient = new LastfmApiClient(httpClient, jsonSerializer);
        }

        public object Post(Login request)
        {
            return _apiClient.RequestSession(request.Username, request.Password).Result;
        }
    }
}

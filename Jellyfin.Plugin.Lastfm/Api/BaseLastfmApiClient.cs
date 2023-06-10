namespace Jellyfin.Plugin.Lastfm.Api
{
    using Models.Requests;
    using Models.Responses;
    using Resources;
    using System;
    using System.Collections.Generic;
    using System.Net.Http.Json;
    using System.Text.Json.Serialization;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Utils;
    using Microsoft.Extensions.Logging;

    public class BaseLastfmApiClient
    {
        private const string ApiVersion = "2.0";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public BaseLastfmApiClient(IHttpClientFactory httpClientFactory, ILogger logger)
        {
            _httpClientFactory = httpClientFactory;
            _httpClient = _httpClientFactory.CreateClient();
            _logger = logger;
        }

        /// <summary>
        /// Send a POST request to the LastFM Api
        /// </summary>
        /// <typeparam name="TRequest">The type of the request</typeparam>
        /// <typeparam name="TResponse">The type of the response</typeparam>
        /// <param name="request">The request</param>
        /// <returns>A response with type TResponse</returns>
        public async Task<TResponse> Post<TRequest, TResponse>(TRequest request) where TRequest : BaseRequest where TResponse : BaseResponse
        {
            var data = request.ToDictionary();

            // Append the signature
            Helpers.AppendSignature(ref data);

            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, BuildPostUrl(request.Secure));
            requestMessage.Content = new StringContent(SetPostData(data), Encoding.UTF8, "application/x-www-form-urlencoded");
            using (var response = await _httpClient.SendAsync(requestMessage, CancellationToken.None))
            {
                var serializeOptions = new JsonSerializerOptions
                {
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                };
                try
                {
                    var result = await response.Content.ReadFromJsonAsync<TResponse>(serializeOptions);
                    // Lets Log the error here to ensure all errors are logged
                    if (result.IsError())
                        _logger.LogError(result.Message);

                    return result;
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                }
            }

            return null;
        }

        public async Task<TResponse> Get<TRequest, TResponse>(TRequest request) where TRequest : BaseRequest where TResponse : BaseResponse
        {
            return await Get<TRequest, TResponse>(request, CancellationToken.None);
        }

        public async Task<TResponse> Get<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken) where TRequest : BaseRequest where TResponse : BaseResponse
        {
            using (var response = await _httpClient.GetAsync(BuildGetUrl(request.ToDictionary(), request.Secure), cancellationToken))
            {
                var serializeOptions = new JsonSerializerOptions
                {
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                };
                try
                {
                    var result = await response.Content.ReadFromJsonAsync<TResponse>(serializeOptions);

                    // Lets Log the error here to ensure all errors are logged
                    if (result.IsError())
                        _logger.LogError(result.Message);

                    return result;
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                }
                return null;
            }
        }

        #region Private methods
        private static string BuildGetUrl(Dictionary<string, string> requestData, bool secure)
        {
            return String.Format("{0}://{1}/{2}/?format=json&{3}",
                                    secure ? "https" : "http",
                                    Strings.Endpoints.LastfmApi,
                                    ApiVersion,
                                    Helpers.DictionaryToQueryString(requestData)
                                );
        }

        private static string BuildPostUrl(bool secure)
        {
            return String.Format("{0}://{1}/{2}/?format=json",
                                    secure ? "https" : "http",
                                    Strings.Endpoints.LastfmApi,
                                    ApiVersion
                                );
        }

        private static string SetPostData(Dictionary<string, string> dic)
        {
            var strings = dic.Keys.Select(key => string.Format("{0}={1}", key, Uri.EscapeDataString(dic[key])));
            return string.Join("&", strings.ToArray());

        }
        #endregion
    }
}


namespace Jellyfin.Plugin.Lastfm.Api
{
    using MediaBrowser.Common.Net;
    using MediaBrowser.Model.Serialization;
    using Models.Requests;
    using Models.Responses;
    using Resources;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Utils;
    using Microsoft.Extensions.Logging;

    public class BaseLastfmApiClient
    {
        private const string ApiVersion = "2.0";

        private readonly IHttpClient _httpClient;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly ILogger _logger;


        public BaseLastfmApiClient(IHttpClient httpClient, IJsonSerializer jsonSerializer, ILogger logger)
        {
            _httpClient = httpClient;
            _jsonSerializer = jsonSerializer;
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

            var options = new HttpRequestOptions
            {
                Url = BuildPostUrl(request.Secure),
                CancellationToken = CancellationToken.None,
                EnableHttpCompression = false,
            };

            options.SetPostData(EscapeDictionary(data));
            using (var response = await _httpClient.Post(options))
            {
                using (var stream = response.Content)
                {
                    try
                    {
                        var result = _jsonSerializer.DeserializeFromStream<TResponse>(stream);
                        // Lets Log the error here to ensure all errors are logged
                        if (result.IsError())
                            _logger.LogError(result.Message);

                        return result;
                    }
                    catch (Exception e)
                    {
                        _logger.LogDebug(e.Message);
                    }
                }

                return null;
            }
        }

        public async Task<TResponse> Get<TRequest, TResponse>(TRequest request) where TRequest : BaseRequest where TResponse : BaseResponse
        {
            return await Get<TRequest, TResponse>(request, CancellationToken.None);
        }

        public async Task<TResponse> Get<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken) where TRequest : BaseRequest where TResponse : BaseResponse
        {
            using (var stream = await _httpClient.Get(new HttpRequestOptions
            {
                Url = BuildGetUrl(request.ToDictionary(), request.Secure),
                CancellationToken = cancellationToken,
                EnableHttpCompression = false,
            }))
            {
                try
                {
                    var result = _jsonSerializer.DeserializeFromStream<TResponse>(stream);

                    // Lets Log the error here to ensure all errors are logged
                    if (result.IsError())
                        _logger.LogError(result.Message);

                    return result;
                }
                catch (Exception e)
                {
                    _logger.LogDebug(e.Message);
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

        private Dictionary<string, string> EscapeDictionary(Dictionary<string, string> dic)
        {
            return dic.ToDictionary(item => item.Key, item => Uri.EscapeDataString(item.Value));
        }
        #endregion
    }
}

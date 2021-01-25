using MediaBrowser.Common.Extensions;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using MediaBrowser.Model.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Lastfm.Providers
{
    public class LastfmArtistProvider : IRemoteMetadataProvider<MusicArtist, ArtistInfo>, IHasOrder
    {
        private readonly IJsonSerializer _json;
        private readonly IHttpClientFactory _httpClientFactory;

        internal const string RootUrl = @"http://ws.audioscrobbler.com/2.0/?";
        internal static string ApiKey = "7b76553c3eb1d341d642755aecc40a33";

        private readonly IServerConfigurationManager _config;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<LastfmArtistProvider> _logger;

        public LastfmArtistProvider(IHttpClientFactory httpClientFactory, IJsonSerializer json, IServerConfigurationManager config, ILoggerFactory loggerFactory)
        {
            _httpClientFactory = httpClientFactory;
            _json = json;
            _config = config;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<LastfmArtistProvider>();
        }

        public Task<IEnumerable<RemoteSearchResult>> GetSearchResults(ArtistInfo searchInfo, CancellationToken cancellationToken)
        {
            return Task.FromResult<IEnumerable<RemoteSearchResult>>(new List<RemoteSearchResult>());
        }

        public async Task<MetadataResult<MusicArtist>> GetMetadata(ArtistInfo id, CancellationToken cancellationToken)
        {
            var result = new MetadataResult<MusicArtist>();

            var musicBrainzId = id.GetMusicBrainzArtistId();

            if (!String.IsNullOrWhiteSpace(musicBrainzId))
            {
                cancellationToken.ThrowIfCancellationRequested();

                result.Item = new MusicArtist();
                result.HasMetadata = true;

                await FetchLastfmData(result.Item, musicBrainzId, cancellationToken).ConfigureAwait(false);
            }

            return result;
        }

        protected async Task FetchLastfmData(MusicArtist item, string musicBrainzId, CancellationToken cancellationToken)
        {
            // Get artist info with provided id
            var url = RootUrl + String.Format("method=artist.getInfo&mbid={0}&api_key={1}&format=json", UrlEncode(musicBrainzId), ApiKey);

            LastfmGetArtistResult result;

            using (var response = await _httpClientFactory.CreateClient().GetAsync(url, cancellationToken).ConfigureAwait(false))
            {
                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    using (var reader = new StreamReader(stream))
                    {
                        var jsonText = await reader.ReadToEndAsync().ConfigureAwait(false);

                        // Fix their bad json
                        jsonText = jsonText.Replace("\"#text\"", "\"url\"");

                        result = _json.DeserializeFromString<LastfmGetArtistResult>(jsonText);
                    }
                }
            }

            if (result != null && result.artist != null)
            {
                ProcessArtistData(item, result.artist, musicBrainzId);
            }
        }

        private void ProcessArtistData(MusicArtist artist, LastfmArtist data, string musicBrainzId)
        {
            var yearFormed = 0;

            if (data.bio != null)
            {
                Int32.TryParse(data.bio.yearformed, out yearFormed);
                if (!artist.LockedFields.Contains(MetadataField.Overview))
                {
                    artist.Overview = (data.bio.content ?? string.Empty).StripHtml();
                }
                if (!string.IsNullOrEmpty(data.bio.placeformed) && !artist.LockedFields.Contains(MetadataField.ProductionLocations))
                {
                    artist.ProductionLocations = new[] { data.bio.placeformed };
                }
            }

            if (yearFormed > 0)
            {
                artist.PremiereDate = new DateTime(yearFormed, 1, 1, 0, 0, 0, DateTimeKind.Utc);

                artist.ProductionYear = yearFormed;
            }

        }

        /// <summary>
        /// Encodes an URL.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>System.String.</returns>
        private string UrlEncode(string name)
        {
            return WebUtility.UrlEncode(name);
        }

        public string Name
        {
            get { return "last.fm"; }
        }

        public int Order
        {
            get
            {
                // After fanart & audiodb
                return 2;
            }
        }

        public Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}

namespace Jellyfin.Plugin.Lastfm.Api
{
    using MediaBrowser.Controller.Entities.Audio;
    using MediaBrowser.Model.Serialization;
    using Models;
    using Models.Requests;
    using Models.Responses;
    using Resources;
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Utils;
    using Microsoft.Extensions.Logging;

    public class LastfmApiClient : BaseLastfmApiClient
    {
        private readonly ILogger _logger;


        public LastfmApiClient(IHttpClientFactory httpClientFactory, ILogger logger) : base(httpClientFactory, logger)
        {
            _logger = logger;
        }



        public async Task<MobileSessionResponse> RequestSession(string username, string password)
        {
            //Build request object
            var request = new MobileSessionRequest
            {
                Username = username,
                Password = password,

                ApiKey = Strings.Keys.LastfmApiKey,
                Method = Strings.Methods.GetMobileSession,
                Secure = true
            };

            var response = await Post<MobileSessionRequest, MobileSessionResponse>(request);


            return response;
        }

        public async Task Scrobble(Audio item, LastfmUser user)
        {
            // API docs -> https://www.last.fm/api/show/track.scrobble
            var request = new ScrobbleRequest
            {
                Track = item.Name,
                Artist = item.Artists.First(),
                Timestamp = Helpers.CurrentTimestamp(),

                ApiKey = Strings.Keys.LastfmApiKey,
                Method = Strings.Methods.Scrobble,
                SessionKey = user.SessionKey,
                Secure = true
            };

            if (!string.IsNullOrWhiteSpace(item.Album))
            {
                request.Album = item.Album;
            }
            if (item.ProviderIds.ContainsKey("MusicBrainzTrack"))
            {
                request.MbId = item.ProviderIds["MusicBrainzTrack"];
            }

            try
            {
                // Send the request
                var response = await Post<ScrobbleRequest, ScrobbleResponse>(request);
                if (response != null && !response.IsError())
                {
                    _logger.LogInformation("{0} played artist={1}, track={2}, album={3}", user.Username, request.Artist, request.Track, request.Album);
                    return;
                }

                _logger.LogError("Failed to Scrobble track: {0}", item.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to Scrobble track: track: ex={0}, name={1}, track={2}, artist={3}, album={4}, mbid={5}", ex, item.Name, request.Track, request.Artist, request.Album, request.MbId);
            }
        }

        public async Task NowPlaying(Audio item, LastfmUser user)
        {
            var request = new NowPlayingRequest
            {
                Track = item.Name,
                Artist = item.Artists.First(),

                ApiKey = Strings.Keys.LastfmApiKey,
                Method = Strings.Methods.NowPlaying,
                SessionKey = user.SessionKey,
                Secure = true
            };


            if (!string.IsNullOrWhiteSpace(item.Album))
            {
                request.Album = item.Album;
            }
            if (item.ProviderIds.ContainsKey("MusicBrainzTrack"))
            {
                request.MbId = item.ProviderIds["MusicBrainzTrack"];
            }

            // Add duration
            if (item.RunTimeTicks != null)
                request.Duration = Convert.ToInt32(TimeSpan.FromTicks((long)item.RunTimeTicks).TotalSeconds);

            try
            {
                var response = await Post<NowPlayingRequest, ScrobbleResponse>(request);
                if (response != null && !response.IsError())
                {
                    _logger.LogInformation("{0} is now playing artist={1}, track={2}, album={3}", user.Username, request.Artist, request.Track, request.Album);
                    return;
                }

                _logger.LogError("Failed to send now playing for track: {0}", item.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to send now playing for track: ex={0}, name={1}, track={2}, artist={3}, album={4}, mbid={5}", ex, item.Name, request.Track, request.Artist, request.Album, request.MbId);
            }
        }

        /// <summary>
        /// Loves or unloves a track
        /// </summary>
        /// <param name="item">The track</param>
        /// <param name="user">The Lastfm User</param>
        /// <param name="love">If the track is loved or not</param>
        /// <returns></returns>
        public async Task<bool> LoveTrack(Audio item, LastfmUser user, bool love = true)
        {
            var request = new TrackLoveRequest
            {
                Artist = item.Artists.First(),
                Track = item.Name,

                ApiKey = Strings.Keys.LastfmApiKey,
                Method = love ? Strings.Methods.TrackLove : Strings.Methods.TrackUnlove,
                SessionKey = user.SessionKey,
                Secure = true
            };

            try
            {
                //Send the request
                var response = await Post<TrackLoveRequest, BaseResponse>(request);

                if (response != null && !response.IsError())
                {
                    _logger.LogInformation("{0} {2}loved track '{1}'", user.Username, item.Name, (love ? "" : "un"));
                    return true;
                }

                _logger.LogError("{0} Failed to love = {3} track '{1}' - {2}", user.Username, item.Name, response.Message, love);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{user.Username} Failed to love = {love} track '{item.Name}'");
                _logger.LogError($"{ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Unlove a track. This is the same as LoveTrack with love as false
        /// </summary>
        /// <param name="item">The track</param>
        /// <param name="user">The Lastfm User</param>
        /// <returns></returns>
        public async Task<bool> UnloveTrack(Audio item, LastfmUser user)
        {
            return await LoveTrack(item, user, false);
        }

        public async Task<LovedTracksResponse> GetLovedTracks(LastfmUser user, CancellationToken cancellationToken, int page)
        {
            var request = new GetLovedTracksRequest
            {
                User = user.Username,
                ApiKey = Strings.Keys.LastfmApiKey,
                Method = Strings.Methods.GetLovedTracks,
                Limit = 1000, // {"error":6,"message":"limit param out of bounds (1-1000)"}
                Page = page,
                Secure = true
            };

            return await Get<GetLovedTracksRequest, LovedTracksResponse>(request, cancellationToken);
        }

        public async Task<GetTracksResponse> GetTracks(LastfmUser user, MusicArtist artist, CancellationToken cancellationToken)
        {
            var request = new GetTracksRequest
            {
                User = user.Username,
                Artist = artist.Name,
                ApiKey = Strings.Keys.LastfmApiKey,
                Method = Strings.Methods.GetTracks,
                Limit = 1000,
                Secure = true
            };

            return await Get<GetTracksRequest, GetTracksResponse>(request, cancellationToken);
        }

        public async Task<GetTracksResponse> GetTracks(LastfmUser user, CancellationToken cancellationToken, int page = 0, int limit = 200)
        {
            var request = new GetTracksRequest
            {
                User = user.Username,
                ApiKey = Strings.Keys.LastfmApiKey,
                Method = Strings.Methods.GetTracks,
                Limit = limit,
                Page = page,
                Secure = true

            };

            return await Get<GetTracksRequest, GetTracksResponse>(request, cancellationToken);
        }
    }
}

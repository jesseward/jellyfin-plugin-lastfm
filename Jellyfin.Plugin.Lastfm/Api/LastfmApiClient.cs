namespace Jellyfin.Plugin.Lastfm.Api
{
    using MediaBrowser.Controller.Entities.Audio;
    using Models;
    using Models.Requests;
    using Models.Responses;
    using Resources;
    using System;
    using Microsoft.Extensions.Caching.Memory;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Utils;
    using Microsoft.Extensions.Logging;

    public class LastfmApiClient : BaseLastfmApiClient
    {
        private readonly ILogger _logger;

        private static readonly TimeSpan DuplicateScrobbleTTL = TimeSpan.FromSeconds(15);
        private readonly MemoryCache _scrobbleCache = new(new MemoryCacheOptions());
        private readonly object _scrobbleLock = new();

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
            if (CheckAndUpdateScrobbleCache(user.Username, item.Id.ToString()))
            {
                return;
            }

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
            var albumArtist = item.AlbumArtists.First();
            if (!string.IsNullOrWhiteSpace(albumArtist) && albumArtist != request.Artist)
            {
                request.AlbumArtist = albumArtist;
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
                _logger.LogError("Failed to Scrobble track: track: ex={0}, name={1}, track={2}, artist={3}, album={4}, albumArtist={5}, mbid={6}", ex, item.Name, request.Track, request.Artist, request.Album, request.AlbumArtist, request.MbId);
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
            var albumArtist = item.AlbumArtists.First();
            if (!string.IsNullOrWhiteSpace(albumArtist) && albumArtist != request.Artist)
            {
                request.AlbumArtist = albumArtist;
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
                _logger.LogError("Failed to send now playing for track: ex={0}, name={1}, track={2}, artist={3}, album={4}, albumArtist={5}, mbid={6}", ex, item.Name, request.Track, request.Artist, request.Album, request.AlbumArtist, request.MbId);
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
            var artist = item.Artists.FirstOrDefault();
            if (artist == null) {
                return false;
            }

            var request = new TrackLoveRequest
            {
                Artist = artist,
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
                    _logger.LogInformation("{Username} {LovedState}loved track '{Track}'", user.Username,  love ? "" : "un", item.Name);
                    return true;
                }

                _logger.LogError("{Username} Failed to {LoveState}love track '{Track}' - {Response}", user.Username, love ? "" : "un", item.Name, response.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError("{0} Failed to love = {3} track '{2}' - {1}", user.Username, ex, item.Name, love);
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

        /// <summary>
        /// Checks for duplicate scrobble and updates cache if not duplicate.
        /// Even though MemoryCache is thread-safe, we use the _scrobbleLock to ensure thread safety for the whole check-and-set operation.
        /// The method also updates the cache with the new scrobble if it's not a duplicate.
        /// Returns true if duplicate, false otherwise.
        /// </summary>
        private bool CheckAndUpdateScrobbleCache(string username, string trackId)
        {
            var cacheKey = $"{username}:{trackId}";
            lock (_scrobbleLock)
            {
                if (_scrobbleCache.TryGetValue(cacheKey, out _))
                {
                    _logger.LogInformation("Duplicate scrobble detected for user={0}, trackId={1} within {2} seconds. Skipping.", username, trackId, DuplicateScrobbleTTL.TotalSeconds);
                    return true;
                }
                _scrobbleCache.Set(cacheKey, true, DuplicateScrobbleTTL);
                return false;
            }
        }
    }
}

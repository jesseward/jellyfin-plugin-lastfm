namespace Jellyfin.Plugin.Lastfm
{
    using Api;
    using MediaBrowser.Controller.Entities.Audio;
    using MediaBrowser.Controller.Library;
    using MediaBrowser.Controller.Session;
    using MediaBrowser.Model.Entities;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Hosting;
    using System.Threading;

    /// <summary>
    /// Class ServerEntryPoint
    /// </summary>
    public class ServerEntryPoint : IHostedService, IDisposable
    {

        // if the length of the song is >= 30 seconds, allow scrobble.
        private const long minimumSongLengthToScrobbleInTicks = 30 * TimeSpan.TicksPerSecond;
        // if a song reaches >= 4 minutes  in playtime, allow scrobble.
        private const long minimumPlayTimeToScrobbleInTicks = 4 * TimeSpan.TicksPerMinute;
        // if a song reaches >= 50% played, allow scrobble.
        private const double minimumPlayPercentage = 50.00;

        private readonly ISessionManager _sessionManager;
        private readonly IUserDataManager _userDataManager;

        private LastfmApiClient _apiClient;
        private readonly ILogger<ServerEntryPoint> _logger;

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static ServerEntryPoint Instance { get; private set; }

        public ServerEntryPoint(
            ISessionManager sessionManager,
            IHttpClientFactory httpClientFactory,
            ILoggerFactory loggerFactory,
            IUserDataManager userDataManager)
        {
            _logger = loggerFactory.CreateLogger<ServerEntryPoint>();

            _sessionManager = sessionManager;
            _userDataManager = userDataManager;
            _apiClient = new LastfmApiClient(httpClientFactory, _logger);
            Instance = this;
        }

        /// <summary>
        /// Let last fm know when a user favourites or unfavourites a track
        /// </summary>
        async void UserDataSaved(object sender, UserDataSaveEventArgs e)
        {
            // We only care about audio
            if (e.Item is not Audio)
                return;

            // We also only care about User rating changes
            if (!e.SaveReason.Equals(UserDataSaveReason.UpdateUserRating))
                return;

            var lastfmUser = Utils.UserHelpers.GetUser(e.UserId);
            if (lastfmUser == null)
            {
                _logger.LogDebug("Could not find user");
                return;
            }

            if (string.IsNullOrWhiteSpace(lastfmUser.SessionKey))
            {
                _logger.LogInformation("No session key present, aborting");
                return;
            }

            var item = e.Item as Audio;

            // Dont do if syncing
            if (Plugin.Syncing)
                return;

            if (!lastfmUser.Options.SyncFavourites)
            {
                _logger.LogDebug("{0} does not want to sync liked songs", lastfmUser.Username);
                return;
            }

            await _apiClient.LoveTrack(item, lastfmUser, e.UserData.IsFavorite).ConfigureAwait(false);
        }


        /// <summary>
        /// Let last.fm know when a track has finished.
        /// Playback stopped is run when a track is finished.
        /// </summary>
        private async void PlaybackStopped(object sender, PlaybackStopEventArgs e)
        {
            // We only care about audio
            if (e.Item is not Audio)
                return;

            var item = e.Item as Audio;

            if (e.PlaybackPositionTicks == null)
            {
                _logger.LogDebug("Playback ticks for {0} is null", item.Name);
                return;
            }

            // Required checkpoints before scrobbling noted at https://www.last.fm/api/scrobbling#when-is-a-scrobble-a-scrobble .
            // A track should only be scrobbled when the following conditions have been met:
            //   * The track must be longer than 30 seconds.
            //   * And the track has been played for at least half its duration, or for 4 minutes (whichever occurs earlier.)
            // is the track length greater than 30 seconds.
            if (item.RunTimeTicks < minimumSongLengthToScrobbleInTicks)
            {
                _logger.LogDebug("{0} - played {1} ticks which is less minimumSongLengthToScrobbleInTicks ({2}), won't scrobble.", item.Name, item.RunTimeTicks, minimumSongLengthToScrobbleInTicks);
                return;
            }

            // the track must have played the minimum percentage (minimumPlayPercentage = 50%) or played for atleast 4 minutes (minimumPlayTimeToScrobbleInTicks).
            var playPercent = ((double)e.PlaybackPositionTicks / item.RunTimeTicks) * 100;
            if (playPercent < minimumPlayPercentage & e.PlaybackPositionTicks < minimumPlayTimeToScrobbleInTicks)
            {
                _logger.LogDebug("{0} - played {1}%, Last.Fm requires minplayed={2}% . played {3} ticks of minimumPlayTimeToScrobbleInTicks ({4}), won't scrobble", item.Name, playPercent, minimumPlayPercentage, e.PlaybackPositionTicks, minimumPlayTimeToScrobbleInTicks);
                return;
            }

            var user = e.Users.FirstOrDefault();
            if (user == null)
            {
                return;
            }

            var lastfmUser = Utils.UserHelpers.GetUser(user);
            if (lastfmUser == null)
            {
                _logger.LogDebug("Could not find last.fm user");
                return;
            }

            // User doesn't want to scrobble
            if (!lastfmUser.Options.Scrobble)
            {
                _logger.LogDebug("{0} ({1}) does not want to scrobble", user.Username, lastfmUser.Username);
                return;
            }

            if (string.IsNullOrWhiteSpace(lastfmUser.SessionKey))
            {
                _logger.LogInformation("No session key present, aborting");
                return;
            }

            if (string.IsNullOrWhiteSpace(item.Artists.FirstOrDefault()) || string.IsNullOrWhiteSpace(item.Name))
            {
                _logger.LogInformation("track {0} is missing  artist ({1}) or track name ({2}) metadata. Not submitting", item.Path, item.Artists.FirstOrDefault(), item.Name);
                return;
            }
            await _apiClient.Scrobble(item, lastfmUser).ConfigureAwait(false);
        }

        /// <summary>
        /// Let Last.fm know when a user has started listening to a track
        /// </summary>
        private async void PlaybackStart(object sender, PlaybackProgressEventArgs e)
        {
            // We only care about audio
            if (e.Item is not Audio)
                return;

            var user = e.Users.FirstOrDefault();
            if (user == null)
            {
                return;
            }

            var lastfmUser = Utils.UserHelpers.GetUser(user);
            if (lastfmUser == null)
            {
                _logger.LogDebug("Could not find last.fm user");
                return;
            }

            // User doesn't want to scrobble
            if (!lastfmUser.Options.Scrobble)
            {
                _logger.LogDebug("{0} ({1}) does not want to scrobble", user.Username, lastfmUser.Username);
                return;
            }

            if (string.IsNullOrWhiteSpace(lastfmUser.SessionKey))
            {
                _logger.LogInformation("No session key present, aborting");
                return;
            }

            var item = e.Item as Audio;
            if (string.IsNullOrWhiteSpace(item.Artists.FirstOrDefault()) || string.IsNullOrWhiteSpace(item.Name))
            {
                _logger.LogInformation("track {0} is missing artist ({1}) or track name ({2}) metadata. Not submitting", item.Path, item.Artists.FirstOrDefault(), item.Name);
                return;
            }
            await _apiClient.NowPlaying(item, lastfmUser).ConfigureAwait(false);
        }

        /// <summary>
        /// Runs this instance.
        /// </summary>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            //Bind events
            _sessionManager.PlaybackStart += PlaybackStart;
            _sessionManager.PlaybackStopped += PlaybackStopped;
            _userDataManager.UserDataSaved += UserDataSaved;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            // Unbind events
            _sessionManager.PlaybackStart -= PlaybackStart;
            _sessionManager.PlaybackStopped -= PlaybackStopped;
            _userDataManager.UserDataSaved -= UserDataSaved;

            // Clean up
            _apiClient = null;
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}

using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.Lastfm.Api;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Lastfm
{
    /// <summary>
    /// All communication between the server and the plugins server instance should occur in this class.
    /// </summary>
    public sealed class ServerEntryPoint : IHostedService, IDisposable
    {
        // if the length of the song is >= 30 seconds, allow scrobble.
        private const long MinimumSongLengthToScrobbleInTicks = 30 * TimeSpan.TicksPerSecond;
        // if a song reaches >= 4 minutes  in playtime, allow scrobble.
        private const long MinimumPlayTimeToScrobbleInTicks = 4 * TimeSpan.TicksPerMinute;
        // if a song reaches >= 50% played, allow scrobble.
        private const double MinimumPlayPercentage = 50.00;

        private readonly ISessionManager _sessionManager;
        private readonly IUserDataManager _userDataManager;
        private readonly ILogger<ServerEntryPoint> _logger;
        private LastfmApiClient _apiClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerEntryPoint"/> class.
        /// </summary>
        /// <param name="sessionManager">The <see cref="ISessionManager"/>.</param>
        /// <param name="httpClientFactory">The <see cref="IHttpClientFactory"/>.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        /// <param name="userDataManager">The <see cref="IUserDataManager"/>.</param>
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
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static ServerEntryPoint? Instance { get; private set; }

        /// <summary>
        /// User data was saved.
        /// </summary>
        /// <param name="sender">The sending entity.</param>
        /// <param name="userDataSaveEventArgs">The <see cref="UserDataSaveEventArgs"/>.</param>
        private async void UserDataSaved(object? sender, UserDataSaveEventArgs userDataSaveEventArgs)
        {
            // We only care about audio
            if (userDataSaveEventArgs.Item is not Audio)
            {
                return;
            }

            // We also only care about User rating changes
            if (!userDataSaveEventArgs.SaveReason.Equals(UserDataSaveReason.UpdateUserRating))
            {
                return;
            }

            var lastfmUser = Utils.UserHelpers.GetUser(userDataSaveEventArgs.UserId);
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

            var item = userDataSaveEventArgs.Item as Audio;
            if (item == null)
            {
                return;
            }

            // Dont do if syncing
            if (Plugin.Syncing)
            {
                return;
            }

            await _apiClient.LoveTrack(item, lastfmUser, userDataSaveEventArgs.UserData.IsFavorite).ConfigureAwait(false);
        }

        /// <summary>
        /// Let last.fm know when a track has finished.
        /// Playback stopped is run when a track is finished.
        /// </summary>
        private async void PlaybackStopped(object? sender, PlaybackStopEventArgs e)
        {
            // We only care about audio
            if (e.Item is not Audio)
            {
                return;
            }

            var item = e.Item as Audio;
            if (item == null)
            {
                return;
            }

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
            if (item.RunTimeTicks < MinimumSongLengthToScrobbleInTicks)
            {
                _logger.LogDebug("{0} - played {1} ticks which is less minimumSongLengthToScrobbleInTicks ({2}), won't scrobble.", item.Name, item.RunTimeTicks, MinimumSongLengthToScrobbleInTicks);
                return;
            }

            // the track must have played the minimum percentage (minimumPlayPercentage = 50%) or played for atleast 4 minutes (minimumPlayTimeToScrobbleInTicks).
            var playPercent = ((double)e.PlaybackPositionTicks / item.RunTimeTicks) * 100;
            if (playPercent < MinimumPlayPercentage & e.PlaybackPositionTicks < MinimumPlayTimeToScrobbleInTicks)
            {
                _logger.LogDebug("{0} - played {1}%, Last.Fm requires minplayed={2}% . played {3} ticks of minimumPlayTimeToScrobbleInTicks ({4}), won't scrobble", item.Name, playPercent, MinimumPlayPercentage, e.PlaybackPositionTicks, MinimumPlayTimeToScrobbleInTicks);
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

            if (string.IsNullOrWhiteSpace(item.Artists[0]) || string.IsNullOrWhiteSpace(item.Name))
            {
                _logger.LogInformation("track {0} is missing  artist ({1}) or track name ({2}) metadata. Not submitting", item.Path, item.Artists[0], item.Name);
                return;
            }

            await _apiClient.Scrobble(item, lastfmUser).ConfigureAwait(false);
        }

        /// <summary>
        /// Let Last.fm know when a user has started listening to a track.
        /// </summary>
        private async void PlaybackStart(object? sender, PlaybackProgressEventArgs e)
        {
            // We only care about audio
            if (e.Item is not Audio)
            {
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

            var item = e.Item as Audio;
            if (item == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(item.Artists[0]) || string.IsNullOrWhiteSpace(item.Name))
            {
                _logger.LogInformation("track {0} is missing artist ({1}) or track name ({2}) metadata. Not submitting", item.Path, item.Artists[0], item.Name);
                return;
            }

            await _apiClient.NowPlaying(item, lastfmUser).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Bind events
            _sessionManager.PlaybackStart += PlaybackStart;
            _sessionManager.PlaybackStopped += PlaybackStopped;
            _userDataManager.UserDataSaved += UserDataSaved;
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            // Unbind events
            _sessionManager.PlaybackStart -= PlaybackStart;
            _sessionManager.PlaybackStopped -= PlaybackStopped;
            _userDataManager.UserDataSaved -= UserDataSaved;

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}

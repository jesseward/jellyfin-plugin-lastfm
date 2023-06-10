namespace Jellyfin.Plugin.Lastfm.ScheduledTasks
{
    using Api;
    using Jellyfin.Data.Entities;
    using Jellyfin.Data.Enums;
    using MediaBrowser.Model.Tasks;
    using MediaBrowser.Controller.Entities;
    using MediaBrowser.Controller.Entities.Audio;
    using MediaBrowser.Controller.Library;
    using MediaBrowser.Model.Entities;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Utils;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Task that will sync each users LastFM loved songs with their local library.
    /// </summary>
    public class ImportLastfmData : IScheduledTask
    {
        private readonly IUserManager _userManager;
        private readonly IUserDataManager _userDataManager;
        private ILibraryManager _libraryManager;
        private readonly ILogger<ImportLastfmData> _logger;
        private readonly LastfmApiClient _apiClient;

        public ImportLastfmData(IHttpClientFactory httpClientFactory, IUserManager userManager, IUserDataManager userDataManager, ILibraryManager libraryManager, ILoggerFactory loggerFactory)
        {
            _userManager = userManager;
            _userDataManager = userDataManager;
            _libraryManager = libraryManager;
            _logger = loggerFactory.CreateLogger<ImportLastfmData>();
            _apiClient = new LastfmApiClient(httpClientFactory, loggerFactory.CreateLogger<ImportLastfmData>());
        }

        public string Name => "Import Last.fm Loved Tracks";

        public string Category => "Last.fm";

        public string Key => "ImportLastfmData";

        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers() => Enumerable.Empty<TaskTriggerInfo>();

        public string Description => "Import favourite tracks for each user with Last.fm accounted configured";

        /// <summary>
        /// Gather users information and calls <see cref="SyncDataforUserByArtistBulk"/>
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="progress"></param>
        public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
        {
            //Get all users
            var users = _userManager.Users.Where(u =>
            {
                var user = UserHelpers.GetUser(u);
                return user != null && !String.IsNullOrWhiteSpace(user.SessionKey);
            }).ToList();

            if (users.Count == 0)
            {
                _logger.LogInformation("No users found");
                return;
            }

            Plugin.Syncing = true;

            var usersProcessed = 0;
            var totalUsers = users.Count;

            foreach (var user in users)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var progressOffset = ((double)usersProcessed++ / totalUsers);
                var maxProgressForStage = ((double)usersProcessed / totalUsers);


                await SyncDataforUserByArtistBulk(user, progress, cancellationToken, maxProgressForStage, progressOffset);
            }

            Plugin.Syncing = false;
        }

        private async Task SyncDataforUserByArtistBulk(User user, IProgress<double> progress, CancellationToken cancellationToken, double maxProgress, double progressOffset)
        {

            LastfmUser lastFmUser = UserHelpers.GetUser(user);
            if (!lastFmUser.Options.SyncFavourites)
            {
                return;
            }

            _logger.LogInformation("Syncing LastFM favourties for {0}", user.Username);

            List<MusicArtist> artists = _libraryManager.GetArtists(new InternalItemsQuery(user))
                .Items
                .Select(i => i.Item1)
                .Cast<MusicArtist>()
                .ToList();

            int matchedSongs = 0;

            // Fetch the user's loved tracks from LastFM API.
            List<LastfmLovedTrack> lovedTracks = await GetLovedTracksLibrary(lastFmUser, progress, cancellationToken, maxProgress, progressOffset);

            if (lovedTracks.Count == 0)
            {
                _logger.LogInformation("User {0} has no loved tracks in last.fm", user.Username);
                return;
            }

            // remove any results from last fm loved tracks that do _not_ have an associated musicbrainz id
            lovedTracks.RemoveAll(t => String.IsNullOrEmpty(t.Artist.MusicBrainzId));
            _logger.LogInformation("User {User} has {SongCount} loved tracks in last.fm that have an associated musicbrainz Artist id", user.Username, lovedTracks.Count);
            if (lovedTracks.Count == 0)
                return;

            var lovedTracksGroupedByArtist = lovedTracks.GroupBy(t => t.Artist.MusicBrainzId).ToDictionary(t => t.Key, t => t.ToList());

            // Iterate over each artist in user's library
            // iterate over each song by artist
            // for each song, compare against the list of song/track in the lastfm loved track list
            foreach (MusicArtist artist in artists)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string artistMBid = Helpers.GetMusicBrainzArtistId(artist);
                if (String.IsNullOrEmpty(artistMBid))
                    continue;

                if (!lovedTracksGroupedByArtist.ContainsKey(artistMBid))
                    continue;

                // Loop through each song
                foreach (Audio song in artist.GetTaggedItems(new InternalItemsQuery(user)
                {
                    IncludeItemTypes = new[] { BaseItemKind.Audio },
                    EnableTotalRecordCount = false
                }).OfType<Audio>().ToList())
                {
                    LastfmLovedTrack matchedSong = null;

                    foreach (LastfmLovedTrack artistTrack in lovedTracksGroupedByArtist[artistMBid])
                    {
                        if (StringHelper.IsLike(song.Name, artistTrack.Name))
                        {
                            _logger.LogInformation("Match Found: {Artist}-{Song} <== LastFM :: Library ==> {LovedArtist}-{LovedSong}",
                            artistTrack.Artist.Name, artistTrack.Name,
                            artist.Name, song.Name);
                            matchedSong = artistTrack;
                        }
                    }

                    if (matchedSong == null)
                        continue;

                    // We have found a match
                    matchedSongs++;

                    var userData = _userDataManager.GetUserData(user, song);
                    userData.IsFavorite = true;
                    _userDataManager.SaveUserData(user, song, userData, UserDataSaveReason.UpdateUserRating, cancellationToken);
                }
            }

            _logger.LogInformation("Finished Last.fm lovedTracks sync for {User}. Matched Songs: {MatchCount}", user.Username, matchedSongs);
        }

        /// <summary>
        /// Returns a list of a target user's loved tracks from the Last.FM API. See https://www.last.fm/api/show/user.getLovedTracks
        /// </summary>
        /// <param name="lastfmUser"></param>
        /// <param name="progress"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="maxProgress"></param>
        /// <param name="progressOffset"></param>
        private async Task<List<LastfmLovedTrack>> GetLovedTracksLibrary(LastfmUser lastfmUser, IProgress<double> progress, CancellationToken cancellationToken, double maxProgress, double progressOffset)
        {
            var tracks = new List<LastfmLovedTrack>();
            int page = 1;
            bool moreTracks;

            do
            {
                cancellationToken.ThrowIfCancellationRequested();

                var response = await _apiClient.GetLovedTracks(lastfmUser, cancellationToken, page++).ConfigureAwait(false);

                if (response == null || !response.HasLovedTracks())
                    break;

                tracks.AddRange(response.LovedTracks.Tracks);

                moreTracks = !response.LovedTracks.Metadata.IsLastPage();

                // Only report progress in download because it will be 90% of the time taken
                var currentProgress = ((double)response.LovedTracks.Metadata.Page / response.LovedTracks.Metadata.TotalPages) * (maxProgress - progressOffset) + progressOffset;

                _logger.LogDebug("Progress: " + currentProgress * 100);

                progress.Report(currentProgress * 100);
            } while (moreTracks);
            _logger.LogInformation("Retrieved {0} lovedTracks from LastFM for user {1}", tracks.Count(), lastfmUser.Username);
            return tracks;
        }
    }
}

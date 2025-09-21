## jellyfin-plugin-lastfm

Enables audio scrobbling to Last.FM as well as a metadata fetcher source.

This plug-in was migrated from the original Emby repository and has been adapted to function within the Jellyfin ecosystem. This plugin *cannot* be distributed with Jellyfin due to a missing compatible license. I will attempt to keep this repo up-to-date and in-sync as the Jellyfin project matures.

## 🔧 Installation and Configuration

Install the plugin via the Jellyfin plugin repository (see: [announcement](https://jellyfin.org/posts/plugin-updates/)). Navigate to `plugins` section of the admin dashboard, add the following repository to follow stable builds for this plugin.
* Repo name: LastFM Stable
* Repo URL: https://jellyfin-repo.jesseward.com/manifest.json

![plugins](https://github.com/jesseward/jellyfin-plugin-lastfm/assets/465993/9adf1434-0ba8-4182-b267-6ce34d5933a7)

## ❓ Known Issues

###  When enabling the plugin, you receive `Authentication Failed - You do not have permissions to access this service` when authenticating using your last.fm credentials.

This error is returned from the last.fm API. If you're certain you have correctly entered your username and password. Try resetting your last.fm username and password (change the password and change it back) via the last.fm site. This may be due to stale credentials cached on the last.fm infastructure. See https://github.com/jesseward/jellyfin-plugin-lastfm/issues/51 for context.

### 3rd party Jellyfin applications (Gelli, Finamp) may not scrobble.

This appears to be due to the method in which these clients play media files. This plugin relies the invocation of the `PlaybackStartEvent` and `PlaybackStopEvent` events. Some details and references to upstream issues is located at https://github.com/jesseward/jellyfin-plugin-lastfm/issues/27#issuecomment-744031810

As an workaround, you can enable "Alternative Mode" in settings, which will scrobble on `UserDataSaved` events instead. See the [documentation of `jellyfin-plugin-listenbrainz`](https://github.com/lyarenei/jellyfin-plugin-listenbrainz/blob/main/doc/configuration.md#use-alternative-event-for-recognizing-listens) for the differences of the events.

### (very) Poor matching of artist/album/track names in the `LovedTracks` flow.

Syncing of Loved tracks between LastFM and this plugin is subpar. This is due to the `IsLike` method that is used to compare track metadata. See https://github.com/jesseward/jellyfin-plugin-lastfm/issues/24

## 🚧 Developing

### Developer: Build using Codespaces

GitHub Codespaces is the quickest solution to get started with development. Once your codespace is up and running, issue the following to build and start-up Jellyfin.

```sh
cd /workspaces/jellyfin-plugin-lastfm/.devcontainer && make
```

Port `8096` is exposed via the Codespace to allow a remote connection to your Jellyfin instance.

### Developer: Local Builds

Install the .NET SDK on Linux or macOS, see the download page at https://dotnet.microsoft.com/download . Native package manager instructions can be found for Debian, RHEL, Ubuntu, Fedora, SLES, and CentOS.

Once the SDK is installed, run the following.

```
git clone https://github.com/jesseward/jellyfin-plugin-lastfm.git
cd jellyfin-plugin-lastfm
dotnet build
```

If the build is successful, the tool will report the path to your Plugin dll (`Jellyfin.Plugin.Lastfm/bin/Debug/*/Jellyfin.Plugin.Lastfm.dll`)

The plugin should then be copied into your Jellyfin `${CONFIG_DIR}/plugins/LastFM` directory.

### Running Jellyfin from Docker

```
CACHE_DIR=/path/to/cache
MEDIA_DIR=/path/to/media
CONFIG_DIR=/path/to/config

docker run -d \
    --name jelly \
    --volume ${CONFIG_DIR}:/config \
    --volume ${MEDIA_DIR}:/media \
    --volume ${CACHE_DIR}:/cache \
    --publish 8096:8096 \
    --rm \
jellyfin/jellyfin
```

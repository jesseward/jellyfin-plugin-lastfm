# jellyfin-plugin-lastfm

Enables audio scrobbling to Last.FM as well as a metadata fetcher source.

This plug-in was migrated from the original Emby repository and has been adapted to function within the Jellyfin ecosystem. This plugin *cannot* be distributed with Jellyfin due to a missing compatible license. I will attempt to keep this repo up-to-date and in-sync as the Jellyfin project matures.

# Repo Install
Jellyfin 10.6.0 introduces 3rd party plugin repositories (see: [announcement](https://jellyfin.org/posts/plugin-updates/)), configure the following to follow stable builds for this plugin
* Repo name: LastFM Stable
* Repo URL: https://jellyfin-repo.jesseward.com/manifest.json

# Developer: Build using Codespaces

GitHub Codespaces is the quickest solution to get started with development. Once your codespace is up and running, issue the following to build and start-up Jellyfin.

```sh
cd /workspaces/jellyfin-plugin-lastfm/.devcontainer && make
```

# Developer: Local Builds


Install the .NET SDK on Linux or macOS, see the download page at https://dotnet.microsoft.com/download . Native package manager instructions can be found for Debian, RHEL, Ubuntu, Fedora, SLES, and CentOS.

Once the SDK is installed, run the following.

```
git clone https://github.com/jesseward/jellyfin-plugin-lastfm.git
cd jellyfin-plugin-lastfm
dotnet build
```

If the build is successful, the tool will report the path to your Plugin dll (`Jellyfin.Plugin.Lastfm/bin/Debug/*/Jellyfin.Plugin.Lastfm.dll`)

The plugin should then be copied into your Jellyfin `${CONFIG_DIR}/plugins/LastFM` directory.

# Running Jellyfin from Docker

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

# Known Issues

* 3rd party Jellyfin applications (Gelli, Finamp) may not scrobble. This appears to be due to the method in which these clients play media files. This plugin relies the invocation of the `PlaybackStartEvent` and `PlaybackStopEvent` events. Some details and references to upstream issues is located at https://github.com/jesseward/jellyfin-plugin-lastfm/issues/27#issuecomment-744031810
* (very) Poor matching of artist/album/track names in the `LovedTracks` flow. Syncing of Loved tracks between LastFM and this plugin is subpar. This is due to the `IsLike` method that is used to compare track metadata. See https://github.com/jesseward/jellyfin-plugin-lastfm/issues/24
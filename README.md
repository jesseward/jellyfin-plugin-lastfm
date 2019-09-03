# jellyfin-plugin-lastfm

Enables audio scrobbling to Last.FM as well as a metadata fetcher source.

This plug-in was migrated from the original Emby repository and has been adapted to function within the Jellyfin ecosystem. This plugin *cannot* be distributed with Jellyfin due to a missing compatible license. I will attempt to keep this repo up-to-date and in-sync as the Jellyfin project matures.

# Build

.NET core 2 is required to build the LastFM plugin. To install the .NET SDK on Linux or macOS, see the download page at https://dotnet.microsoft.com/download . Native package manager instructions can be found for Debian, RHEL, Ubuntu, Fedora, SLES, and CentOS.

Once the SDK is installed, run the following.

```
git clone https://github.com/jesseward/jellyfin-plugin-lastfm.git
cd jellyfin-plugin-lastfm
dotnet build
```

# INSTALL

If the build is successful, the tool will report the path to your Plugin dll (`Jellyfin.Plugin.Lastfm/bin/Debug/netstandard2.0/Jellyfin.Plugin.Lastfm.dll`)

The plugin should then be copied into your Jellyfin ${CONFIG_DIR}/plugins directory.

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

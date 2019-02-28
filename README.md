# jellyfin-plugin-lastfm

Enables audio scrobbling to Last.FM as well as a metadata fetcher source.

# Build

.NET core 2 is required to build the LastFM plugin. To install the .NET SDK on Linux or macOS, see the download page at https://dotnet.microsoft.com/download . Native package manager instructions can be found for Debian, RHEL, Ubuntu, Fedora, SLES, and CentOS.

Once the SDK is installed, run the following.

```
git clone https://github.com/jesseward/jellyfin-plugin-lastfm.git
cd jellyfin-plugin-lastfm
dotnet build
```

If the build is successful, the tool will report the path to your Plugin dll (`Jellyfin.Plugin.Lastfm/bin/Debug/netstandard2.0/Jellyfin.Plugin.Lastfm.dll`)

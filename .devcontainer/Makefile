.PHONY: build run

run:
	jellyfin -w /usr/share/jellyfin/web/ -c /mnt/jellyfin/config -d /mnt/jellyfin/data -l /mnt/jellyfin/logs -C /mnt/jellyfin/cache

build:
	dotnet build Jellyfin.Plugin.Lastfm/Jellyfin.Plugin.Lastfm.csproj -c Release
	mkdir -p /mnt/jellyfin/data/plugins/LastFM
	cp Jellyfin.Plugin.Lastfm/bin/Release/net9.0/Jellyfin.Plugin.Lastfm.dll /mnt/jellyfin/data/plugins/LastFM/

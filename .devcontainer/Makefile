all : clean restore down build up

clean:
	cd /workspaces/jellyfin-plugin-lastfm && dotnet clean
	rm -rf /opt/jellyfin/config/plugins/LastFM

restore:
	cd /workspaces/jellyfin-plugin-lastfm && dotnet restore

build:
	cd /workspaces/jellyfin-plugin-lastfm && dotnet build --output /opt/jellyfin/config/plugins/LastFM

up:
	/usr/local/share/jellyfin-init.sh

down:
	pkill /opt/jellyfin/jellyfin/jellyfin
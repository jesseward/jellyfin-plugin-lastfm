all : clean restore down build up

clean:
	cd /workspaces/jellyfin-plugin-lastfm && dotnet clean
	rm -rf /opt/jellyfin/data/plugins/LastFM

restore:
	cd /workspaces/jellyfin-plugin-lastfm && dotnet restore

build:
	mkdir /opt/jellyfin/data/plugins/LastFM
	cd /workspaces/jellyfin-plugin-lastfm && dotnet build --property:OutputPath=/opt/jellyfin/data/plugins/LastFM

up:
	/usr/local/share/jellyfin-init.sh &

down:
	sudo pkill -f /opt/jellyfin/jellyfin/jellyfin
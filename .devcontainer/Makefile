all : clean restore down build up

clean:
	cd /workspaces/jellyfin-plugin-lastfm && dotnet clean

restore:
	cd /workspaces/jellyfin-plugin-lastfm && dotnet restore

build: 
	cd /workspaces/jellyfin-plugin-lastfm && dotnet build --output /workspaces/jf/config/plugins/LastFM

up:
	cd /workspaces/jellyfin-plugin-lastfm/.devcontainer && docker-compose up

down:
	cd /workspaces/jellyfin-plugin-lastfm/.devcontainer && docker-compose down
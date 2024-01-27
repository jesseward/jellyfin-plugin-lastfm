#!/bin/bash
set -e

JELLYFIN_VOLUME="/opt/jellyfin"
PROJECT_HOME="/workspaces/jellyfin-plugin-lastfm/"

cd ${PROJECT_HOME} && dotnet restore

sudo mkdir -p ${JELLYFIN_VOLUME}/config/plugins
sudo mkdir -p ${JELLYFIN_VOLUME}/media
sudo cp -R ${PROJECT_HOME}/tests/* ${JELLYFIN_VOLUME}/media/

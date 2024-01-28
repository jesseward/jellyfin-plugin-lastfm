#!/bin/bash
set -e

JELLYFIN_VOLUME="/opt/jellyfin"
PROJECT_HOME="/workspaces/jellyfin-plugin-lastfm/"

cd ${PROJECT_HOME} && dotnet restore

sudo chown -R ${USER} ${JELLYFIN_VOLUME}
mkdir -p ${JELLYFIN_VOLUME}/config/plugins
mkdir -p ${JELLYFIN_VOLUME}/media
cp -R ${PROJECT_HOME}/tests/* ${JELLYFIN_VOLUME}/media/

#!/bin/bash
set -e

JELLYFIN_VOLUME="/workspaces/jf"
PROJECT_HOME="/workspaces/jellyfin-plugin-lastfm/"

cd ${PROJECT_HOME} && dotnet restore

mkdir -p ${JELLYFIN_VOLUME}/{config,cache,media}
mkdir -p ${JELLYFIN_VOLUME}/config/plugins

cp -R ${PROJECT_HOME}/tests/* ${JELLYFIN_VOLUME}/media/

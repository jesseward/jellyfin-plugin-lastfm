#!/bin/bash
set -e

JELLYFIN_VOLUME="/workspaces/jf"
PROJECT_HOME="/workspaces/jellyfin-plugin-lastfm/"

cd ${PROJECT_HOME} && dotnet restore

mkdir -p ${JELLYFIN_VOLUME}/{config,cache,media}
mkdir -p ${JELLYFIN_VOLUME}/config/plugins

# example mp3 file with the following metadata
# $ mutagen-inspect example.mp3 
# -- example.mp3
# - MPEG 1 layer 3, 205366 bps (VBR, LAME 3.100.0+, -V 2), 44100 Hz, 2 chn, 3.97 seconds (audio/mp3)
# TALB=Orient / One Way
# TDRC=2000
# TIT2=Orient
# TPE1=Echomen
# TRCK=1/1
cp ${PROJECT_HOME}/tests/example.mp3 ${JELLYFIN_VOLUME}/media/

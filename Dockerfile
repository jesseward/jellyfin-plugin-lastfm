FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder
WORKDIR /src

COPY *.sln .
COPY Jellyfin.Plugin.Lastfm/*.csproj ./Jellyfin.Plugin.Lastfm/
RUN dotnet restore

COPY . .

# Publish the plugin in Release configuration
RUN dotnet publish "Jellyfin.Plugin.Lastfm/Jellyfin.Plugin.Lastfm.csproj" -c Release -o /app/publish

FROM jellyfin/jellyfin:latest
ARG PLUGIN_NAME=LastFM
RUN mkdir -p /config/plugins/${PLUGIN_NAME}/
COPY --from=builder /app/publish/Jellyfin.Plugin.Lastfm.dll /config/plugins/${PLUGIN_NAME}/

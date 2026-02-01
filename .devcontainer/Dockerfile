
FROM mcr.microsoft.com/dotnet/sdk:9.0

# Install Jellyfin
USER root
RUN apt-get update && apt-get install -y curl gpg
RUN curl -fsSL https://repo.jellyfin.org/jellyfin_team.gpg.key | gpg --dearmor -o /etc/apt/trusted.gpg.d/jellyfin.gpg
RUN echo "deb [arch=$( dpkg --print-architecture )] https://repo.jellyfin.org/debian $( awk -F'=' '/^VERSION_CODENAME=/{ print $2 }' /etc/os-release ) main" | tee /etc/apt/sources.list.d/jellyfin.list
RUN apt-get update && apt-get install -y jellyfin jellyfin-web ffmpeg make

# Create a non-root user
ARG USERNAME=vscode
ARG USER_UID=1000
ARG USER_GID=$USER_UID
RUN groupadd --gid $USER_GID $USERNAME \
    && useradd --uid $USER_UID --gid $USER_GID -m $USERNAME \
    && apt-get update \
    && apt-get install -y sudo \
    && echo $USERNAME ALL=\(root\) NOPASSWD:ALL > /etc/sudoers.d/$USERNAME \
    && chmod 0440 /etc/sudoers.d/$USERNAME

# Switch to the non-root user
USER $USERNAME

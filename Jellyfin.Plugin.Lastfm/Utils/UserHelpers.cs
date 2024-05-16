using System;
using System.Linq;
using Jellyfin.Data.Entities;
using Jellyfin.Plugin.Lastfm.Models;

namespace Jellyfin.Plugin.Lastfm.Utils;

/// <summary>
/// Common functions to aide in the retrieval of Last.fm users.
/// </summary>
public static class UserHelpers
{
    /// <summary>
    /// Get a user by a User ojbect.
    /// </summary>
    /// <param name="user">User object.</param>
    /// <returns>LastfmUser object.</returns>
    public static LastfmUser? GetUser(User user)
    {
        if (user == null)
        {
            return null;
        }

        if (Plugin.Instance.PluginConfiguration.LastfmUsers == null)
        {
            return null;
        }

        return GetUser(user.Id);
    }

    /// <summary>
    /// Get a user by their GUID.
    /// </summary>
    /// <param name="userId">User GUID.</param>
    /// <returns>LastfmUser object.</returns>
    public static LastfmUser? GetUser(Guid userId)
    {
        return Plugin.Instance.PluginConfiguration.LastfmUsers.FirstOrDefault(u => u.MediaBrowserUserId.Equals(userId));
    }

    /// <summary>
    /// Get a user by their GUID, string format.
    /// </summary>
    /// <param name="userGuid">User GUID.</param>
    /// <returns>LastfmUser object.</returns>
    public static LastfmUser? GetUser(string userGuid)
    {
        if (Guid.TryParse(userGuid, out var g))
        {
            return GetUser(g);
        }

        return null;
    }
}

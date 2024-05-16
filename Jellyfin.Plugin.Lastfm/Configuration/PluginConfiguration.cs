using System;
using System.Collections.Generic;
using Jellyfin.Plugin.Lastfm.Models;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.Lastfm.Configuration;

/// <summary>
/// Plugin configuration class for LastFM plugin.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PluginConfiguration" /> class.
    /// </summary>
    public PluginConfiguration()
    {
        LastfmUsers = Array.Empty<LastfmUser>();
    }

    /// <summary>
    /// Gets or sets all configured LastfmUsers.
    /// </summary>
    public IEnumerable<LastfmUser> LastfmUsers { get; set; }
}

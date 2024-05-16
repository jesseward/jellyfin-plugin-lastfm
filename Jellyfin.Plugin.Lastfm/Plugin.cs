using System;
using System.Collections.Generic;
using Jellyfin.Plugin.Lastfm.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.Lastfm;

/// <summary>
/// The main plugin.
/// </summary>
public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Plugin"/> class.
    /// </summary>
    /// <param name="applicationPaths">Instance of the <see cref="IApplicationPaths"/> interface.</param>
    /// <param name="xmlSerializer">Instance of the <see cref="IXmlSerializer"/> interface.</param>
    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
    }

    /// <summary>
    /// Gets a value indicating whether the plugin is currently running/syncing.
    /// </summary>
    public static bool Syncing { get; internal set; }

    /// <summary>
    /// Gets the plugin configuration.
    /// </summary>
    public PluginConfiguration PluginConfiguration => Configuration;

    /// <inheritdoc />
    public override Guid Id { get; } = new Guid("de7fe7f0-b048-439e-a431-b1a7e99c930d");

    /// <inheritdoc />
    public override string Name
        => "Last.fm";

    /// <inheritdoc />
    public override string Description
        => "Scrobble your music collection to Last.fm";

    /// <summary>
    /// Gets the current plugin instance.
    /// </summary>
    public static Plugin Instance { get; private set; } = null!;

    /// <inheritdoc />
    public IEnumerable<PluginPageInfo> GetPages()
    {
        return new[]
        {
                new PluginPageInfo
                {
                    Name = "lastfm",
                    EmbeddedResourcePath = GetType().Namespace + ".Configuration.configPage.html"
                }
        };
    }
}

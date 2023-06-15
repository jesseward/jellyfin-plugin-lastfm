namespace Jellyfin.Plugin.Lastfm.Configuration
{
    using MediaBrowser.Model.Plugins;

    using Models;

    /// <summary>
    /// Class PluginConfiguration
    /// </summary>
    public class PluginConfiguration : BasePluginConfiguration
    {
        public LastfmUser[] LastfmUsers { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginConfiguration" /> class.
        /// </summary>
        public PluginConfiguration()
        {
            LastfmUsers = new LastfmUser[] { };
        }
    }
}

namespace Lastfm
{
    using Configuration;
    using MediaBrowser.Common.Configuration;
    using MediaBrowser.Common.Plugins;
    using MediaBrowser.Model.Logging;
    using MediaBrowser.Model.Plugins;
    using MediaBrowser.Model.Serialization;
    using System;
    using System.Threading;
    using System.Linq;
    using System.Collections.Generic;

    /// <summary>
    /// Class Plugin
    /// </summary>
    public class Plugin : BasePlugin<PluginConfiguration>
    {
        /// <summary>
        /// Flag set when an Import Syncing task is running
        /// </summary>
        public static bool Syncing { get; internal set; }
        
        internal static readonly SemaphoreSlim LastfmResourcePool = new SemaphoreSlim(4, 4);

        //Global logging instance
        public static ILogger Logger { get; set; }

        public PluginConfiguration PluginConfiguration
        {
            get { return Configuration; }
        }

        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
        }

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

        private Guid _id = new Guid("E603A45D-7088-4EE0-892D-B7CA8BC0B513");
        public override Guid Id
        {
            get { return _id; }
        }

        /// <summary>
        /// Gets the name of the plugin
        /// </summary>
        /// <value>The name.</value>
        public override string Name
        {
            get { return "Last.fm Scrobbler"; }
        }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>The description.</value>
        public override string Description
        {
            get
            {
                return "Scrobble your music collection to Last.fm";
            }
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static Plugin Instance { get; private set; }

        /// <summary>
        /// Updates the configuration.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public override void UpdateConfiguration(BasePluginConfiguration configuration)
        {
            var oldConfig = Configuration;

            base.UpdateConfiguration(configuration);

            ServerEntryPoint.Instance.OnConfigurationUpdated(oldConfig, (PluginConfiguration)configuration);
        }
    }
}

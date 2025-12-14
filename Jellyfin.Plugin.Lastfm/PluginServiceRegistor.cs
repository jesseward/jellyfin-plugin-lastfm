using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.Lastfm
{
    /// <summary>
    /// Register LastFM Scrobbler services
    /// </summary>
    public class PluginServiceRegistrator : IPluginServiceRegistrator
    {
        /// <inheritdoc />
        public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
        {
            serviceCollection.AddSingleton<Api.LastfmApiClient>();
            serviceCollection.AddHostedService<ServerEntryPoint>();
        }
    }
}

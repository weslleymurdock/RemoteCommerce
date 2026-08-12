using Microsoft.Extensions.Logging;

namespace RemoteCommerce.Plugins;

public static class PluginServiceCollectionExtensions
{
    public static void AddInstalledRemoteCommercePlugins(this IServiceCollection services, string pluginsRoot, IConfiguration configuration)
    {
        using var loggerFactory = LoggerFactory.Create(logging => logging.AddConfiguration(configuration.GetSection("Logging")));
        var loader = new PluginLoader(loggerFactory.CreateLogger<PluginLoader>());
        loader.Load(services, pluginsRoot);
    }
}

using Microsoft.Extensions.Logging;

namespace RemoteCommerce.Plugins;

/// <summary>
/// Provides dependency injection registration helpers for the RemoteCommerce plugin system.
/// </summary>
public static class PluginServiceCollectionExtensions
{
    /// <summary>
    /// Registers the plugin installation service and loads installed plugin extensions into the service collection.
    /// </summary>
    /// <param name="services">The application service collection.</param>
    /// <param name="pluginsRoot">The root directory containing installed plugin packages.</param>
    /// <param name="configuration">The application configuration used to configure plugin logging.</param>
    public static void AddInstalledRemoteCommercePlugins(this IServiceCollection services, string pluginsRoot, IConfiguration configuration)
    {
        services.AddScoped<PluginInstallationService>();

        using var loggerFactory = LoggerFactory.Create(logging => logging.AddConfiguration(configuration.GetSection("Logging")));
        var loader = new PluginLoader(loggerFactory.CreateLogger<PluginLoader>());
        loader.Load(services, pluginsRoot);
    }
}

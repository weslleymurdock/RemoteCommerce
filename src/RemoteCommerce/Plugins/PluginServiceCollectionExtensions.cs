using Microsoft.Extensions.Logging;
using RemoteCommerce.Infrastructure.Persistence;

namespace RemoteCommerce.Plugins;

/// <summary>
/// Provides dependency injection registration helpers for the RemoteCommerce plugin system.
/// </summary>
public static class PluginServiceCollectionExtensions
{
    /// <summary>
    /// Registers the plugin installation service and creates the startup plugin loader from the configured host dependencies.
    /// </summary>
    /// <param name="services">The application service collection.</param>
    /// <param name="pluginsRoot">The root directory containing installed plugin packages.</param>
    /// <param name="configuration">The application configuration used to configure plugin logging.</param>
    /// <remarks>
    /// The loader executes before <see cref="WebApplicationBuilder.Build"/> so plugin registrations become part of the final service provider.
    /// </remarks>
    public static void AddInstalledRemoteCommercePlugins(this IServiceCollection services, string pluginsRoot, IConfiguration configuration)
    {
        services.AddScoped<PluginInstallationService>();

        using var loggerFactory = LoggerFactory.Create(logging => logging.AddConfiguration(configuration.GetSection("Logging")));
        var logger = loggerFactory.CreateLogger<PluginLoader>();
        var dbFactory = services.BuildServiceProvider().GetRequiredService<IDbContextFactory<CommerceDbContext>>();
        var loader = new PluginLoader(logger, dbFactory);
        loader.Load(services, pluginsRoot);
    }
}

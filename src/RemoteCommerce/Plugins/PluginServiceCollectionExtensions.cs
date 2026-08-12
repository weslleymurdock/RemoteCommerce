using Microsoft.Extensions.DependencyInjection.Extensions;
using RemoteCommerce.Infrastructure.Persistence;

namespace RemoteCommerce.Plugins;

/// <summary>
/// Provides dependency injection registration helpers for the RemoteCommerce plugin system.
/// </summary>
public static class PluginServiceCollectionExtensions
{
    /// <summary>
    /// Registers plugin runtime services and loads installed plugins before the application host is built.
    /// </summary>
    /// <param name="services">The application service collection.</param>
    /// <param name="pluginsRoot">The root directory containing installed plugin packages.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <remarks>
    /// The plugin loader is constructed from the existing registration graph without replacing the application's final service provider.
    /// </remarks>
    public static void AddInstalledRemoteCommercePlugins(this IServiceCollection services, string pluginsRoot, IConfiguration configuration)
    {
        services.AddScoped<PluginInstallationService>();
        services.AddScoped<PluginPackageInstaller>();

        services.TryAddSingleton<PluginStartupContext>(_ =>
        {
            var loggerFactory = LoggerFactory.Create(logging => logging.AddConfiguration(configuration.GetSection("Logging")));
            return new PluginStartupContext(loggerFactory, pluginsRoot);
        });
    }

    /// <summary>
    /// Loads persisted plugins into the application service collection during startup.
    /// </summary>
    /// <param name="services">The application service collection.</param>
    /// <param name="startupContext">The startup context containing plugin discovery settings.</param>
    /// <param name="dbFactory">The EF Core context factory used to read installation state.</param>
    public static void LoadInstalledRemoteCommercePlugins(this IServiceCollection services, PluginStartupContext startupContext, IDbContextFactory<CommerceDbContext> dbFactory)
    {
        var loader = new PluginLoader(startupContext.LoggerFactory.CreateLogger<PluginLoader>(), dbFactory);
        loader.Load(services, startupContext.PluginsRoot);
    }
}

/// <summary>
/// Contains immutable configuration captured for plugin startup discovery.
/// </summary>
/// <param name="LoggerFactory">The logger factory used by the plugin loader.</param>
/// <param name="PluginsRoot">The root directory containing installed plugin packages.</param>
public sealed record PluginStartupContext(ILoggerFactory LoggerFactory, string PluginsRoot);

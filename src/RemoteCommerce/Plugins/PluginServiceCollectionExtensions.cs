using Microsoft.EntityFrameworkCore;
using RemoteCommerce.Infrastructure.Persistence;

namespace RemoteCommerce.Plugins;

/// <summary>
/// Provides dependency injection registration helpers for the RemoteCommerce plugin system.
/// </summary>
public static class PluginServiceCollectionExtensions
{
    /// <summary>
    /// Registers plugin runtime services and loads persisted plugins before the application host is built.
    /// </summary>
    /// <param name="services">The application service collection.</param>
    /// <param name="pluginsRoot">The root directory containing installed plugin packages.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <remarks>
    /// A short-lived bootstrap provider is intentionally used only to resolve the already-registered EF Core context factory and logger.
    /// The provider is never used as the application's runtime service provider. This is required because plugin registrations must be added
    /// to <paramref name="services"/> before <see cref="WebApplicationBuilder.Build"/> creates the final provider.
    /// </remarks>
    public static void AddInstalledRemoteCommercePlugins(this IServiceCollection services, string pluginsRoot, IConfiguration configuration)
    {
        services.AddScoped<PluginInstallationService>();
        services.AddScoped<PluginPackageInstaller>();

        using var bootstrapProvider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = false
        });

        using var loggerFactory = LoggerFactory.Create(logging => logging.AddConfiguration(configuration.GetSection("Logging")));
        var logger = loggerFactory.CreateLogger<PluginLoader>();
        var dbFactory = bootstrapProvider.GetRequiredService<IDbContextFactory<CommerceDbContext>>();

        using (var db = dbFactory.CreateDbContext())
        {
            db.Database.EnsureCreated();
        }

        var loader = new PluginLoader(logger, dbFactory);
        loader.Load(services, pluginsRoot);
    }
}

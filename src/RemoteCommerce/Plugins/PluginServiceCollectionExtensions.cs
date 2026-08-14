namespace RemoteCommerce.Plugins;

/// <summary>Provides dependency injection registration helpers for the RemoteCommerce plugin system.</summary>
public static class PluginServiceCollectionExtensions
{
    /// <summary>Registers plugin administration services and loads persisted plugins before the application host is built.</summary>
    /// <param name="services">The application service collection.</param>
    /// <param name="pluginsRoot">The root directory containing installed plugin packages.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <remarks>A short-lived bootstrap provider is used only to apply host persistence migrations and load plugins. It is never used as the runtime application provider.</remarks>
    public static void AddInstalledRemoteCommercePlugins(this IServiceCollection services, string pluginsRoot, IConfiguration configuration)
    {
        services.AddSingleton<IApplicationRestartService, ApplicationRestartService>();
        services.AddSingleton<IPluginPackageSource, TrustedPluginPackageSource>();
        services.AddSingleton<IPluginManifestValidator, PluginManifestValidator>();
        services.AddSingleton<IPluginCompatibilityValidator, PluginCompatibilityValidator>();
        services.AddSingleton<IPluginPackageValidator, PluginPackageValidator>();
        services.AddScoped<PluginPackageInstaller>();
        services.AddScoped<PluginDependencyValidator>();
        services.AddScoped<PluginInstallationService>();
        services.AddScoped<PluginManagementService>();
        services.AddScoped<PluginSettingsService>();

        using var bootstrapProvider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = false
        });

        var dbFactory = bootstrapProvider.GetRequiredService<IDbContextFactory<CommerceDbContext>>();
        using (var db = dbFactory.CreateDbContext())
        {
            db.Database.Migrate();
        }

        using var loggerFactory = LoggerFactory.Create(logging => logging.AddConfiguration(configuration.GetSection("Logging")));
        var loader = new PluginLoader(
            loggerFactory.CreateLogger<PluginLoader>(),
            dbFactory,
            bootstrapProvider.GetRequiredService<IPluginManifestValidator>(),
            bootstrapProvider.GetRequiredService<IPluginCompatibilityValidator>(),
            bootstrapProvider.GetRequiredService<RemoteCommerce.Application.Persistence.Abstractions.IDatabaseProvider>());
        loader.Load(services, pluginsRoot);
    }
}

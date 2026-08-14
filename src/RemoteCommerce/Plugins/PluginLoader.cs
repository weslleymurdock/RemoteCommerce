namespace RemoteCommerce.Plugins;

/// <summary>Discovers, validates, initializes, and registers installed plugins before the application host is built.</summary>
/// <param name="logger">The logger used to report plugin discovery and activation failures.</param>
/// <param name="dbFactory">The factory used to read and persist plugin activation state.</param>
/// <param name="manifestValidator">The validator used to revalidate persisted manifests at startup.</param>
/// <param name="compatibilityValidator">The validator used to revalidate host compatibility at startup.</param>
/// <param name="databaseProvider">The selected relational provider used to configure plugin DbContexts.</param>
public sealed class PluginLoader(
    ILogger<PluginLoader> logger,
    IDbContextFactory<CommerceDbContext> dbFactory,
    IPluginManifestValidator manifestValidator,
    IPluginCompatibilityValidator compatibilityValidator,
    IDatabaseProvider databaseProvider)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>Loads all administratively enabled plugin packages into the supplied service collection.</summary>
    /// <param name="services">The application service collection that plugins may extend.</param>
    /// <param name="pluginsRoot">The root directory containing installed plugin packages.</param>
    /// <returns>The manifests of successfully loaded plugins.</returns>
    public IReadOnlyList<PluginManifest> Load(IServiceCollection services, string pluginsRoot)
    {
        CleanupPendingDeletes(pluginsRoot);
        if (!Directory.Exists(pluginsRoot))
        {
            return [];
        }

        var configuration = services.LastOrDefault(x => x.ServiceType == typeof(IConfiguration))?.ImplementationInstance as IConfiguration
            ?? throw new InvalidOperationException("IConfiguration must be registered in the host service collection before loading RemoteCommerce plugins.");

        using var db = dbFactory.CreateDbContext();
        var installations = db.PluginInstallations.ToDictionary(x => x.PluginId, StringComparer.OrdinalIgnoreCase);
        var loaded = new List<PluginManifest>();
        var loading = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var loadedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var installation in installations.Values.OrderBy(x => x.PluginId, StringComparer.OrdinalIgnoreCase))
        {
            if (installation.DesiredState == PluginDesiredState.Disabled)
            {
                installation.State = PluginInstallationState.Disabled;
                installation.LastError = null;
                installation.UpdatedAt = DateTimeOffset.UtcNow;
                continue;
            }

            try
            {
                LoadPlugin(installation.PluginId, installations, services, configuration, loading, loadedIds, loaded);
            }
            catch (Exception exception)
            {
                MarkFailed(db, installation, "startup", "activation", exception);
                logger.LogError(exception, "Failed to load plugin {PluginId}.", installation.PluginId);
            }
        }

        db.SaveChanges();
        return loaded;
    }

    private void LoadPlugin(
        string pluginId,
        Dictionary<string, PluginInstallation> installations,
        IServiceCollection services,
        IConfiguration configuration,
        HashSet<string> loading,
        HashSet<string> loadedIds,
        List<PluginManifest> loaded)
    {
        if (loadedIds.Contains(pluginId))
        {
            return;
        }

        if (!loading.Add(pluginId))
        {
            throw new InvalidOperationException($"Circular plugin dependency detected at '{pluginId}'.");
        }

        try
        {
            if (!installations.TryGetValue(pluginId, out var installation))
            {
                throw new InvalidOperationException($"Plugin '{pluginId}' is not installed.");
            }

            if (installation.DesiredState == PluginDesiredState.Disabled)
            {
                throw new InvalidOperationException($"Plugin '{pluginId}' is disabled but is required by another enabled plugin.");
            }

            var manifestPath = Path.Combine(installation.PackagePath, "plugin.manifest.json");
            if (!File.Exists(manifestPath))
            {
                throw new FileNotFoundException("Plugin manifest was not found.", manifestPath);
            }

            var manifest = JsonSerializer.Deserialize<PluginManifest>(File.ReadAllText(manifestPath), JsonOptions)
                ?? throw new InvalidOperationException("Plugin manifest is empty.");
            var manifestIssues = manifestValidator.Validate(manifest)
                .Concat(compatibilityValidator.Validate(manifest))
                .Where(x => x.Severity == PluginValidationSeverity.Error)
                .ToArray();
            if (manifestIssues.Length > 0)
            {
                throw new InvalidOperationException(string.Join(Environment.NewLine, manifestIssues.Select(x => $"[{x.Code}] {x.Message}")));
            }

            foreach (var dependency in manifest.DependencyDeclarations)
            {
                LoadPlugin(dependency.PluginId, installations, services, configuration, loading, loadedIds, loaded);
            }

            var assemblyPath = Path.GetFullPath(Path.Combine(installation.PackagePath, manifest.EntryAssembly));
            if (!assemblyPath.StartsWith(Path.GetFullPath(installation.PackagePath) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Plugin entry assembly '{manifest.EntryAssembly}' escapes the installation directory.");
            }

            if (!File.Exists(assemblyPath))
            {
                throw new FileNotFoundException("Plugin entry assembly was not found.", assemblyPath);
            }

            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
            var pluginType = assembly.GetType(manifest.EntryType, true, false)
                ?? throw new InvalidOperationException($"Plugin type '{manifest.EntryType}' was not found.");
            if (!typeof(IRemoteCommercePlugin).IsAssignableFrom(pluginType))
            {
                throw new InvalidOperationException($"Plugin type '{manifest.EntryType}' must implement IRemoteCommercePlugin.");
            }

            var plugin = (IRemoteCommercePlugin?)Activator.CreateInstance(pluginType)
                ?? throw new InvalidOperationException($"Plugin type '{manifest.EntryType}' could not be instantiated.");
            plugin.ConfigureServices(services, manifest, configuration);

            if (plugin is IRemoteCommercePluginPersistence persistence)
            {
                var persistenceBuilder = new PluginPersistenceBuilder(services, manifest.Id);
                persistence.ConfigurePersistence(persistenceBuilder);
                ApplyMigrations(persistenceBuilder, manifest);
            }

            PluginAssemblyRegistry.Add(assembly);
            installation.State = PluginInstallationState.Loaded;
            installation.PendingVersion = null;
            installation.LastError = null;
            installation.UpdatedAt = DateTimeOffset.UtcNow;
            loadedIds.Add(pluginId);
            loaded.Add(manifest);
            logger.LogInformation("Loaded plugin {PluginId} version {PluginVersion}.", manifest.Id, manifest.Version);
        }
        finally
        {
            loading.Remove(pluginId);
        }
    }

    private void ApplyMigrations(PluginPersistenceBuilder builder, PluginManifest manifest)
    {
        foreach (var descriptor in builder.GetDescriptors())
        {
            logger.LogInformation(
                "Initializing persistence for plugin {PluginId} using {DbContextType}.",
                manifest.Id,
                descriptor.DbContextType.FullName);
            InvokeMigration(descriptor.DbContextType, descriptor.MigrationsAssembly).GetAwaiter().GetResult();
        }
    }

    private Task InvokeMigration(Type dbContextType, string? migrationsAssembly)
    {
        var method = typeof(PluginLoader)
            .GetMethod(nameof(MigrateDbContext), BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Plugin migration method could not be located.");
        var genericMethod = method.MakeGenericMethod(dbContextType);
        return (Task)(genericMethod.Invoke(this, [migrationsAssembly])
            ?? throw new InvalidOperationException("Plugin migration could not be started."));
    }

    private async Task MigrateDbContext<TDbContext>(string? migrationsAssembly)
        where TDbContext : DbContext
    {
        var optionsBuilder = new DbContextOptionsBuilder<TDbContext>();
        databaseProvider.ConfigureDbContext(optionsBuilder, migrationsAssembly);
        await using var context = Activator.CreateInstance(typeof(TDbContext), optionsBuilder.Options) as TDbContext
            ?? throw new InvalidOperationException($"Plugin DbContext '{typeof(TDbContext).FullName}' could not be created.");
        var pending = await context.Database.GetPendingMigrationsAsync();
        if (pending.Any())
        {
            logger.LogInformation(
                "Applying {MigrationCount} pending migration(s) for plugin DbContext {DbContextType}.",
                pending.Count(),
                typeof(TDbContext).FullName);
        }

        await context.Database.MigrateAsync();
    }

    private static void MarkFailed(CommerceDbContext db, PluginInstallation installation, string operation, string category, Exception exception)
    {
        installation.State = PluginInstallationState.Failed;
        installation.LastError = exception.Message;
        installation.UpdatedAt = DateTimeOffset.UtcNow;
        db.PluginLifecycleErrors.Add(new PluginLifecycleError
        {
            Id = Guid.NewGuid(),
            PluginId = installation.PluginId,
            Operation = operation,
            Category = category,
            Message = exception.Message,
            ExceptionType = exception.GetType().FullName,
            CreatedAt = DateTimeOffset.UtcNow
        });
    }

    private static void CleanupPendingDeletes(string pluginsRoot)
    {
        var pendingRoot = Path.Combine(pluginsRoot, ".pending-delete");
        if (Directory.Exists(pendingRoot))
        {
            foreach (var directory in Directory.EnumerateDirectories(pendingRoot))
            {
                try
                {
                    Directory.Delete(directory, true);
                }
                catch
                {
                }
            }
        }

        if (!Directory.Exists(pluginsRoot))
        {
            return;
        }

        foreach (var pluginRoot in Directory.EnumerateDirectories(pluginsRoot))
        {
            var pluginPendingRoot = Path.Combine(pluginRoot, ".pending-delete");
            if (!Directory.Exists(pluginPendingRoot))
            {
                continue;
            }

            foreach (var directory in Directory.EnumerateDirectories(pluginPendingRoot))
            {
                try
                {
                    Directory.Delete(directory, true);
                }
                catch
                {
                }
            }
        }
    }
}

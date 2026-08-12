using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RemoteCommerce.Infrastructure.Persistence;
using RemoteCommerce.Plugins.Abstractions;

namespace RemoteCommerce.Plugins;

/// <summary>Discovers and registers installed RemoteCommerce plugins before the application host is built.</summary>
/// <param name="logger">The logger used to report plugin discovery failures.</param>
/// <param name="dbFactory">The factory used to read persisted plugin activation state.</param>
/// <param name="configuration">The host application configuration exposed to plugins during startup.</param>
public sealed class PluginLoader(
    ILogger<PluginLoader> logger,
    IDbContextFactory<CommerceDbContext> dbFactory,
    IConfiguration configuration)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>Loads all persisted active plugin packages into the supplied service collection.</summary>
    /// <param name="services">The application service collection that plugins may extend.</param>
    /// <param name="pluginsRoot">The root directory containing installed plugin packages.</param>
    /// <returns>The manifests of successfully loaded plugins.</returns>
    public IReadOnlyList<PluginManifest> Load(IServiceCollection services, string pluginsRoot)
    {
        CleanupPendingDeletes(pluginsRoot);
        if (!Directory.Exists(pluginsRoot)) return [];
        using var db = dbFactory.CreateDbContext();
        var installed = db.PluginInstallations.AsNoTracking().Where(x => x.State == PluginInstallationState.Installed).ToDictionary(x => x.PluginId, StringComparer.OrdinalIgnoreCase);
        var loaded = new List<PluginManifest>();
        foreach (var manifestPath in Directory.EnumerateFiles(pluginsRoot, "plugin.manifest.json", SearchOption.AllDirectories).OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                var manifest = JsonSerializer.Deserialize<PluginManifest>(File.ReadAllText(manifestPath), JsonOptions) ?? throw new InvalidOperationException("Manifest is empty.");
                ValidateManifest(manifest);
                if (!installed.ContainsKey(manifest.Id)) continue;
                var pluginDirectory = Path.GetDirectoryName(manifestPath)!;
                var assemblyPath = Path.GetFullPath(Path.Combine(pluginDirectory, manifest.EntryAssembly));
                if (!File.Exists(assemblyPath)) throw new FileNotFoundException("Plugin entry assembly was not found.", assemblyPath);
                var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
                var pluginType = assembly.GetType(manifest.EntryType, true, false) ?? throw new InvalidOperationException($"Plugin type '{manifest.EntryType}' was not found.");
                if (!typeof(IRemoteCommercePlugin).IsAssignableFrom(pluginType)) throw new InvalidOperationException($"Plugin type '{manifest.EntryType}' must implement IRemoteCommercePlugin.");
                var plugin = (IRemoteCommercePlugin)Activator.CreateInstance(pluginType)!;
                plugin.ConfigureServices(services, manifest, configuration);
                PluginAssemblyRegistry.Add(assembly);
                loaded.Add(manifest);
                logger.LogInformation("Loaded plugin {PluginId} version {PluginVersion}.", manifest.Id, manifest.Version);
            }
            catch (Exception exception) { logger.LogError(exception, "Failed to load plugin manifest {ManifestPath}.", manifestPath); }
        }
        return loaded;
    }

    private static void ValidateManifest(PluginManifest manifest)
    {
        if (new[] { manifest.Id, manifest.Name, manifest.License, manifest.Readme, manifest.Version, manifest.EntryAssembly, manifest.EntryType, manifest.MinHostVersion, manifest.Description, manifest.PackageId, manifest.PackageTags, manifest.Title, manifest.Authors, manifest.Company, manifest.RepositoryUrl, manifest.RepositoryType, manifest.PackageProjectUrl }.Any(string.IsNullOrWhiteSpace)) throw new InvalidOperationException("The plugin manifest contains required empty metadata.");
        if (!Version.TryParse(manifest.MinHostVersion, out var minimumHostVersion)) throw new InvalidOperationException("Plugin MinHostVersion must be a valid version value.");
        var hostVersion = Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(0, 0);
        if (hostVersion < minimumHostVersion) throw new InvalidOperationException($"Plugin requires host version {minimumHostVersion}, but the current host is {hostVersion}.");
    }

    private static void CleanupPendingDeletes(string pluginsRoot)
    {
        var pendingRoot = Path.Combine(pluginsRoot, ".pending-delete");
        if (!Directory.Exists(pendingRoot)) return;
        foreach (var directory in Directory.EnumerateDirectories(pendingRoot)) try { Directory.Delete(directory, true); } catch { }
    }
}

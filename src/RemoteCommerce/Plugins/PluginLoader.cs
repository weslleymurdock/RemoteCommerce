using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RemoteCommerce.Infrastructure.Persistence;
using RemoteCommerce.Plugins.Abstractions;

namespace RemoteCommerce.Plugins;

/// <summary>
/// Discovers and registers installed RemoteCommerce plugins before the application host is built.
/// </summary>
/// <param name="logger">The logger used to report plugin discovery failures.</param>
/// <param name="dbFactory">The factory used to read persisted plugin activation state.</param>
public sealed class PluginLoader(
    ILogger<PluginLoader> logger,
    IDbContextFactory<CommerceDbContext> dbFactory)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Loads all persisted active plugin packages into the supplied service collection.
    /// </summary>
    /// <param name="services">The application service collection that plugins may extend.</param>
    /// <param name="pluginsRoot">The root directory containing installed plugin packages.</param>
    /// <returns>The manifests of successfully loaded plugins.</returns>
    public IReadOnlyList<PluginManifest> Load(IServiceCollection services, string pluginsRoot)
    {
        if (!Directory.Exists(pluginsRoot))
        {
            return [];
        }

        using var db = dbFactory.CreateDbContext();
        var installed = db.PluginInstallations
            .AsNoTracking()
            .Where(x => x.State == PluginInstallationState.Installed)
            .ToDictionary(x => x.PluginId, StringComparer.OrdinalIgnoreCase);

        var loaded = new List<PluginManifest>();

        foreach (var manifestPath in Directory.EnumerateFiles(pluginsRoot, "plugin.manifest.json", SearchOption.AllDirectories).OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                var manifest = JsonSerializer.Deserialize<PluginManifest>(File.ReadAllText(manifestPath), JsonOptions)
                    ?? throw new InvalidOperationException("Manifest is empty.");

                ValidateManifest(manifest);

                if (!installed.ContainsKey(manifest.Id))
                {
                    continue;
                }

                var pluginDirectory = Path.GetDirectoryName(manifestPath)!;
                var assemblyPath = Path.GetFullPath(Path.Combine(pluginDirectory, manifest.EntryAssembly));
                if (!File.Exists(assemblyPath))
                {
                    throw new FileNotFoundException("Plugin entry assembly was not found.", assemblyPath);
                }

                var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
                var pluginType = assembly.GetType(manifest.EntryType, throwOnError: true, ignoreCase: false)
                    ?? throw new InvalidOperationException($"Plugin type '{manifest.EntryType}' was not found.");

                if (!typeof(IRemoteCommercePlugin).IsAssignableFrom(pluginType))
                {
                    throw new InvalidOperationException($"Plugin type '{manifest.EntryType}' must implement IRemoteCommercePlugin.");
                }

                var plugin = (IRemoteCommercePlugin)Activator.CreateInstance(pluginType)!;
                plugin.ConfigureServices(services, manifest);
                loaded.Add(manifest);

                logger.LogInformation("Loaded plugin {PluginId} version {PluginVersion}.", manifest.Id, manifest.Version);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to load plugin manifest {ManifestPath}.", manifestPath);
            }
        }

        return loaded;
    }

    private static void ValidateManifest(PluginManifest manifest)
    {
        if (string.IsNullOrWhiteSpace(manifest.Id) || string.IsNullOrWhiteSpace(manifest.EntryAssembly) || string.IsNullOrWhiteSpace(manifest.EntryType))
        {
            throw new InvalidOperationException("Plugin manifest requires Id, EntryAssembly and EntryType.");
        }
    }
}

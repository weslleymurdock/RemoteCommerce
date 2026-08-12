using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using RemoteCommerce.Plugins.Abstractions;

namespace RemoteCommerce.Plugins;

public sealed class PluginLoader(ILogger<PluginLoader> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public IReadOnlyList<PluginManifest> Load(IServiceCollection services, string pluginsRoot)
    {
        if (!Directory.Exists(pluginsRoot))
        {
            return [];
        }

        var loaded = new List<PluginManifest>();

        foreach (var manifestPath in Directory.EnumerateFiles(pluginsRoot, "plugin.manifest.json", SearchOption.AllDirectories))
        {
            try
            {
                var manifest = JsonSerializer.Deserialize<PluginManifest>(File.ReadAllText(manifestPath), JsonOptions)
                    ?? throw new InvalidOperationException("Manifest is empty.");

                ValidateManifest(manifest);

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

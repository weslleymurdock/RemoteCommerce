using System.Text.Json;
using RemoteCommerce.Plugins.Abstractions;

namespace RemoteCommerce.Plugins;

/// <summary>
/// Validates and copies a plugin package into the application plugin directory.
/// </summary>
/// <param name="environment">The host environment used to resolve the installation root.</param>
public sealed class PluginPackageInstaller(IWebHostEnvironment environment)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Validates and installs a plugin package into its stable plugin directory.
    /// </summary>
    /// <param name="sourceDirectory">The directory containing the plugin package.</param>
    /// <param name="cancellationToken">The token used to cancel the copy operation.</param>
    /// <returns>The validated manifest and target package directory.</returns>
    /// <exception cref="DirectoryNotFoundException">Thrown when <paramref name="sourceDirectory"/> does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the package does not contain a valid manifest or entry assembly.</exception>
    public async Task<(PluginManifest Manifest, string TargetDirectory)> InstallAsync(string sourceDirectory, CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(sourceDirectory))
        {
            throw new DirectoryNotFoundException(sourceDirectory);
        }

        var manifestPath = Path.Combine(sourceDirectory, "plugin.manifest.json");
        if (!File.Exists(manifestPath))
        {
            throw new InvalidOperationException("The plugin package must contain plugin.manifest.json.");
        }

        var manifest = JsonSerializer.Deserialize<PluginManifest>(await File.ReadAllTextAsync(manifestPath, cancellationToken), JsonOptions)
            ?? throw new InvalidOperationException("Plugin manifest is empty.");

        ValidateManifest(manifest);

        var assemblyPath = Path.GetFullPath(Path.Combine(sourceDirectory, manifest.EntryAssembly));
        if (!File.Exists(assemblyPath))
        {
            throw new InvalidOperationException("The plugin entry assembly does not exist.");
        }

        var targetDirectory = Path.Combine(environment.ContentRootPath, "App_Data", "plugins", manifest.Id);
        Directory.CreateDirectory(targetDirectory);

        await CopyDirectoryAsync(sourceDirectory, targetDirectory, cancellationToken);
        return (manifest, targetDirectory);
    }

    private static void ValidateManifest(PluginManifest manifest)
    {
        if (string.IsNullOrWhiteSpace(manifest.Id) || string.IsNullOrWhiteSpace(manifest.EntryAssembly) || string.IsNullOrWhiteSpace(manifest.EntryType))
        {
            throw new InvalidOperationException("Plugin manifest requires Id, EntryAssembly and EntryType.");
        }

        if (Path.IsPathRooted(manifest.EntryAssembly) || manifest.EntryAssembly.Contains("..", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Plugin EntryAssembly must be a relative path inside the package.");
        }
    }

    private static async Task CopyDirectoryAsync(string source, string target, CancellationToken cancellationToken)
    {
        foreach (var directory in Directory.EnumerateDirectories(source, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(Path.Combine(target, Path.GetRelativePath(source, directory)));
        }

        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            var destination = Path.Combine(target, Path.GetRelativePath(source, file));
            await using var input = File.OpenRead(file);
            await using var output = File.Create(destination);
            await input.CopyToAsync(output, cancellationToken);
        }
    }
}

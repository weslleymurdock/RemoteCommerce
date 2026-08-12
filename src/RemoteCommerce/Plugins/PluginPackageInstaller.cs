using System.IO.Compression;
using System.Text.Json;
using RemoteCommerce.Plugins.Abstractions;

namespace RemoteCommerce.Plugins;

/// <summary>
/// Validates and installs RemoteCommerce plugin NuGet packages.
/// </summary>
/// <param name="environment">The host environment used to resolve the plugin installation root.</param>
public sealed class PluginPackageInstaller(IWebHostEnvironment environment)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Validates a plugin NuGet package and extracts it into its stable installation directory.
    /// </summary>
    /// <param name="packagePath">The path to the <c>.nupkg</c> file to install.</param>
    /// <param name="cancellationToken">The token used to cancel package processing.</param>
    /// <returns>The validated manifest and installation directory.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the package file does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the package or manifest is invalid, or the plugin is already installed.</exception>
    public async Task<(PluginManifest Manifest, string TargetDirectory)> InstallAsync(string packagePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(packagePath))
            throw new FileNotFoundException("The plugin package was not found.", packagePath);
        if (!string.Equals(Path.GetExtension(packagePath), ".nupkg", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("RemoteCommerce plugins must be distributed as .nupkg files.");

        using var archive = ZipFile.OpenRead(packagePath);
        var manifestEntry = archive.GetEntry("plugin.manifest.json")
            ?? throw new InvalidOperationException("The plugin package must contain plugin.manifest.json at its root.");
        await using var manifestStream = manifestEntry.Open();
        var manifest = await JsonSerializer.DeserializeAsync<PluginManifest>(manifestStream, JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("The plugin manifest is empty or invalid.");

        ValidateManifest(manifest);
        var assemblyEntry = archive.GetEntry(manifest.EntryAssembly.Replace('\\', '/'))
            ?? throw new InvalidOperationException($"The plugin entry assembly '{manifest.EntryAssembly}' was not found in the package.");
        if (!assemblyEntry.FullName.StartsWith("lib/net10.0/", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("The plugin entry assembly must be located under lib/net10.0 in the package.");

        var targetDirectory = Path.Combine(environment.ContentRootPath, "App_Data", "plugins", manifest.Id);
        if (Directory.Exists(targetDirectory))
            throw new InvalidOperationException($"Plugin '{manifest.Id}' is already installed. Uninstall it before installing another package.");

        var stagingDirectory = Path.Combine(environment.ContentRootPath, "App_Data", "plugins", ".staging", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(stagingDirectory);
        try
        {
            ExtractSafely(archive, stagingDirectory);
            Directory.Move(stagingDirectory, targetDirectory);
        }
        catch
        {
            if (Directory.Exists(stagingDirectory))
                Directory.Delete(stagingDirectory, recursive: true);
            throw;
        }

        return (manifest, targetDirectory);
    }

    private static void ValidateManifest(PluginManifest manifest)
    {
        if (string.IsNullOrWhiteSpace(manifest.Id) || string.IsNullOrWhiteSpace(manifest.Name) || string.IsNullOrWhiteSpace(manifest.Version) ||
            string.IsNullOrWhiteSpace(manifest.EntryAssembly) || string.IsNullOrWhiteSpace(manifest.EntryType) || string.IsNullOrWhiteSpace(manifest.MinHostVersion))
            throw new InvalidOperationException("The plugin manifest requires Id, Name, Version, EntryAssembly, EntryType and MinHostVersion.");

        if (manifest.Id.Contains("..", StringComparison.Ordinal) || manifest.Id.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
            manifest.Id.Contains('/') || manifest.Id.Contains('\\'))
            throw new InvalidOperationException("The plugin manifest Id is not a safe directory name.");

        if (!Version.TryParse(manifest.Version, out _) || !Version.TryParse(manifest.MinHostVersion, out var minimumHostVersion))
            throw new InvalidOperationException("Plugin Version and MinHostVersion must be valid semantic version-compatible values.");

        var hostVersion = typeof(PluginPackageInstaller).Assembly.GetName().Version ?? new Version(0, 0);
        if (hostVersion < minimumHostVersion)
            throw new InvalidOperationException($"Plugin requires host version {minimumHostVersion}, but the current host is {hostVersion}.");

        ValidateRelativePath(manifest.EntryAssembly);
    }

    private static void ValidateRelativePath(string path)
    {
        if (Path.IsPathRooted(path) || path.Contains("..", StringComparison.Ordinal) || path.StartsWith("/", StringComparison.Ordinal) || path.StartsWith("\\", StringComparison.Ordinal))
            throw new InvalidOperationException($"Plugin path '{path}' is not a safe package-relative path.");
    }

    private static void ExtractSafely(ZipArchive archive, string targetDirectory)
    {
        var root = Path.GetFullPath(targetDirectory) + Path.DirectorySeparatorChar;
        foreach (var entry in archive.Entries)
        {
            var normalized = entry.FullName.Replace('\\', '/');
            if (string.IsNullOrEmpty(normalized) || normalized.EndsWith('/'))
                continue;
            ValidateRelativePath(normalized);
            var destination = Path.GetFullPath(Path.Combine(targetDirectory, normalized.Replace('/', Path.DirectorySeparatorChar)));
            if (!destination.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("The plugin package contains a path traversal entry.");
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            entry.ExtractToFile(destination, overwrite: true);
        }
    }
}

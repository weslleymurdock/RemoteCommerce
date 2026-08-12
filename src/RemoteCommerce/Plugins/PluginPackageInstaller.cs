using System.IO.Compression;
using RemoteCommerce.Plugins.Abstractions;

namespace RemoteCommerce.Plugins;

/// <summary>Validates and installs RemoteCommerce plugin NuGet packages into an isolated application data directory.</summary>
/// <param name="environment">The host environment used to resolve the plugin installation root.</param>
/// <param name="packageValidator">The package validator used before any package extraction occurs.</param>
public sealed class PluginPackageInstaller(
    IWebHostEnvironment environment,
    IPluginPackageValidator packageValidator)
{
    /// <summary>Validates a plugin package and extracts it into a staging directory before atomically installing it.</summary>
    /// <param name="packagePath">The path to the <c>.nupkg</c> file to install.</param>
    /// <param name="versionDirectoryName">The directory name used for this package version.</param>
    /// <param name="cancellationToken">The token used to cancel package processing.</param>
    /// <returns>The validated manifest, package hash, and installation directory.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the plugin package file does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown when package validation fails or the target directory already exists.</exception>
    public async Task<(PluginManifest Manifest, string PackageHash, string TargetDirectory)> InstallAsync(
        string packagePath,
        string versionDirectoryName,
        CancellationToken cancellationToken = default)
    {
        var validation = await packageValidator.ValidateAsync(packagePath, cancellationToken);
        if (!validation.IsValid || validation.Manifest is null)
            throw new InvalidOperationException(string.Join(" ", validation.Issues.Where(x => x.Severity == PluginValidationSeverity.Error).Select(x => x.Message)));

        var manifest = validation.Manifest;
        var pluginsRoot = Path.Combine(environment.ContentRootPath, "App_Data", "plugins");
        var pluginRoot = Path.Combine(pluginsRoot, manifest.Id);
        var targetDirectory = Path.Combine(pluginRoot, versionDirectoryName);
        if (Directory.Exists(targetDirectory))
            throw new InvalidOperationException($"Plugin version '{manifest.Id} {manifest.Version}' is already installed.");

        var stagingDirectory = Path.Combine(pluginsRoot, ".staging", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(stagingDirectory);
        try
        {
            ExtractSafely(packagePath, stagingDirectory);
            Directory.CreateDirectory(pluginRoot);
            Directory.Move(stagingDirectory, targetDirectory);
            return (manifest, validation.PackageHash, targetDirectory);
        }
        catch
        {
            if (Directory.Exists(stagingDirectory)) Directory.Delete(stagingDirectory, true);
            throw;
        }
    }

    private static void ExtractSafely(string packagePath, string targetDirectory)
    {
        using var archive = ZipFile.OpenRead(packagePath);
        var root = Path.GetFullPath(targetDirectory) + Path.DirectorySeparatorChar;
        foreach (var entry in archive.Entries)
        {
            var normalized = entry.FullName.Replace('\\', '/');
            if (string.IsNullOrEmpty(normalized) || normalized.EndsWith('/')) continue;
            if (Path.IsPathRooted(normalized) || normalized.Contains("..", StringComparison.Ordinal) || normalized.StartsWith('/'))
                throw new InvalidOperationException($"The plugin package contains an unsafe path '{entry.FullName}'.");

            var destination = Path.GetFullPath(Path.Combine(targetDirectory, normalized.Replace('/', Path.DirectorySeparatorChar)));
            if (!destination.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("The plugin package contains a path traversal entry.");

            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            entry.ExtractToFile(destination, true);
        }
    }
}

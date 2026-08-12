using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RemoteCommerce.Infrastructure.Persistence;
using RemoteCommerce.Infrastructure.Persistence.Entities;
using RemoteCommerce.Plugins.Abstractions;

namespace RemoteCommerce.Plugins;

/// <summary>
/// Installs trusted plugin packages and persists their activation metadata for the next host restart.
/// </summary>
/// <param name="dbFactory">The factory used to create persistence contexts.</param>
/// <param name="environment">The host environment used to resolve application data paths.</param>
public sealed class PluginInstallationService(
    IDbContextFactory<CommerceDbContext> dbFactory,
    IWebHostEnvironment environment)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Installs a plugin package into the host plugin directory.
    /// </summary>
    /// <param name="sourceDirectory">The directory containing the plugin manifest and entry assembly.</param>
    /// <param name="cancellationToken">The token used to cancel the installation operation.</param>
    /// <returns>The installed plugin manifest.</returns>
    /// <exception cref="DirectoryNotFoundException">Thrown when the source directory does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the package manifest is invalid or conflicts with an installed plugin.</exception>
    public async Task<PluginManifest> InstallAsync(string sourceDirectory, CancellationToken cancellationToken = default)
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

        var entryAssemblyPath = Path.GetFullPath(Path.Combine(sourceDirectory, manifest.EntryAssembly));
        if (!File.Exists(entryAssemblyPath))
        {
            throw new InvalidOperationException("The plugin entry assembly does not exist.");
        }

        var targetDirectory = Path.Combine(environment.ContentRootPath, "App_Data", "plugins", manifest.Id);
        Directory.CreateDirectory(targetDirectory);

        await CopyDirectoryAsync(sourceDirectory, targetDirectory, cancellationToken);

        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var existing = await db.PluginInstallations.SingleOrDefaultAsync(x => x.PluginId == manifest.Id, cancellationToken);
        if (existing is not null)
        {
            throw new InvalidOperationException($"Plugin '{manifest.Id}' is already installed.");
        }

        db.PluginInstallations.Add(new PluginInstallation
        {
            Id = Guid.NewGuid(),
            PluginId = manifest.Id,
            Version = manifest.Version,
            PackagePath = targetDirectory,
            State = PluginInstallationState.Installed,
            InstalledAt = DateTimeOffset.UtcNow
        });

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return manifest;
    }

    private static void ValidateManifest(PluginManifest manifest)
    {
        if (string.IsNullOrWhiteSpace(manifest.Id) || string.IsNullOrWhiteSpace(manifest.EntryAssembly) || string.IsNullOrWhiteSpace(manifest.EntryType))
        {
            throw new InvalidOperationException("Plugin manifest requires Id, EntryAssembly and EntryType.");
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

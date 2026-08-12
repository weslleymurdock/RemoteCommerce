using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RemoteCommerce.Infrastructure.Persistence;
using RemoteCommerce.Infrastructure.Persistence.Entities;
using RemoteCommerce.Plugins.Abstractions;

namespace RemoteCommerce.Plugins;

/// <summary>Provides persistent lifecycle and information operations for installed RemoteCommerce plugins.</summary>
/// <param name="dbFactory">The factory used to create persistence contexts.</param>
public sealed class PluginManagementService(IDbContextFactory<CommerceDbContext> dbFactory)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>Gets installed plugins together with their package manifest metadata.</summary>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The installed plugin information ordered by plugin identifier.</returns>
    public async Task<IReadOnlyList<PluginInformation>> ListAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var installations = await db.PluginInstallations.AsNoTracking().OrderBy(x => x.PluginId).ToListAsync(cancellationToken);
        var result = new List<PluginInformation>(installations.Count);
        foreach (var installation in installations)
        {
            var manifestPath = Path.Combine(installation.PackagePath, "plugin.manifest.json");
            PluginManifest? manifest = null;
            if (File.Exists(manifestPath))
                manifest = JsonSerializer.Deserialize<PluginManifest>(await File.ReadAllTextAsync(manifestPath, cancellationToken), JsonOptions);
            result.Add(new PluginInformation(installation, manifest));
        }
        return result;
    }

    /// <summary>Reads the required README and license documents from an installed plugin package.</summary>
    /// <param name="pluginId">The stable identifier of the installed plugin.</param>
    /// <param name="cancellationToken">The token used to cancel file access.</param>
    /// <returns>The package documentation content.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the plugin is not installed.</exception>
    /// <exception cref="FileNotFoundException">Thrown when a required package documentation file is missing.</exception>
    public async Task<PluginDocumentation> GetDocumentationAsync(string pluginId, CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var installation = await db.PluginInstallations.AsNoTracking().SingleOrDefaultAsync(x => x.PluginId == pluginId, cancellationToken) ?? throw new KeyNotFoundException($"Plugin '{pluginId}' is not installed.");
        var readmePath = Path.Combine(installation.PackagePath, "README.md");
        var licensePath = Path.Combine(installation.PackagePath, "LICENSE.md");
        if (!File.Exists(readmePath)) throw new FileNotFoundException("The installed plugin README.md file was not found.", readmePath);
        if (!File.Exists(licensePath)) throw new FileNotFoundException("The installed plugin LICENSE.md file was not found.", licensePath);
        return new PluginDocumentation(await File.ReadAllTextAsync(readmePath, cancellationToken), await File.ReadAllTextAsync(licensePath, cancellationToken));
    }

    /// <summary>Disables a plugin for the next application startup.</summary>
    /// <param name="pluginId">The stable identifier of the plugin.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the plugin is not installed.</exception>
    public async Task DisableAsync(string pluginId, CancellationToken cancellationToken = default) => await SetStateAsync(pluginId, PluginInstallationState.Disabled, cancellationToken);

    /// <summary>Enables a disabled plugin for the next application startup.</summary>
    /// <param name="pluginId">The stable identifier of the plugin.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the plugin is not installed.</exception>
    public async Task EnableAsync(string pluginId, CancellationToken cancellationToken = default) => await SetStateAsync(pluginId, PluginInstallationState.Installed, cancellationToken);

    /// <summary>Uninstalls a plugin from persistent configuration and schedules its files for deletion.</summary>
    /// <param name="pluginId">The stable identifier of the plugin.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the plugin is not installed.</exception>
    public async Task UninstallAsync(string pluginId, CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var installation = await db.PluginInstallations.SingleOrDefaultAsync(x => x.PluginId == pluginId, cancellationToken) ?? throw new KeyNotFoundException($"Plugin '{pluginId}' is not installed.");
        db.PluginInstallations.Remove(installation);
        await db.SaveChangesAsync(cancellationToken);
        if (Directory.Exists(installation.PackagePath))
        {
            var pendingRoot = Path.Combine(Path.GetDirectoryName(installation.PackagePath)!, ".pending-delete");
            Directory.CreateDirectory(pendingRoot);
            Directory.Move(installation.PackagePath, Path.Combine(pendingRoot, $"{pluginId}-{Guid.NewGuid():N}"));
        }
    }

    private async Task SetStateAsync(string pluginId, PluginInstallationState state, CancellationToken cancellationToken)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var installation = await db.PluginInstallations.SingleOrDefaultAsync(x => x.PluginId == pluginId, cancellationToken) ?? throw new KeyNotFoundException($"Plugin '{pluginId}' is not installed.");
        installation.State = state;
        await db.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>Combines persisted installation state with package manifest metadata.</summary>
/// <param name="Installation">The persisted installation state.</param>
/// <param name="Manifest">The package manifest, when it can be read from disk.</param>
public sealed record PluginInformation(PluginInstallation Installation, PluginManifest? Manifest)
{
    /// <summary>Gets the plugin display name, falling back to its stable identifier.</summary>
    public string Name => Manifest?.Name ?? Installation.PluginId;
    /// <summary>Gets the package description.</summary>
    public string Description => Manifest?.Description ?? string.Empty;
    /// <summary>Gets the NuGet package identifier.</summary>
    public string PackageId => Manifest?.PackageId ?? Installation.PluginId;
    /// <summary>Gets the package title.</summary>
    public string Title => Manifest?.Title ?? string.Empty;
    /// <summary>Gets the package authors.</summary>
    public string Authors => Manifest?.Authors ?? string.Empty;
    /// <summary>Gets the package company.</summary>
    public string Company => Manifest?.Company ?? string.Empty;
    /// <summary>Gets the package tags.</summary>
    public string PackageTags => Manifest?.PackageTags ?? string.Empty;
    /// <summary>Gets the source repository URL.</summary>
    public string RepositoryUrl => Manifest?.RepositoryUrl ?? string.Empty;
    /// <summary>Gets the project homepage URL.</summary>
    public string PackageProjectUrl => Manifest?.PackageProjectUrl ?? string.Empty;
    /// <summary>Gets whether the package requires license acceptance.</summary>
    public bool PackageRequireLicenseAcceptance => Manifest?.PackageRequireLicenseAcceptance ?? false;
    /// <summary>Gets the stable plugin identifier.</summary>
    public string PluginId => Installation.PluginId;
    /// <summary>Gets the installed version.</summary>
    public string Version => Installation.Version;
    /// <summary>Gets the persisted lifecycle state.</summary>
    public PluginInstallationState State => Installation.State;
    /// <summary>Gets the installation timestamp.</summary>
    public DateTimeOffset InstalledAt => Installation.InstalledAt;
}

/// <summary>Contains the required documentation displayed for an installed plugin.</summary>
/// <param name="Readme">The README.md content.</param>
/// <param name="License">The LICENSE.md content.</param>
public sealed record PluginDocumentation(string Readme, string License);

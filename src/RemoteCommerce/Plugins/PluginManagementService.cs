using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RemoteCommerce.Infrastructure.Persistence;
using RemoteCommerce.Infrastructure.Persistence.Entities;
using RemoteCommerce.Plugins.Abstractions;

namespace RemoteCommerce.Plugins;

/// <summary>Provides persistent lifecycle, version, documentation, and administration operations for installed plugins.</summary>
/// <param name="dbFactory">The factory used to create persistence contexts.</param>
/// <param name="restartService">The service used to report restart requirements.</param>
public sealed class PluginManagementService(IDbContextFactory<CommerceDbContext> dbFactory, IApplicationRestartService restartService)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>Gets installed plugins together with package metadata, dependency information, and diagnostics.</summary>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The installed plugin information ordered by plugin identifier.</returns>
    public async Task<IReadOnlyList<PluginInformation>> ListAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var installations = await db.PluginInstallations.AsNoTracking().OrderBy(x => x.PluginId).ToListAsync(cancellationToken);
        var dependencies = await db.PluginDependencies.AsNoTracking().ToListAsync(cancellationToken);
        var errors = await db.PluginLifecycleErrors.AsNoTracking().OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);
        var latestErrors = errors.GroupBy(x => x.PluginId, StringComparer.OrdinalIgnoreCase).ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);
        var result = new List<PluginInformation>(installations.Count);
        foreach (var installation in installations)
        {
            var manifestPath = Path.Combine(installation.PackagePath, "plugin.manifest.json");
            PluginManifest? manifest = null;
            if (File.Exists(manifestPath))
                manifest = JsonSerializer.Deserialize<PluginManifest>(await File.ReadAllTextAsync(manifestPath, cancellationToken), JsonOptions);
            var pluginDependencies = dependencies.Where(x => string.Equals(x.PluginId, installation.PluginId, StringComparison.OrdinalIgnoreCase)).ToArray();
            latestErrors.TryGetValue(installation.PluginId, out var latestError);
            result.Add(new PluginInformation(installation, manifest, pluginDependencies, latestError));
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

    /// <summary>Requests that a plugin be enabled during the next application startup.</summary>
    /// <param name="pluginId">The stable identifier of the plugin.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the plugin is not installed.</exception>
    public Task EnableAsync(string pluginId, CancellationToken cancellationToken = default) => SetDesiredStateAsync(pluginId, PluginDesiredState.Enabled, cancellationToken);

    /// <summary>Requests that a plugin be disabled during the next application startup.</summary>
    /// <param name="pluginId">The stable identifier of the plugin.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the plugin is not installed.</exception>
    public Task DisableAsync(string pluginId, CancellationToken cancellationToken = default) => SetDesiredStateAsync(pluginId, PluginDesiredState.Disabled, cancellationToken);

    /// <summary>Uninstalls a plugin after protecting plugins that declare it as a required dependency.</summary>
    /// <param name="pluginId">The stable identifier of the plugin.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the plugin is not installed.</exception>
    /// <exception cref="InvalidOperationException">Thrown when another installed plugin depends on the plugin.</exception>
    public async Task UninstallAsync(string pluginId, CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var installation = await db.PluginInstallations.SingleOrDefaultAsync(x => x.PluginId == pluginId, cancellationToken) ?? throw new KeyNotFoundException($"Plugin '{pluginId}' is not installed.");
        var dependents = await db.PluginDependencies.AsNoTracking().Where(x => x.DependencyPluginId == pluginId && x.PluginId != pluginId).Select(x => x.PluginId).Distinct().ToListAsync(cancellationToken);
        if (dependents.Count > 0) throw new InvalidOperationException($"Plugin '{pluginId}' cannot be uninstalled because it is required by: {string.Join(", ", dependents)}.");

        db.PluginDependencies.RemoveRange(db.PluginDependencies.Where(x => x.PluginId == pluginId));
        db.PluginSettings.RemoveRange(db.PluginSettings.Where(x => x.PluginId == pluginId));
        db.PluginVersions.RemoveRange(db.PluginVersions.Where(x => x.PluginId == pluginId));
        db.PluginInstallations.Remove(installation);
        await db.SaveChangesAsync(cancellationToken);
        MovePackageToPendingDelete(installation.PackagePath);
        restartService.RequestRestart($"Plugin '{pluginId}' was uninstalled and its loaded assembly requires restart removal.");
    }

    /// <summary>Schedules a retained plugin version as the version to activate after restart.</summary>
    /// <param name="pluginId">The stable identifier of the plugin.</param>
    /// <param name="version">The retained version to restore.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous rollback operation.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the plugin or requested version is not found.</exception>
    public async Task RollbackAsync(string pluginId, string version, CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var installation = await db.PluginInstallations.SingleOrDefaultAsync(x => x.PluginId == pluginId, cancellationToken) ?? throw new KeyNotFoundException($"Plugin '{pluginId}' is not installed.");
        var retained = await db.PluginVersions.SingleOrDefaultAsync(x => x.PluginId == pluginId && x.Version == version, cancellationToken) ?? throw new KeyNotFoundException($"Plugin version '{pluginId} {version}' is not retained.");
        var manifestPath = Path.Combine(retained.PackagePath, "plugin.manifest.json");
        var manifest = File.Exists(manifestPath) ? JsonSerializer.Deserialize<PluginManifest>(await File.ReadAllTextAsync(manifestPath, cancellationToken), JsonOptions) : null;
        installation.Version = retained.Version;
        installation.PackagePath = retained.PackagePath;
        installation.PackageHash = retained.PackageHash;
        installation.PendingVersion = retained.Version;
        installation.State = PluginInstallationState.ActivationPending;
        installation.UpdatedAt = DateTimeOffset.UtcNow;
        foreach (var item in db.PluginVersions.Where(x => x.PluginId == pluginId)) item.IsCurrent = item.Id == retained.Id;
        db.PluginDependencies.RemoveRange(db.PluginDependencies.Where(x => x.PluginId == pluginId));
        if (manifest is not null)
            foreach (var dependency in manifest.DependencyDeclarations)
                db.PluginDependencies.Add(new PluginDependency { Id = Guid.NewGuid(), PluginId = pluginId, DependencyPluginId = dependency.PluginId, MinimumVersion = dependency.MinimumVersion, MaximumVersion = dependency.MaximumVersion });
        await db.SaveChangesAsync(cancellationToken);
        restartService.RequestRestart($"Plugin '{pluginId}' rollback to {version} is pending activation.");
    }

    /// <summary>Gets all retained versions of an installed plugin.</summary>
    /// <param name="pluginId">The stable identifier of the plugin.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The retained versions ordered newest first.</returns>
    public async Task<IReadOnlyList<PluginVersion>> GetVersionsAsync(string pluginId, CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.PluginVersions.AsNoTracking().Where(x => x.PluginId == pluginId).OrderByDescending(x => x.InstalledAt).ToListAsync(cancellationToken);
    }

    private async Task SetDesiredStateAsync(string pluginId, PluginDesiredState desiredState, CancellationToken cancellationToken)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var installation = await db.PluginInstallations.SingleOrDefaultAsync(x => x.PluginId == pluginId, cancellationToken) ?? throw new KeyNotFoundException($"Plugin '{pluginId}' is not installed.");
        installation.DesiredState = desiredState;
        installation.State = PluginInstallationState.ActivationPending;
        installation.LastError = null;
        installation.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        restartService.RequestRestart($"Plugin '{pluginId}' {desiredState.ToString().ToLowerInvariant()} state is pending application restart.");
    }

    private static void MovePackageToPendingDelete(string packagePath)
    {
        if (!Directory.Exists(packagePath)) return;
        var pluginRoot = Directory.GetParent(packagePath)?.Parent?.FullName ?? Path.GetDirectoryName(packagePath)!;
        var pendingRoot = Path.Combine(pluginRoot, ".pending-delete");
        Directory.CreateDirectory(pendingRoot);
        Directory.Move(packagePath, Path.Combine(pendingRoot, $"{Path.GetFileName(packagePath)}-{Guid.NewGuid():N}"));
    }
}

/// <summary>Combines persisted installation state with package manifest metadata and diagnostics.</summary>
/// <param name="Installation">The persisted installation state.</param>
/// <param name="Manifest">The package manifest, when it can be read from disk.</param>
/// <param name="Dependencies">The dependencies declared by the installed plugin.</param>
/// <param name="LatestError">The most recent persisted lifecycle error.</param>
public sealed record PluginInformation(PluginInstallation Installation, PluginManifest? Manifest, IReadOnlyList<PluginDependency> Dependencies, PluginLifecycleError? LatestError)
{
    /// <summary>Gets the plugin display name, falling back to its stable identifier.</summary>
    public string Name => Manifest?.Name ?? Installation.PluginId;
    /// <summary>Gets the package description.</summary>
    public string Description => Manifest?.Description ?? string.Empty;
    /// <summary>Gets the package identifier.</summary>
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
    /// <summary>Gets the administrative desired state.</summary>
    public PluginDesiredState DesiredState => Installation.DesiredState;
    /// <summary>Gets the installed package SHA-256 hash.</summary>
    public string PackageHash => Installation.PackageHash;
    /// <summary>Gets the installation timestamp.</summary>
    public DateTimeOffset InstalledAt => Installation.InstalledAt;
}

/// <summary>Contains the required documentation displayed for an installed plugin.</summary>
/// <param name="Readme">The README.md content.</param>
/// <param name="License">The LICENSE.md content.</param>
public sealed record PluginDocumentation(string Readme, string License);

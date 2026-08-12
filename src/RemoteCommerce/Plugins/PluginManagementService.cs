using Microsoft.EntityFrameworkCore;
using RemoteCommerce.Infrastructure.Persistence;
using RemoteCommerce.Infrastructure.Persistence.Entities;

namespace RemoteCommerce.Plugins;

/// <summary>Provides persistent lifecycle and information operations for installed RemoteCommerce plugins.</summary>
/// <param name="dbFactory">The factory used to create persistence contexts.</param>
public sealed class PluginManagementService(IDbContextFactory<CommerceDbContext> dbFactory)
{
    /// <summary>Gets the currently persisted plugin installations.</summary>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The installed plugin records ordered by plugin identifier.</returns>
    public async Task<IReadOnlyList<PluginInstallation>> ListAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.PluginInstallations.AsNoTracking().OrderBy(x => x.PluginId).ToListAsync(cancellationToken);
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
        var installation = await db.PluginInstallations.AsNoTracking().SingleOrDefaultAsync(x => x.PluginId == pluginId, cancellationToken)
            ?? throw new KeyNotFoundException($"Plugin '{pluginId}' is not installed.");

        var readmePath = Path.Combine(installation.PackagePath, "README.md");
        var licensePath = Path.Combine(installation.PackagePath, "LICENSE.md");
        if (!File.Exists(readmePath)) throw new FileNotFoundException("The installed plugin README.md file was not found.", readmePath);
        if (!File.Exists(licensePath)) throw new FileNotFoundException("The installed plugin LICENSE.md file was not found.", licensePath);

        return new PluginDocumentation(
            await File.ReadAllTextAsync(readmePath, cancellationToken),
            await File.ReadAllTextAsync(licensePath, cancellationToken));
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

/// <summary>Contains the required documentation displayed for an installed plugin.</summary>
/// <param name="Readme">The README.md content.</param>
/// <param name="License">The LICENSE.md content.</param>
public sealed record PluginDocumentation(string Readme, string License);

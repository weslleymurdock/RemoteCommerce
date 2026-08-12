using Microsoft.EntityFrameworkCore;
using RemoteCommerce.Infrastructure.Persistence;
using RemoteCommerce.Infrastructure.Persistence.Entities;

namespace RemoteCommerce.Plugins;

/// <summary>
/// Provides persistent lifecycle operations for installed RemoteCommerce plugins.
/// </summary>
/// <param name="dbFactory">The factory used to create persistence contexts.</param>
public sealed class PluginManagementService(IDbContextFactory<CommerceDbContext> dbFactory)
{
    /// <summary>
    /// Gets the currently persisted plugin installations.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The installed plugin records ordered by plugin identifier.</returns>
    public async Task<IReadOnlyList<PluginInstallation>> ListAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.PluginInstallations.AsNoTracking().OrderBy(x => x.PluginId).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Disables a plugin for the next application startup.
    /// </summary>
    /// <param name="pluginId">The stable identifier of the plugin.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the plugin is not installed.</exception>
    public async Task DisableAsync(string pluginId, CancellationToken cancellationToken = default)
    {
        await SetStateAsync(pluginId, PluginInstallationState.Disabled, cancellationToken);
    }

    /// <summary>
    /// Enables a disabled plugin for the next application startup.
    /// </summary>
    /// <param name="pluginId">The stable identifier of the plugin.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the plugin is not installed.</exception>
    public async Task EnableAsync(string pluginId, CancellationToken cancellationToken = default)
    {
        await SetStateAsync(pluginId, PluginInstallationState.Installed, cancellationToken);
    }

    /// <summary>
    /// Uninstalls a plugin from persistent configuration and schedules its files for deletion.
    /// </summary>
    /// <param name="pluginId">The stable identifier of the plugin.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the plugin is not installed.</exception>
    public async Task UninstallAsync(string pluginId, CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var installation = await db.PluginInstallations.SingleOrDefaultAsync(x => x.PluginId == pluginId, cancellationToken)
            ?? throw new KeyNotFoundException($"Plugin '{pluginId}' is not installed.");

        db.PluginInstallations.Remove(installation);
        await db.SaveChangesAsync(cancellationToken);

        if (Directory.Exists(installation.PackagePath))
        {
            var pendingRoot = Path.Combine(Path.GetDirectoryName(installation.PackagePath)!, ".pending-delete");
            Directory.CreateDirectory(pendingRoot);
            var pendingPath = Path.Combine(pendingRoot, $"{pluginId}-{Guid.NewGuid():N}");
            Directory.Move(installation.PackagePath, pendingPath);
        }
    }

    private async Task SetStateAsync(string pluginId, PluginInstallationState state, CancellationToken cancellationToken)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var installation = await db.PluginInstallations.SingleOrDefaultAsync(x => x.PluginId == pluginId, cancellationToken)
            ?? throw new KeyNotFoundException($"Plugin '{pluginId}' is not installed.");

        installation.State = state;
        await db.SaveChangesAsync(cancellationToken);
    }
}

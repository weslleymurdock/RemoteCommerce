using Microsoft.EntityFrameworkCore;
using RemoteCommerce.Infrastructure.Persistence;
using RemoteCommerce.Infrastructure.Persistence.Entities;

namespace RemoteCommerce.Plugins;

/// <summary>Provides persistent key/value settings for plugin-owned configuration.</summary>
/// <param name="dbFactory">The factory used to create persistence contexts.</param>
public sealed class PluginSettingsService(IDbContextFactory<CommerceDbContext> dbFactory)
{
    /// <summary>Gets a plugin setting value.</summary>
    /// <param name="pluginId">The stable plugin identifier.</param>
    /// <param name="key">The plugin-defined setting key.</param>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The stored value, or <see langword="null"/> when the setting does not exist.</returns>
    public async Task<string?> GetAsync(string pluginId, string key, CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.PluginSettings.AsNoTracking().Where(x => x.PluginId == pluginId && x.Key == key).Select(x => x.Value).SingleOrDefaultAsync(cancellationToken);
    }

    /// <summary>Creates or updates a plugin setting value and optional metadata.</summary>
    /// <param name="pluginId">The stable plugin identifier.</param>
    /// <param name="key">The plugin-defined setting key.</param>
    /// <param name="value">The serialized setting value.</param>
    /// <param name="metadata">Optional JSON metadata describing the setting.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous persistence operation.</returns>
    public async Task SetAsync(string pluginId, string key, string value, string? metadata = null, CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var setting = await db.PluginSettings.SingleOrDefaultAsync(x => x.PluginId == pluginId && x.Key == key, cancellationToken);
        if (setting is null)
        {
            db.PluginSettings.Add(new PluginSetting { Id = Guid.NewGuid(), PluginId = pluginId, Key = key, Value = value, Metadata = metadata });
        }
        else
        {
            setting.Value = value;
            setting.Metadata = metadata;
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}

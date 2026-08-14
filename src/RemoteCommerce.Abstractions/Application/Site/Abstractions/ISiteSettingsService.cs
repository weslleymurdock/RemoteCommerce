namespace RemoteCommerce.Application.Site.Abstractions;

/// <summary>Provides validated access to persistent application/site settings.</summary>
public interface ISiteSettingsService
{
    /// <summary>Gets the current site settings, creating safe defaults when no record exists.</summary>
    /// <returns>The current site settings.</returns>
    Task<SiteSettingsModel> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>Validates and persists new site settings.</summary>
    /// <param name="settings">The settings to validate and persist.</param>
    /// <param name="userId">The authenticated actor responsible for the change, when available.</param>
    /// <param name="actor">The display name of the actor.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>A task that completes when the settings have been persisted.</returns>
    Task UpdateAsync(SiteSettingsModel settings, Guid? userId, string actor, CancellationToken cancellationToken = default);
}

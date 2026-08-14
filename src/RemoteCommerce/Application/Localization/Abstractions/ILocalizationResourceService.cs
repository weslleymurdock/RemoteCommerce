namespace RemoteCommerce.Application.Localization.Abstractions;

/// <summary>Defines administration operations for imported localization resources.</summary>
public interface ILocalizationResourceService
{
    /// <summary>Validates, versions, persists, and activates an XML resource.</summary>
    /// <param name="content">The XML resource stream.</param>
    /// <param name="culture">The resource culture.</param>
    /// <param name="resourceType">The resource marker type name.</param>
    /// <param name="importedByUserId">The importing user identifier.</param>
    /// <param name="actor">The importing actor display name.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>The imported resource metadata.</returns>
    Task<LocalizationResourceImportResult> ImportAsync(Stream content, string culture, string resourceType, Guid? importedByUserId, string actor, CancellationToken cancellationToken = default);

    /// <summary>Lists imported localization resource versions.</summary>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>The known resource versions ordered newest first.</returns>
    Task<IReadOnlyList<LocalizationResourceSummary>> ListAsync(CancellationToken cancellationToken = default);
}

/// <summary>Describes a successfully imported localization resource.</summary>
/// <param name="Culture">The resource culture.</param>
/// <param name="ResourceType">The resource marker type.</param>
/// <param name="Version">The assigned resource version.</param>
/// <param name="EntryCount">The number of validated resource entries.</param>
/// <param name="ContentHash">The SHA-256 content hash.</param>
public sealed record LocalizationResourceImportResult(string Culture, string ResourceType, int Version, int EntryCount, string ContentHash);

/// <summary>Describes an imported localization resource version.</summary>
/// <param name="Id">The persistence identifier.</param>
/// <param name="Culture">The resource culture.</param>
/// <param name="ResourceType">The resource marker type.</param>
/// <param name="Version">The resource version.</param>
/// <param name="ImportedAt">The import timestamp.</param>
/// <param name="IsActive">Whether this version is active.</param>
public sealed record LocalizationResourceSummary(long Id, string Culture, string ResourceType, int Version, DateTime ImportedAt, bool IsActive);

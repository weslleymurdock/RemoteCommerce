namespace RemoteCommerce.Infrastructure.Persistence.Entities;

/// <summary>Tracks an imported localization resource version without duplicating its content.</summary>
public sealed class LocalizationResource : ISoftDeletable
{
    /// <summary>Gets or sets the resource record identifier.</summary>
    public long Id { get; set; }

    /// <summary>Gets or sets the resource culture.</summary>
    public string Culture { get; set; } = string.Empty;

    /// <summary>Gets or sets the resource type identifier.</summary>
    public string ResourceType { get; set; } = string.Empty;

    /// <summary>Gets or sets the resource content hash.</summary>
    public string ContentHash { get; set; } = string.Empty;

    /// <summary>Gets or sets the resource version number.</summary>
    public int Version { get; set; }

    /// <summary>Gets or sets the identity of the user who imported the resource.</summary>
    public Guid? ImportedByUserId { get; set; }

    /// <summary>Gets or sets the UTC timestamp of the import.</summary>
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets whether this version is currently active.</summary>
    public bool IsActive { get; set; }

    /// <inheritdoc />
    public bool IsDisabled { get; set; }
}

namespace RemoteCommerce.Domain.Catalog.Entities;

/// <summary>Provides common persistence-independent state for mutable catalog entities.</summary>
public abstract class CatalogEntity : ISoftDeletable
{
    /// <summary>Gets the stable entity identifier.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Gets the UTC creation timestamp.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets the UTC timestamp of the last update.</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets whether the entity has been soft-deleted.</summary>
    public bool IsDeleted { get; set; }

    /// <summary>Gets or sets the UTC timestamp at which the entity was soft-deleted.</summary>
    public DateTimeOffset? DeletedAt { get; set; }

    /// <summary>Gets or sets whether the entity is disabled.</summary>
    public bool IsDisabled { get; set; }
}

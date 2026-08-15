namespace RemoteCommerce.Infrastructure.Persistence.Entities;

/// <summary>Represents a named authorization role in RemoteCommerce.</summary>
public sealed class ApplicationRole : IdentityRole<Guid>, ISoftDeletable
{
    /// <summary>Gets or sets a human-readable description of the role.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets whether the role has been soft-deleted.</summary>
    public bool IsDeleted { get; set; }

    /// <summary>Gets or sets the UTC timestamp at which the role was soft-deleted.</summary>
    public DateTimeOffset? DeletedAt { get; set; }

    /// <summary>Gets or sets whether the role is disabled.</summary>
    public bool IsDisabled { get; set; }
}

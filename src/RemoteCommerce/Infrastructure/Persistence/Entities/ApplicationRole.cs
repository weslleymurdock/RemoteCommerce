namespace RemoteCommerce.Infrastructure.Persistence.Entities;

/// <summary>Represents a named authorization role in RemoteCommerce.</summary>
public sealed class ApplicationRole : IdentityRole<Guid>, ISoftDeletable
{
    /// <summary>Gets or sets a human-readable description of the role.</summary>
    public string Description { get; set; } = string.Empty;

    /// <inheritdoc />
    public bool IsDeleted { get; set; }

    /// <inheritdoc />
    public DateTimeOffset? DeletedAt { get; set; }
}

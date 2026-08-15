namespace RemoteCommerce.Infrastructure.Persistence.Entities;

/// <summary>Represents an authenticated RemoteCommerce administrator or user.</summary>
public sealed class ApplicationUser : IdentityUser<Guid>, ISoftDeletable
{
    /// <summary>Gets or sets the display name shown in administrative interfaces.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets whether the user has been soft-deleted.</summary>
    public bool IsDeleted { get; set; }

    /// <summary>Gets or sets the UTC timestamp at which the user was soft-deleted.</summary>
    public DateTimeOffset? DeletedAt { get; set; }

    /// <summary>Gets or sets whether the user is disabled.</summary>
    public bool IsDisabled { get; set; }
}

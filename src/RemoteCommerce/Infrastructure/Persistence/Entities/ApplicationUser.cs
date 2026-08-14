namespace RemoteCommerce.Infrastructure.Persistence.Entities;

/// <summary>Represents an authenticated RemoteCommerce administrator or user.</summary>
public sealed class ApplicationUser : IdentityUser<Guid>, ISoftDeletable
{
    /// <summary>Gets or sets the display name shown in administrative interfaces.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <inheritdoc />
    public bool IsDisabled { get; set; }
}

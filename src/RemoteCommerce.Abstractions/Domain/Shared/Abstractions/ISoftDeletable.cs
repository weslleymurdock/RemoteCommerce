namespace RemoteCommerce.Domain.Shared.Abstractions;

/// <summary>Defines the shared soft-delete contract for mutable domain records.</summary>
public interface ISoftDeletable
{
    /// <summary>Gets or sets whether the record has been soft-deleted.</summary>
    bool IsDeleted { get; set; }

    /// <summary>Gets or sets the UTC timestamp at which the record was deleted.</summary>
    DateTimeOffset? DeletedAt { get; set; }

    /// <summary>Gets or sets whether the record is disabled.</summary>
    bool IsDisabled { get; set; }
}

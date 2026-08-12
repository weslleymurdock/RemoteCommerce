namespace RemoteCommerce.Infrastructure.Persistence;

/// <summary>Defines the persistence contract for records that are removed without physical deletion.</summary>
public interface ISoftDeletable
{
    /// <summary>Gets or sets whether the record is excluded from normal application queries.</summary>
    bool IsDeleted { get; set; }

    /// <summary>Gets or sets the UTC timestamp at which the record was soft-deleted.</summary>
    DateTimeOffset? DeletedAt { get; set; }
}

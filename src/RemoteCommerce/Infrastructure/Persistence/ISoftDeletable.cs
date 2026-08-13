namespace RemoteCommerce.Infrastructure.Persistence;

/// <summary>Defines the persistence contract for records that are excluded from normal queries instead of being physically deleted.</summary>
public interface ISoftDeletable
{
    /// <summary>Gets or sets whether the record has been soft-deleted.</summary>
    bool IsDisabled { get; set; }
}

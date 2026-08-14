namespace RemoteCommerce.Domain.Shared.Abstractions;

/// <summary>Defines the soft-delete state required by mutable domain entities.</summary>
public interface ISoftDeletable
{
    /// <summary>Gets or sets whether the entity is excluded from normal queries.</summary>
    bool IsDisabled { get; set; }
}

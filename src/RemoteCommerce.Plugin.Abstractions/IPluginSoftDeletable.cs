namespace RemoteCommerce.Plugins.Abstractions;

/// <summary>Marks a mutable plugin entity as participating in the RemoteCommerce soft-delete policy.</summary>
public interface IPluginSoftDeletable
{
    /// <summary>Gets or sets whether the entity is excluded from normal queries.</summary>
    bool IsDisabled { get; set; }
}

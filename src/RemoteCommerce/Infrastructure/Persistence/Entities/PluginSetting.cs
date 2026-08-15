namespace RemoteCommerce.Infrastructure.Persistence.Entities;

/// <summary>Stores a plugin-owned configuration value without copying package-defined documentation into the database.</summary>
public sealed class PluginSetting : ISoftDeletable
{
    /// <summary>Gets or sets the persistent setting identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the stable plugin identifier that owns the setting.</summary>
    public string PluginId { get; set; } = string.Empty;

    /// <summary>Gets or sets the plugin-defined setting key.</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Gets or sets the serialized setting value.</summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>Gets or sets optional JSON metadata describing the setting.</summary>
    public string? Metadata { get; set; }

    /// <summary>Gets or sets whether the setting has been soft-deleted.</summary>
    public bool IsDeleted { get; set; }

    /// <summary>Gets or sets the UTC timestamp at which the setting was soft-deleted.</summary>
    public DateTimeOffset? DeletedAt { get; set; }

    /// <summary>Gets or sets whether the setting is disabled.</summary>
    public bool IsDisabled { get; set; }
}

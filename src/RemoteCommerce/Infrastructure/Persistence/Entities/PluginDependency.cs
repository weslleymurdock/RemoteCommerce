namespace RemoteCommerce.Infrastructure.Persistence.Entities;

/// <summary>Represents a persisted dependency declared by an installed plugin version.</summary>
public sealed class PluginDependency : Persistence.ISoftDeletable
{
    /// <summary>Gets or sets the persistent dependency identifier.</summary>
    public Guid Id { get; set; }
    /// <summary>Gets or sets the plugin that owns the dependency declaration.</summary>
    public string PluginId { get; set; } = string.Empty;
    /// <summary>Gets or sets the required plugin identifier.</summary>
    public string DependencyPluginId { get; set; } = string.Empty;
    /// <summary>Gets or sets the minimum supported dependency version.</summary>
    public string MinimumVersion { get; set; } = string.Empty;
    /// <summary>Gets or sets the optional exclusive maximum dependency version.</summary>
    public string? MaximumVersion { get; set; }
    /// <inheritdoc />
    public bool IsDisabled { get; set; }
}

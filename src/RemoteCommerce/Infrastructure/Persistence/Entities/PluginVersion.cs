namespace RemoteCommerce.Infrastructure.Persistence.Entities;

/// <summary>Represents a package version retained for plugin update and rollback administration.</summary>
public sealed class PluginVersion
{
    /// <summary>Gets or sets the persistent version record identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the stable plugin identifier.</summary>
    public string PluginId { get; set; } = string.Empty;

    /// <summary>Gets or sets the semantic package version.</summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>Gets or sets the package path for this version.</summary>
    public string PackagePath { get; set; } = string.Empty;

    /// <summary>Gets or sets the SHA-256 integrity hash of the original package.</summary>
    public string PackageHash { get; set; } = string.Empty;

    /// <summary>Gets or sets the UTC timestamp at which this version was installed.</summary>
    public DateTimeOffset InstalledAt { get; set; }

    /// <summary>Gets or sets whether this version is currently selected for activation.</summary>
    public bool IsCurrent { get; set; }
}

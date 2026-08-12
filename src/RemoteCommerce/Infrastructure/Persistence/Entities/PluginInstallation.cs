using RemoteCommerce.Plugins;

namespace RemoteCommerce.Infrastructure.Persistence.Entities;

/// <summary>Represents the persisted administrative state of an installed RemoteCommerce plugin.</summary>
public sealed class PluginInstallation
{
    /// <summary>Gets or sets the persistent identifier of the installation record.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the stable plugin identifier declared by the manifest.</summary>
    public string PluginId { get; set; } = string.Empty;

    /// <summary>Gets or sets the currently selected plugin version.</summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>Gets or sets the absolute package directory containing the selected version.</summary>
    public string PackagePath { get; set; } = string.Empty;

    /// <summary>Gets or sets the persisted lifecycle state observed by the host.</summary>
    public PluginInstallationState State { get; set; }

    /// <summary>Gets or sets the administrative state requested for the next startup.</summary>
    public PluginDesiredState DesiredState { get; set; }

    /// <summary>Gets or sets the SHA-256 integrity hash of the installed package.</summary>
    public string PackageHash { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional version that is pending activation after restart.</summary>
    public string? PendingVersion { get; set; }

    /// <summary>Gets or sets the diagnostic message associated with the current failed state.</summary>
    public string? LastError { get; set; }

    /// <summary>Gets or sets the UTC timestamp at which the plugin was installed.</summary>
    public DateTimeOffset InstalledAt { get; set; }

    /// <summary>Gets or sets the UTC timestamp of the most recent lifecycle state transition.</summary>
    public DateTimeOffset UpdatedAt { get; set; }
}

using RemoteCommerce.Plugins;

namespace RemoteCommerce.Infrastructure.Persistence.Entities;

/// <summary>
/// Represents a plugin package installed in the RemoteCommerce host.
/// </summary>
public sealed class PluginInstallation
{
    /// <summary>
    /// Gets or sets the persistent identifier of the installation record.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the stable plugin identifier declared by the manifest.
    /// </summary>
    public string PluginId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the installed plugin version.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the absolute or application-relative plugin package path.
    /// </summary>
    public string PackagePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the serialized plugin installation state.
    /// </summary>
    public PluginInstallationState State { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp at which the plugin was installed.
    /// </summary>
    public DateTimeOffset InstalledAt { get; set; }
}

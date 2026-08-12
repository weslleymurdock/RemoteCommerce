using RemoteCommerce.Plugins;

namespace RemoteCommerce.Infrastructure.Persistence.Entities;

/// <summary>
/// Represents a plugin package installed in the RemoteCommerce host.
/// </summary>
public sealed class PluginInstallation
{
    /// <summary>Gets or sets the persistent identifier of the installation record.</summary>
    public Guid Id { get; set; }
    /// <summary>Gets or sets the stable plugin identifier declared by the manifest.</summary>
    public string PluginId { get; set; } = string.Empty;
    /// <summary>Gets or sets the installed plugin version.</summary>
    public string Version { get; set; } = string.Empty;
    /// <summary>Gets or sets the absolute or application-relative plugin package path.</summary>
    public string PackagePath { get; set; } = string.Empty;
    /// <summary>Gets or sets the serialized plugin installation state.</summary>
    public PluginInstallationState State { get; set; }
    /// <summary>Gets or sets the UTC timestamp at which the plugin was installed.</summary>
    public DateTimeOffset InstalledAt { get; set; }
    /// <summary>Gets or sets the human-readable plugin name.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Gets or sets the plugin description.</summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>Gets or sets the NuGet package identifier.</summary>
    public string PackageId { get; set; } = string.Empty;
    /// <summary>Gets or sets the NuGet package tags.</summary>
    public string PackageTags { get; set; } = string.Empty;
    /// <summary>Gets or sets the package title.</summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>Gets or sets the package authors.</summary>
    public string Authors { get; set; } = string.Empty;
    /// <summary>Gets or sets the package company or publisher.</summary>
    public string Company { get; set; } = string.Empty;
    /// <summary>Gets or sets the source repository URL.</summary>
    public string RepositoryUrl { get; set; } = string.Empty;
    /// <summary>Gets or sets the source repository type.</summary>
    public string RepositoryType { get; set; } = string.Empty;
    /// <summary>Gets or sets the project homepage URL.</summary>
    public string PackageProjectUrl { get; set; } = string.Empty;
    /// <summary>Gets or sets whether the package requires license acceptance.</summary>
    public bool PackageRequireLicenseAcceptance { get; set; }
}

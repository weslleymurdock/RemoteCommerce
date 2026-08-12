namespace RemoteCommerce.Plugins;

/// <summary>
/// Defines the persisted lifecycle states of a RemoteCommerce plugin installation.
/// </summary>
public enum PluginInstallationState
{
    /// <summary>The package has been discovered but has not yet completed validation.</summary>
    Discovered = 0,

    /// <summary>The package passed validation and is ready for installation.</summary>
    Validated = 1,

    /// <summary>The validated package is installed and has persistent administrative state.</summary>
    Installed = 2,

    /// <summary>The plugin is administratively enabled and is eligible for startup activation.</summary>
    Enabled = 3,

    /// <summary>The plugin is administratively disabled and will not be activated at startup.</summary>
    Disabled = 4,

    /// <summary>The plugin was successfully loaded and registered during the current startup.</summary>
    Loaded = 5,

    /// <summary>A lifecycle change has been persisted and requires a process restart before activation changes can apply.</summary>
    ActivationPending = 6,

    /// <summary>The plugin could not be activated or otherwise failed a lifecycle operation.</summary>
    Failed = 7
}

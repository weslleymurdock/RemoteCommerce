namespace RemoteCommerce.Plugins;

/// <summary>
/// Defines the lifecycle states of an installed RemoteCommerce plugin.
/// </summary>
public enum PluginInstallationState
{
    /// <summary>
    /// The plugin package has been accepted but is not yet active.
    /// </summary>
    Installed = 0,

    /// <summary>
    /// The plugin is disabled and must not be loaded during startup.
    /// </summary>
    Disabled = 1
}

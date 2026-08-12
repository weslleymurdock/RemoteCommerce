namespace RemoteCommerce.Plugins;

/// <summary>Defines the administrative state that should be applied to a plugin after the next host startup.</summary>
public enum PluginDesiredState
{
    /// <summary>The plugin should be enabled and activated when compatible.</summary>
    Enabled = 0,

    /// <summary>The plugin should remain disabled and must not be activated.</summary>
    Disabled = 1
}

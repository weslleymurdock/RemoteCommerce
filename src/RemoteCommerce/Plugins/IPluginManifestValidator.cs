using RemoteCommerce.Plugins.Abstractions;

namespace RemoteCommerce.Plugins;

/// <summary>Validates the semantic and structural rules of a RemoteCommerce plugin manifest.</summary>
public interface IPluginManifestValidator
{
    /// <summary>Validates a manifest without loading or executing the plugin assembly.</summary>
    /// <param name="manifest">The manifest to validate.</param>
    /// <returns>The validation issues discovered in the manifest.</returns>
    IReadOnlyList<PluginValidationIssue> Validate(PluginManifest manifest);
}

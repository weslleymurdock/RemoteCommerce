using RemoteCommerce.Plugins.Abstractions;

namespace RemoteCommerce.Plugins;

/// <summary>Validates plugin compatibility against the current RemoteCommerce host.</summary>
public interface IPluginCompatibilityValidator
{
    /// <summary>Validates host, target framework, and EF Core compatibility requirements declared by a plugin.</summary>
    /// <param name="manifest">The plugin manifest to validate.</param>
    /// <returns>The compatibility issues discovered in the manifest.</returns>
    IReadOnlyList<PluginValidationIssue> Validate(PluginManifest manifest);
}

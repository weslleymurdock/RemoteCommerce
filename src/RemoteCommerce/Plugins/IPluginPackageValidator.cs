namespace RemoteCommerce.Plugins;

/// <summary>Validates a plugin package without installing or activating its code.</summary>
public interface IPluginPackageValidator
{
    /// <summary>Validates package structure, manifest, compatibility, documentation, and assembly metadata.</summary>
    /// <param name="packagePath">The path to the candidate <c>.nupkg</c> file.</param>
    /// <param name="cancellationToken">The token used to cancel package inspection.</param>
    /// <returns>The validation result, including the manifest and package integrity hash when successful.</returns>
    Task<PluginPackageValidationResult> ValidateAsync(string packagePath, CancellationToken cancellationToken = default);
}

/// <summary>Contains the result of validating a plugin package.</summary>
/// <param name="Manifest">The parsed plugin manifest, when available.</param>
/// <param name="PackageHash">The SHA-256 hash of the original package file.</param>
/// <param name="Issues">The validation diagnostics.</param>
public sealed record PluginPackageValidationResult(
    RemoteCommerce.Plugins.Abstractions.PluginManifest? Manifest,
    string PackageHash,
    IReadOnlyList<PluginValidationIssue> Issues)
{
    /// <summary>Gets whether the package contains no blocking validation errors.</summary>
    public bool IsValid => Issues.All(x => x.Severity != PluginValidationSeverity.Error);
}

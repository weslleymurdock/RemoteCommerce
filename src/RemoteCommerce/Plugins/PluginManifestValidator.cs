using RemoteCommerce.Plugins.Abstractions;

namespace RemoteCommerce.Plugins;

/// <summary>Provides deterministic validation for plugin manifest metadata.</summary>
public sealed class PluginManifestValidator : IPluginManifestValidator
{
    /// <inheritdoc />
    public IReadOnlyList<PluginValidationIssue> Validate(PluginManifest manifest)
    {
        var issues = new List<PluginValidationIssue>();
        AddRequired(issues, nameof(manifest.Id), manifest.Id);
        AddRequired(issues, nameof(manifest.Name), manifest.Name);
        AddRequired(issues, nameof(manifest.Version), manifest.Version);
        AddRequired(issues, nameof(manifest.Description), manifest.Description);
        AddRequired(issues, nameof(manifest.Authors), manifest.Authors);
        AddRequired(issues, nameof(manifest.EntryAssembly), manifest.EntryAssembly);
        AddRequired(issues, nameof(manifest.EntryType), manifest.EntryType);
        AddRequired(issues, nameof(manifest.MinHostVersion), manifest.MinHostVersion);
        AddRequired(issues, nameof(manifest.PackageId), manifest.PackageId);
        AddRequired(issues, nameof(manifest.License), manifest.License);
        AddRequired(issues, nameof(manifest.Readme), manifest.Readme);

        if (!string.IsNullOrWhiteSpace(manifest.Id) && !IsSafeIdentifier(manifest.Id))
            issues.Add(new("PLUGIN_ID_INVALID", "The plugin id is not a safe package identifier.", PluginValidationSeverity.Error));
        if (!Version.TryParse(manifest.Version, out _))
            issues.Add(new("PLUGIN_VERSION_INVALID", "The plugin version must be a valid System.Version value.", PluginValidationSeverity.Error));
        if (!Version.TryParse(manifest.MinHostVersion, out _))
            issues.Add(new("HOST_VERSION_INVALID", "MinHostVersion must be a valid System.Version value.", PluginValidationSeverity.Error));
        if (!string.Equals(manifest.License, "LICENSE.md", StringComparison.OrdinalIgnoreCase))
            issues.Add(new("LICENSE_PATH_INVALID", "The manifest license path must be LICENSE.md at the package root.", PluginValidationSeverity.Error));
        if (!string.Equals(manifest.Readme, "README.md", StringComparison.OrdinalIgnoreCase))
            issues.Add(new("README_PATH_INVALID", "The manifest readme path must be README.md at the package root.", PluginValidationSeverity.Error));

        ValidateRelativePath(issues, manifest.EntryAssembly, "ENTRY_ASSEMBLY_PATH_INVALID");
        var normalizedAssemblyPath = manifest.EntryAssembly.Replace('\\', '/');
        if (!string.IsNullOrWhiteSpace(manifest.EntryAssembly) && !normalizedAssemblyPath.StartsWith("lib/net10.0/", StringComparison.OrdinalIgnoreCase))
            issues.Add(new("ENTRY_ASSEMBLY_TARGET_INVALID", "The plugin entry assembly must be located under lib/net10.0/.", PluginValidationSeverity.Error));
        if (string.IsNullOrWhiteSpace(manifest.EntryType) || !manifest.EntryType.Contains('.', StringComparison.Ordinal))
            issues.Add(new("ENTRY_TYPE_INVALID", "EntryType must be a fully qualified type name.", PluginValidationSeverity.Error));

        var dependencies = manifest.DependencyDeclarations;
        foreach (var duplicate in dependencies.Where(x => !string.IsNullOrWhiteSpace(x.PluginId)).GroupBy(x => x.PluginId, StringComparer.OrdinalIgnoreCase).Where(x => x.Count() > 1).Select(x => x.Key))
            issues.Add(new("DEPENDENCY_DUPLICATE", $"Plugin dependency '{duplicate}' is declared more than once.", PluginValidationSeverity.Error));

        foreach (var dependency in dependencies)
        {
            if (string.IsNullOrWhiteSpace(dependency.PluginId))
                issues.Add(new("DEPENDENCY_ID_REQUIRED", "Every plugin dependency must declare a plugin id.", PluginValidationSeverity.Error));
            if (!Version.TryParse(dependency.MinimumVersion, out _))
                issues.Add(new("DEPENDENCY_VERSION_INVALID", $"Dependency '{dependency.PluginId}' has an invalid minimum version.", PluginValidationSeverity.Error));
            if (dependency.MaximumVersion is not null && !Version.TryParse(dependency.MaximumVersion, out _))
                issues.Add(new("DEPENDENCY_VERSION_INVALID", $"Dependency '{dependency.PluginId}' has an invalid maximum version.", PluginValidationSeverity.Error));
        }

        if (!string.IsNullOrWhiteSpace(manifest.EfCoreVersion) && !Version.TryParse(manifest.EfCoreVersion, out _))
            issues.Add(new("EF_VERSION_INVALID", "EfCoreVersion must be a valid version when declared.", PluginValidationSeverity.Error));

        return issues;
    }

    private static void AddRequired(List<PluginValidationIssue> issues, string name, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            issues.Add(new("MANIFEST_REQUIRED", $"Manifest property '{name}' is required.", PluginValidationSeverity.Error));
    }

    private static void ValidateRelativePath(List<PluginValidationIssue> issues, string path, string code)
    {
        if (string.IsNullOrWhiteSpace(path) || Path.IsPathRooted(path) || path.Contains("..", StringComparison.Ordinal) || path.StartsWith("/", StringComparison.Ordinal) || path.StartsWith("\\", StringComparison.Ordinal))
            issues.Add(new(code, $"Package path '{path}' is not a safe relative path.", PluginValidationSeverity.Error));
    }

    private static bool IsSafeIdentifier(string value)
        => !value.Contains("..", StringComparison.Ordinal)
            && value.IndexOfAny(Path.GetInvalidFileNameChars()) < 0
            && !value.Contains('/', StringComparison.Ordinal)
            && !value.Contains('\\', StringComparison.Ordinal);
}

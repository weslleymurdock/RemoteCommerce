using System.Reflection;
using Microsoft.EntityFrameworkCore;
using RemoteCommerce.Plugins.Abstractions;

namespace RemoteCommerce.Plugins;

/// <summary>Validates plugin compatibility against the versions used by the running host.</summary>
public sealed class PluginCompatibilityValidator : IPluginCompatibilityValidator
{
    /// <inheritdoc />
    public IReadOnlyList<PluginValidationIssue> Validate(PluginManifest manifest)
    {
        var issues = new List<PluginValidationIssue>();
        if (!Version.TryParse(manifest.MinHostVersion, out var minimumHostVersion))
            return issues;

        var hostVersion = Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(0, 0);
        if (hostVersion < minimumHostVersion)
            issues.Add(new("HOST_VERSION_INCOMPATIBLE", $"Plugin requires RemoteCommerce {minimumHostVersion} or newer, but the current host is {hostVersion}.", PluginValidationSeverity.Error));

        if (!string.IsNullOrWhiteSpace(manifest.EfCoreVersion) && Version.TryParse(manifest.EfCoreVersion, out var requiredEf))
        {
            var hostEf = typeof(DbContext).Assembly.GetName().Version ?? new Version(0, 0);
            if (hostEf.Major != requiredEf.Major || hostEf.Minor != requiredEf.Minor)
                issues.Add(new("EF_VERSION_INCOMPATIBLE", $"Plugin requires EF Core {requiredEf.Major}.{requiredEf.Minor}, but the host uses {hostEf.Major}.{hostEf.Minor}.", PluginValidationSeverity.Error));
        }

        return issues;
    }
}

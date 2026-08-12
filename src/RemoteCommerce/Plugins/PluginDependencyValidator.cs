using Microsoft.EntityFrameworkCore;
using RemoteCommerce.Infrastructure.Persistence;
using RemoteCommerce.Infrastructure.Persistence.Entities;
using RemoteCommerce.Plugins.Abstractions;

namespace RemoteCommerce.Plugins;

/// <summary>Validates plugin dependency declarations against the installed plugin graph.</summary>
/// <param name="dbFactory">The factory used to inspect persisted plugin installations and dependencies.</param>
public sealed class PluginDependencyValidator(IDbContextFactory<CommerceDbContext> dbFactory)
{
    /// <summary>Validates dependencies for a candidate plugin package.</summary>
    /// <param name="manifest">The candidate plugin manifest.</param>
    /// <param name="cancellationToken">The token used to cancel the database query.</param>
    /// <returns>The dependency validation issues.</returns>
    public async Task<IReadOnlyList<PluginValidationIssue>> ValidateAsync(PluginManifest manifest, CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var installations = await db.PluginInstallations.AsNoTracking().ToListAsync(cancellationToken);
        var dependencies = await db.PluginDependencies.AsNoTracking().ToListAsync(cancellationToken);
        var byId = installations.ToDictionary(x => x.PluginId, StringComparer.OrdinalIgnoreCase);
        var issues = new List<PluginValidationIssue>();

        foreach (var dependency in manifest.DependencyDeclarations)
        {
            if (!byId.TryGetValue(dependency.PluginId, out var installed))
            {
                issues.Add(new("DEPENDENCY_MISSING", $"Plugin '{manifest.Id}' requires plugin '{dependency.PluginId}', which is not installed.", PluginValidationSeverity.Error));
                continue;
            }

            if (installed.DesiredState == PluginDesiredState.Disabled || installed.State == PluginInstallationState.Disabled)
                issues.Add(new("DEPENDENCY_DISABLED", $"Plugin '{manifest.Id}' requires disabled plugin '{dependency.PluginId}'.", PluginValidationSeverity.Error));

            if (!Version.TryParse(installed.Version, out var installedVersion) || !Version.TryParse(dependency.MinimumVersion, out var minimumVersion))
            {
                issues.Add(new("DEPENDENCY_VERSION_INVALID", $"The installed version for dependency '{dependency.PluginId}' cannot be evaluated.", PluginValidationSeverity.Error));
                continue;
            }

            if (installedVersion < minimumVersion || dependency.MaximumVersion is not null && Version.TryParse(dependency.MaximumVersion, out var maximumVersion) && installedVersion >= maximumVersion)
                issues.Add(new("DEPENDENCY_INCOMPATIBLE", $"Plugin '{manifest.Id}' requires '{dependency.PluginId}' in the declared version range, but {installed.Version} is installed.", PluginValidationSeverity.Error));
        }

        var graph = BuildGraph(installations, dependencies, manifest);
        if (HasCycle(graph, manifest.Id))
            issues.Add(new("DEPENDENCY_CYCLE", $"Plugin '{manifest.Id}' introduces a circular dependency.", PluginValidationSeverity.Error));

        return issues;
    }

    private static Dictionary<string, List<string>> BuildGraph(IEnumerable<PluginInstallation> installations, IEnumerable<PluginDependency> dependencies, PluginManifest candidate)
    {
        var graph = installations.ToDictionary(x => x.PluginId, _ => new List<string>(), StringComparer.OrdinalIgnoreCase);
        foreach (var dependency in dependencies)
        {
            if (!graph.TryGetValue(dependency.PluginId, out var edges))
                graph[dependency.PluginId] = edges = [];
            edges.Add(dependency.DependencyPluginId);
        }

        graph[candidate.Id] = candidate.DependencyDeclarations.Select(x => x.PluginId).ToList();
        return graph;
    }

    private static bool HasCycle(Dictionary<string, List<string>> graph, string root)
    {
        var visiting = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        bool Visit(string node)
        {
            if (!visiting.Add(node)) return true;
            if (visited.Contains(node))
            {
                visiting.Remove(node);
                return false;
            }

            if (graph.TryGetValue(node, out var children))
                foreach (var child in children)
                    if (Visit(child)) return true;

            visiting.Remove(node);
            visited.Add(node);
            return false;
        }

        return Visit(root);
    }
}

namespace RemoteCommerce.Plugins.Abstractions;

/// <summary>
/// Describes a RemoteCommerce NuGet plugin package and its host compatibility requirements.
/// </summary>
/// <param name="Id">The stable unique identifier of the plugin.</param>
/// <param name="Name">The human-readable plugin name.</param>
/// <param name="Version">The semantic version of the plugin package.</param>
/// <param name="EntryAssembly">The package-relative path to the plugin entry assembly.</param>
/// <param name="EntryType">The fully qualified type implementing <see cref="IRemoteCommercePlugin"/>.</param>
/// <param name="MinHostVersion">The minimum RemoteCommerce host version supported by the plugin.</param>
/// <param name="Description">An optional human-readable plugin description.</param>
public sealed record PluginManifest(
    string Id,
    string Name,
    string Version,
    string EntryAssembly,
    string EntryType,
    string MinHostVersion,
    string? Description = null);

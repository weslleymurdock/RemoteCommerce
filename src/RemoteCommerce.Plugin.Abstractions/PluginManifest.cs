namespace RemoteCommerce.Plugins.Abstractions;

/// <summary>
/// Describes a RemoteCommerce NuGet plugin package, its metadata, dependencies, and host compatibility requirements.
/// </summary>
/// <param name="Id">The stable unique identifier of the plugin.</param>
/// <param name="Name">The human-readable plugin name.</param>
/// <param name="License">The required root package license file name.</param>
/// <param name="Readme">The required root package readme file name.</param>
/// <param name="Version">The semantic version of the plugin package.</param>
/// <param name="EntryAssembly">The package-relative path to the plugin entry assembly.</param>
/// <param name="EntryType">The fully qualified type implementing <see cref="IRemoteCommercePlugin"/>.</param>
/// <param name="MinHostVersion">The minimum RemoteCommerce host version supported by the plugin.</param>
/// <param name="Description">The NuGet package description.</param>
/// <param name="PackageId">The NuGet package identifier.</param>
/// <param name="PackageTags">The NuGet package tags separated by semicolons.</param>
/// <param name="Title">The NuGet package title.</param>
/// <param name="Authors">The NuGet package authors.</param>
/// <param name="Company">The NuGet package company or publisher.</param>
/// <param name="RepositoryUrl">The source repository URL.</param>
/// <param name="RepositoryType">The source repository type, such as git.</param>
/// <param name="PackageRequireLicenseAcceptance">Indicates whether the package requires license acceptance.</param>
/// <param name="PackageProjectUrl">The project homepage URL.</param>
/// <param name="Dependencies">The plugins that must be installed and loadable before this plugin can be activated.</param>
/// <param name="EfCoreVersion">The optional EF Core major/minor version required by the plugin persistence boundary.</param>
public sealed record PluginManifest(
    string Id,
    string Name,
    string License,
    string Readme,
    string Version,
    string EntryAssembly,
    string EntryType,
    string MinHostVersion,
    string Description,
    string PackageId,
    string PackageTags,
    string Title,
    string Authors,
    string Company,
    string RepositoryUrl,
    string RepositoryType,
    bool PackageRequireLicenseAcceptance,
    string PackageProjectUrl,
    IReadOnlyList<PluginDependencyDeclaration>? Dependencies = null,
    string? EfCoreVersion = null)
{
    /// <summary>Gets the declared plugin dependencies, or an empty collection when the package has none.</summary>
    /// <remarks>The collection is normalized to an empty array so callers do not need null checks.</remarks>
    [JsonIgnore]
    public IReadOnlyList<PluginDependencyDeclaration> DependencyDeclarations => Dependencies ?? [];
}

/// <summary>Declares a dependency on another RemoteCommerce plugin.</summary>
/// <param name="PluginId">The stable identifier of the required plugin.</param>
/// <param name="MinimumVersion">The minimum compatible version of the required plugin.</param>
/// <param name="MaximumVersion">The optional exclusive upper version boundary.</param>
public sealed record PluginDependencyDeclaration(
    string PluginId,
    string MinimumVersion,
    string? MaximumVersion = null);

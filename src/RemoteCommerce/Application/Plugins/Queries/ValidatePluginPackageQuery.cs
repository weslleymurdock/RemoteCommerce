namespace RemoteCommerce.Application.Plugins.Queries;

/// <summary>Validates a plugin package without installing or activating it.</summary>
/// <param name="PackagePath">The temporary package path.</param>
public sealed record ValidatePluginPackageQuery(string PackagePath) : IQuery<PluginPackageValidationResult>;

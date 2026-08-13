namespace RemoteCommerce.Application.Plugins.Commands;

/// <summary>Installs a validated plugin package.</summary>
/// <param name="PackagePath">The temporary package path.</param>
public sealed record InstallPluginCommand(string PackagePath) : ICommand<PluginManifest>, ITransactionalCommand;
/// <summary>Updates an installed plugin with a validated newer package.</summary>
/// <param name="PluginId">The installed plugin identifier.</param><param name="PackagePath">The temporary package path.</param>
public sealed record UpdatePluginCommand(string PluginId, string PackagePath) : ICommand<PluginManifest>, ITransactionalCommand;
/// <summary>Requests plugin enablement after application restart.</summary>
/// <param name="PluginId">The installed plugin identifier.</param>
public sealed record EnablePluginCommand(string PluginId) : ICommand<Unit>, ITransactionalCommand;
/// <summary>Requests plugin disablement after application restart.</summary>
/// <param name="PluginId">The installed plugin identifier.</param>
public sealed record DisablePluginCommand(string PluginId) : ICommand<Unit>, ITransactionalCommand;
/// <summary>Requests plugin uninstallation while preserving database history.</summary>
/// <param name="PluginId">The installed plugin identifier.</param>
public sealed record UninstallPluginCommand(string PluginId) : ICommand<Unit>, ITransactionalCommand;
/// <summary>Requests rollback to a retained plugin version.</summary>
/// <param name="PluginId">The installed plugin identifier.</param><param name="Version">The retained version.</param>
public sealed record RollbackPluginCommand(string PluginId, string Version) : ICommand<Unit>, ITransactionalCommand;

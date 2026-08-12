namespace RemoteCommerce.Application.Plugins;

/// <summary>Validates a plugin package without installing or activating it.</summary>
/// <param name="PackagePath">The temporary package path.</param>
public sealed record ValidatePluginPackageQuery(string PackagePath) : IQuery<PluginPackageValidationResult>;

/// <summary>Installs a validated plugin package.</summary>
/// <param name="PackagePath">The temporary package path.</param>
public sealed record InstallPluginCommand(string PackagePath) : ICommand<PluginManifest>, ITransactionalCommand;

/// <summary>Updates an installed plugin with a validated newer package.</summary>
/// <param name="PluginId">The installed plugin identifier.</param>
/// <param name="PackagePath">The temporary package path.</param>
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
/// <param name="PluginId">The installed plugin identifier.</param>
/// <param name="Version">The retained version.</param>
public sealed record RollbackPluginCommand(string PluginId, string Version) : ICommand<Unit>, ITransactionalCommand;

/// <summary>Handles plugin package validation.</summary>
/// <param name="installationService">The plugin installation service.</param>
public sealed class ValidatePluginPackageQueryHandler(PluginInstallationService installationService) : IRequestHandler<ValidatePluginPackageQuery, PluginPackageValidationResult>
{
    /// <summary>Validates the supplied package.</summary>
    /// <param name="request">The package validation query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The package validation result.</returns>
    public Task<PluginPackageValidationResult> Handle(ValidatePluginPackageQuery request, CancellationToken cancellationToken)
        => installationService.ValidatePackageAsync(request.PackagePath, cancellationToken);
}

/// <summary>Handles plugin installation.</summary>
/// <param name="installationService">The plugin installation service.</param>
public sealed class InstallPluginCommandHandler(PluginInstallationService installationService) : IRequestHandler<InstallPluginCommand, PluginManifest>
{
    /// <summary>Installs the package and leaves activation pending restart.</summary>
    /// <param name="request">The installation command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The installed plugin manifest.</returns>
    public Task<PluginManifest> Handle(InstallPluginCommand request, CancellationToken cancellationToken)
        => installationService.InstallAsync(request.PackagePath, cancellationToken);
}

/// <summary>Handles plugin updates.</summary>
/// <param name="installationService">The plugin installation service.</param>
public sealed class UpdatePluginCommandHandler(PluginInstallationService installationService) : IRequestHandler<UpdatePluginCommand, PluginManifest>
{
    /// <summary>Installs the newer package while retaining the previous version.</summary>
    /// <param name="request">The update command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated plugin manifest.</returns>
    public Task<PluginManifest> Handle(UpdatePluginCommand request, CancellationToken cancellationToken)
        => installationService.UpdateAsync(request.PluginId, request.PackagePath, cancellationToken);
}

/// <summary>Handles plugin enablement.</summary>
/// <param name="managementService">The plugin management service.</param>
public sealed class EnablePluginCommandHandler(PluginManagementService managementService) : IRequestHandler<EnablePluginCommand, Unit>
{
    /// <summary>Persists the desired enabled state.</summary>
    /// <param name="request">The enable command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A completed mediator unit value.</returns>
    public async Task<Unit> Handle(EnablePluginCommand request, CancellationToken cancellationToken)
    {
        await managementService.EnableAsync(request.PluginId, cancellationToken);
        return Unit.Value;
    }
}

/// <summary>Handles plugin disablement.</summary>
/// <param name="managementService">The plugin management service.</param>
public sealed class DisablePluginCommandHandler(PluginManagementService managementService) : IRequestHandler<DisablePluginCommand, Unit>
{
    /// <summary>Persists the desired disabled state.</summary>
    /// <param name="request">The disable command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A completed mediator unit value.</returns>
    public async Task<Unit> Handle(DisablePluginCommand request, CancellationToken cancellationToken)
    {
        await managementService.DisableAsync(request.PluginId, cancellationToken);
        return Unit.Value;
    }
}

/// <summary>Handles plugin uninstallation.</summary>
/// <param name="managementService">The plugin management service.</param>
public sealed class UninstallPluginCommandHandler(PluginManagementService managementService) : IRequestHandler<UninstallPluginCommand, Unit>
{
    /// <summary>Soft-deletes plugin administration records and schedules package removal.</summary>
    /// <param name="request">The uninstall command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A completed mediator unit value.</returns>
    public async Task<Unit> Handle(UninstallPluginCommand request, CancellationToken cancellationToken)
    {
        await managementService.UninstallAsync(request.PluginId, cancellationToken);
        return Unit.Value;
    }
}

/// <summary>Handles plugin rollback.</summary>
/// <param name="managementService">The plugin management service.</param>
public sealed class RollbackPluginCommandHandler(PluginManagementService managementService) : IRequestHandler<RollbackPluginCommand, Unit>
{
    /// <summary>Restores a retained plugin version for activation after restart.</summary>
    /// <param name="request">The rollback command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A completed mediator unit value.</returns>
    public async Task<Unit> Handle(RollbackPluginCommand request, CancellationToken cancellationToken)
    {
        await managementService.RollbackAsync(request.PluginId, request.Version, cancellationToken);
        return Unit.Value;
    }
}

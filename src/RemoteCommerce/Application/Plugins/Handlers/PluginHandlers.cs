namespace RemoteCommerce.Application.Plugins.Handlers;

/// <summary>Handles plugin package validation.</summary>
public sealed class ValidatePluginPackageQueryHandler(PluginInstallationService installationService) : IRequestHandler<ValidatePluginPackageQuery, PluginPackageValidationResult>
{
    /// <inheritdoc />
    public Task<PluginPackageValidationResult> Handle(ValidatePluginPackageQuery request, CancellationToken cancellationToken) => installationService.ValidatePackageAsync(request.PackagePath, cancellationToken);
}

/// <summary>Handles plugin installation.</summary>
public sealed class InstallPluginCommandHandler(PluginInstallationService installationService) : IRequestHandler<InstallPluginCommand, PluginManifest>
{
    /// <inheritdoc />
    public Task<PluginManifest> Handle(InstallPluginCommand request, CancellationToken cancellationToken) => installationService.InstallAsync(request.PackagePath, cancellationToken);
}

/// <summary>Handles plugin updates.</summary>
public sealed class UpdatePluginCommandHandler(PluginInstallationService installationService) : IRequestHandler<UpdatePluginCommand, PluginManifest>
{
    /// <inheritdoc />
    public Task<PluginManifest> Handle(UpdatePluginCommand request, CancellationToken cancellationToken) => installationService.UpdateAsync(request.PluginId, request.PackagePath, cancellationToken);
}

/// <summary>Handles plugin enablement.</summary>
public sealed class EnablePluginCommandHandler(PluginManagementService managementService) : IRequestHandler<EnablePluginCommand, Unit>
{
    /// <inheritdoc />
    public async Task<Unit> Handle(EnablePluginCommand request, CancellationToken cancellationToken) { await managementService.EnableAsync(request.PluginId, cancellationToken); return Unit.Value; }
}

/// <summary>Handles plugin disablement.</summary>
public sealed class DisablePluginCommandHandler(PluginManagementService managementService) : IRequestHandler<DisablePluginCommand, Unit>
{
    /// <inheritdoc />
    public async Task<Unit> Handle(DisablePluginCommand request, CancellationToken cancellationToken) { await managementService.DisableAsync(request.PluginId, cancellationToken); return Unit.Value; }
}

/// <summary>Handles plugin uninstallation.</summary>
public sealed class UninstallPluginCommandHandler(PluginManagementService managementService) : IRequestHandler<UninstallPluginCommand, Unit>
{
    /// <inheritdoc />
    public async Task<Unit> Handle(UninstallPluginCommand request, CancellationToken cancellationToken) { await managementService.UninstallAsync(request.PluginId, cancellationToken); return Unit.Value; }
}

/// <summary>Handles plugin rollback.</summary>
public sealed class RollbackPluginCommandHandler(PluginManagementService managementService) : IRequestHandler<RollbackPluginCommand, Unit>
{
    /// <inheritdoc />
    public async Task<Unit> Handle(RollbackPluginCommand request, CancellationToken cancellationToken) { await managementService.RollbackAsync(request.PluginId, request.Version, cancellationToken); return Unit.Value; }
}

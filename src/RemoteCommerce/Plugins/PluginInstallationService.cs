using Microsoft.EntityFrameworkCore;
using RemoteCommerce.Infrastructure.Persistence;
using RemoteCommerce.Infrastructure.Persistence.Entities;
using RemoteCommerce.Plugins.Abstractions;

namespace RemoteCommerce.Plugins;

/// <summary>Coordinates transactional plugin installation and update operations.</summary>
/// <param name="dbFactory">The factory used to create persistence contexts.</param>
/// <param name="packageInstaller">The service responsible for validated package extraction.</param>
/// <param name="packageValidator">The service responsible for package validation before extraction.</param>
/// <param name="dependencyValidator">The validator responsible for installed dependency compatibility.</param>
/// <param name="packageSource">The explicitly trusted local package source.</param>
/// <param name="restartService">The service used to report that activation requires a restart.</param>
public sealed class PluginInstallationService(
    IDbContextFactory<CommerceDbContext> dbFactory,
    PluginPackageInstaller packageInstaller,
    IPluginPackageValidator packageValidator,
    PluginDependencyValidator dependencyValidator,
    IPluginPackageSource packageSource,
    IApplicationRestartService restartService)
{
    /// <summary>Validates, installs, and persists a plugin package as pending activation.</summary>
    /// <param name="packagePath">The path to a candidate <c>.nupkg</c> package.</param>
    /// <param name="cancellationToken">The token used to cancel installation.</param>
    /// <returns>The installed plugin manifest.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the package does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown when validation fails, dependencies are incompatible, or the plugin is already installed.</exception>
    public async Task<PluginManifest> InstallAsync(string packagePath, CancellationToken cancellationToken = default)
    {
        var validation = await ValidatePackageAsync(packagePath, cancellationToken);
        if (!validation.IsValid || validation.Manifest is null)
            throw CreateValidationException(validation.Issues);

        await EnsureDependenciesAsync(validation.Manifest, cancellationToken);
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        if (await db.PluginInstallations.AnyAsync(x => x.PluginId == validation.Manifest.Id, cancellationToken))
            throw new InvalidOperationException($"Plugin '{validation.Manifest.Id}' is already installed. Use update instead.");

        var versionDirectory = $"versions-{SanitizeVersion(validation.Manifest.Version)}-{Guid.NewGuid():N}";
        var installed = await packageInstaller.InstallAsync(packagePath, versionDirectory, cancellationToken);
        try
        {
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
            var now = DateTimeOffset.UtcNow;
            db.PluginInstallations.Add(new PluginInstallation
            {
                Id = Guid.NewGuid(), PluginId = installed.Manifest.Id, Version = installed.Manifest.Version,
                PackagePath = installed.TargetDirectory, PackageHash = installed.PackageHash,
                State = PluginInstallationState.ActivationPending, DesiredState = PluginDesiredState.Enabled,
                InstalledAt = now, UpdatedAt = now
            });
            db.PluginVersions.Add(new PluginVersion
            {
                Id = Guid.NewGuid(), PluginId = installed.Manifest.Id, Version = installed.Manifest.Version,
                PackagePath = installed.TargetDirectory, PackageHash = installed.PackageHash,
                InstalledAt = now, IsCurrent = true
            });
            foreach (var dependency in installed.Manifest.DependencyDeclarations)
                db.PluginDependencies.Add(new PluginDependency
                {
                    Id = Guid.NewGuid(), PluginId = installed.Manifest.Id,
                    DependencyPluginId = dependency.PluginId, MinimumVersion = dependency.MinimumVersion,
                    MaximumVersion = dependency.MaximumVersion
                });

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            if (Directory.Exists(installed.TargetDirectory)) Directory.Delete(installed.TargetDirectory, true);
            throw;
        }

        restartService.RequestRestart($"Plugin '{installed.Manifest.Id}' was installed and is pending activation.");
        return installed.Manifest;
    }

    /// <summary>Installs a package selected from the explicitly trusted local package source.</summary>
    /// <param name="fileName">The package file name as exposed by the trusted source.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The installed plugin manifest.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the package is not available from the trusted source.</exception>
    public async Task<PluginManifest> InstallFromTrustedSourceAsync(string fileName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        var packages = await packageSource.ListAsync(cancellationToken);
        var path = packages.SingleOrDefault(x => string.Equals(Path.GetFileName(x), fileName, StringComparison.OrdinalIgnoreCase));
        if (path is null) throw new FileNotFoundException("The requested package is not available from the trusted package source.", fileName);
        return await InstallAsync(path, cancellationToken);
    }

    /// <summary>Validates a candidate plugin package without installing or activating it.</summary>
    /// <param name="packagePath">The path to the candidate <c>.nupkg</c> package.</param>
    /// <param name="cancellationToken">The token used to cancel validation.</param>
    /// <returns>The package validation result.</returns>
    public Task<PluginPackageValidationResult> ValidatePackageAsync(string packagePath, CancellationToken cancellationToken = default)
        => packageValidator.ValidateAsync(packagePath, cancellationToken);

    /// <summary>Installs a validated newer plugin version while preserving the previous package for rollback.</summary>
    /// <param name="pluginId">The stable identifier of the installed plugin.</param>
    /// <param name="packagePath">The path to the candidate update package.</param>
    /// <param name="cancellationToken">The token used to cancel the update.</param>
    /// <returns>The updated plugin manifest.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the plugin is not installed.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the candidate version is not newer or validation fails.</exception>
    public async Task<PluginManifest> UpdateAsync(string pluginId, string packagePath, CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var installation = await db.PluginInstallations.SingleOrDefaultAsync(x => x.PluginId == pluginId, cancellationToken)
            ?? throw new KeyNotFoundException($"Plugin '{pluginId}' is not installed.");

        var validation = await ValidatePackageAsync(packagePath, cancellationToken);
        if (!validation.IsValid || validation.Manifest is null)
            throw CreateValidationException(validation.Issues);
        if (!string.Equals(validation.Manifest.Id, pluginId, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"The update package id '{validation.Manifest.Id}' does not match '{pluginId}'.");
        if (!Version.TryParse(validation.Manifest.Version, out var newVersion) || !Version.TryParse(installation.Version, out var currentVersion) || newVersion <= currentVersion)
            throw new InvalidOperationException($"The update version {validation.Manifest.Version} must be newer than {installation.Version}.");

        await EnsureDependenciesAsync(validation.Manifest, cancellationToken);
        var versionDirectory = $"versions-{SanitizeVersion(validation.Manifest.Version)}-{Guid.NewGuid():N}";
        var installed = await packageInstaller.InstallAsync(packagePath, versionDirectory, cancellationToken);
        try
        {
            var now = DateTimeOffset.UtcNow;
            installation.PendingVersion = installed.Manifest.Version;
            installation.PackagePath = installed.TargetDirectory;
            installation.Version = installed.Manifest.Version;
            installation.PackageHash = installed.PackageHash;
            installation.State = PluginInstallationState.ActivationPending;
            installation.UpdatedAt = now;

            foreach (var version in db.PluginVersions.Where(x => x.PluginId == pluginId)) version.IsCurrent = false;
            db.PluginVersions.Add(new PluginVersion
            {
                Id = Guid.NewGuid(), PluginId = pluginId, Version = installed.Manifest.Version,
                PackagePath = installed.TargetDirectory, PackageHash = installed.PackageHash,
                InstalledAt = now, IsCurrent = true
            });

            db.PluginDependencies.RemoveRange(db.PluginDependencies.Where(x => x.PluginId == pluginId));
            foreach (var dependency in installed.Manifest.DependencyDeclarations)
                db.PluginDependencies.Add(new PluginDependency
                {
                    Id = Guid.NewGuid(), PluginId = pluginId, DependencyPluginId = dependency.PluginId,
                    MinimumVersion = dependency.MinimumVersion, MaximumVersion = dependency.MaximumVersion
                });

            await db.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            if (Directory.Exists(installed.TargetDirectory)) Directory.Delete(installed.TargetDirectory, true);
            throw;
        }

        restartService.RequestRestart($"Plugin '{pluginId}' was updated to {installed.Manifest.Version} and requires restart.");
        return installed.Manifest;
    }

    private async Task EnsureDependenciesAsync(PluginManifest manifest, CancellationToken cancellationToken)
    {
        var issues = await dependencyValidator.ValidateAsync(manifest, cancellationToken);
        if (issues.Any(x => x.Severity == PluginValidationSeverity.Error))
            throw CreateValidationException(issues);
    }

    private static InvalidOperationException CreateValidationException(IEnumerable<PluginValidationIssue> issues)
        => new(string.Join(Environment.NewLine, issues.Where(x => x.Severity == PluginValidationSeverity.Error).Select(x => $"[{x.Code}] {x.Message}")));

    private static string SanitizeVersion(string version)
        => string.Concat(version.Select(character => char.IsLetterOrDigit(character) || character is '.' or '-' ? character : '_'));
}

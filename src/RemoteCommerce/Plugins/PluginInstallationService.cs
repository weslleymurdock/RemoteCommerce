using Microsoft.EntityFrameworkCore;
using RemoteCommerce.Infrastructure.Persistence;
using RemoteCommerce.Infrastructure.Persistence.Entities;
using RemoteCommerce.Plugins.Abstractions;

namespace RemoteCommerce.Plugins;

/// <summary>
/// Installs trusted plugin NuGet packages and persists their activation metadata for the next host restart.
/// </summary>
/// <param name="dbFactory">The factory used to create persistence contexts.</param>
/// <param name="packageInstaller">The service responsible for package validation and file installation.</param>
public sealed class PluginInstallationService(IDbContextFactory<CommerceDbContext> dbFactory, PluginPackageInstaller packageInstaller)
{
    /// <summary>
    /// Installs a plugin package and persists its active installation state.
    /// </summary>
    /// <param name="packagePath">The path to a trusted <c>.nupkg</c> package.</param>
    /// <param name="cancellationToken">The token used to cancel the installation operation.</param>
    /// <returns>The installed plugin manifest.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the package does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the package is invalid or the plugin is already installed.</exception>
    public async Task<PluginManifest> InstallAsync(string packagePath, CancellationToken cancellationToken = default)
    {
        var (manifest, targetDirectory) = await packageInstaller.InstallAsync(packagePath, cancellationToken);

        try
        {
            await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

            var existing = await db.PluginInstallations.SingleOrDefaultAsync(x => x.PluginId == manifest.Id, cancellationToken);
            if (existing is not null)
                throw new InvalidOperationException($"Plugin '{manifest.Id}' is already installed.");

            db.PluginInstallations.Add(new PluginInstallation
            {
                Id = Guid.NewGuid(),
                PluginId = manifest.Id,
                Version = manifest.Version,
                PackagePath = targetDirectory,
                State = PluginInstallationState.Installed,
                InstalledAt = DateTimeOffset.UtcNow
            });

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return manifest;
        }
        catch
        {
            if (Directory.Exists(targetDirectory))
                Directory.Delete(targetDirectory, recursive: true);
            throw;
        }
    }
}

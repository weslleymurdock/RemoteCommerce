namespace RemoteCommerce.Application.Persistence.Abstractions;

/// <summary>Coordinates administrator setup for database topologies that require replication initialization.</summary>
public interface IDatabaseSetupService
{
    /// <summary>Gets the current persisted setup state.</summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The current setup state.</returns>
    Task<DatabaseSetupState> GetStateAsync(CancellationToken cancellationToken = default);

    /// <summary>Validates and initializes the configured replication topology.</summary>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task representing the setup operation.</returns>
    Task ConfigureAsync(CancellationToken cancellationToken = default);
}

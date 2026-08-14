namespace RemoteCommerce.Application.Persistence.Handlers;

/// <summary>Handles database topology setup requests.</summary>
/// <param name="setupService">The database setup orchestration service.</param>
public sealed class DatabaseSetupHandlers(IDatabaseSetupService setupService)
{
    /// <summary>Handles the setup state query.</summary>
    /// <param name="request">The setup state query.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The current database setup state.</returns>
    public Task<DatabaseSetupState> Handle(
        GetDatabaseSetupStateQuery request,
        CancellationToken cancellationToken)
        => setupService.GetStateAsync(cancellationToken);

    /// <summary>Handles the database replication setup command.</summary>
    /// <param name="request">The setup command.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task that completes after setup succeeds.</returns>
    public async Task<Unit> Handle(
        ConfigureDatabaseReplicationCommand request,
        CancellationToken cancellationToken)
    {
        await setupService.ConfigureAsync(cancellationToken);
        return Unit.Value;
    }
}

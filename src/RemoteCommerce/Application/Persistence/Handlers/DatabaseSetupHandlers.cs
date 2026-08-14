namespace RemoteCommerce.Application.Persistence.Handlers;

/// <summary>Handles database topology setup state queries.</summary>
/// <param name="setupService">The database setup orchestration service.</param>
public sealed class GetDatabaseSetupStateQueryHandler(IDatabaseSetupService setupService)
    : IRequestHandler<GetDatabaseSetupStateQuery, DatabaseSetupState>
{
    /// <inheritdoc />
    public Task<DatabaseSetupState> Handle(
        GetDatabaseSetupStateQuery request,
        CancellationToken cancellationToken)
        => setupService.GetStateAsync(cancellationToken);
}

/// <summary>Handles database replication setup commands.</summary>
/// <param name="setupService">The database setup orchestration service.</param>
public sealed class ConfigureDatabaseReplicationCommandHandler(IDatabaseSetupService setupService)
    : IRequestHandler<ConfigureDatabaseReplicationCommand, Unit>
{
    /// <inheritdoc />
    public async Task<Unit> Handle(
        ConfigureDatabaseReplicationCommand request,
        CancellationToken cancellationToken)
    {
        await setupService.ConfigureAsync(cancellationToken);
        return Unit.Value;
    }
}

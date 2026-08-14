namespace RemoteCommerce.Infrastructure.Persistence.Services;

/// <summary>Coordinates provider validation and initialization for required database replication setup.</summary>
/// <param name="databaseProvider">The selected database provider.</param>
/// <param name="replicationProvider">The provider-aware replication strategy.</param>
/// <param name="stateStore">The non-secret setup state store.</param>
/// <param name="logger">The logger used for safe setup diagnostics.</param>
public sealed class DatabaseSetupService(
    RemoteCommerce.Application.Persistence.Abstractions.IDatabaseProvider databaseProvider,
    IDatabaseReplicationProvider replicationProvider,
    DatabaseSetupStateStore stateStore,
    ILogger<DatabaseSetupService> logger) : IDatabaseSetupService
{
    /// <inheritdoc />
    public async Task<DatabaseSetupState> GetStateAsync(CancellationToken cancellationToken = default)
    {
        if (!databaseProvider.RequiresSetup)
        {
            return DatabaseSetupState.NotRequired;
        }

        var persisted = await stateStore.ReadAsync(cancellationToken);
        return persisted?.State == DatabaseSetupState.Configured
            ? DatabaseSetupState.Configured
            : DatabaseSetupState.Required;
    }

    /// <inheritdoc />
    public async Task ConfigureAsync(CancellationToken cancellationToken = default)
    {
        if (!databaseProvider.RequiresSetup)
        {
            await stateStore.WriteAsync(
                new DatabaseSetupStateDocument(
                    DatabaseSetupState.NotRequired,
                    DateTimeOffset.UtcNow,
                    null),
                cancellationToken);
            return;
        }

        await stateStore.WriteAsync(
            new DatabaseSetupStateDocument(
                DatabaseSetupState.InProgress,
                DateTimeOffset.UtcNow,
                null),
            cancellationToken);

        try
        {
            if (!string.Equals(
                    databaseProvider.Name,
                    replicationProvider.ProviderName,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "The selected database provider does not match the replication provider.");
            }

            await databaseProvider.ValidateAsync(cancellationToken);
            await replicationProvider.ValidateAsync(cancellationToken);
            await replicationProvider.InitializeAsync(cancellationToken);

            await stateStore.WriteAsync(
                new DatabaseSetupStateDocument(
                    DatabaseSetupState.Configured,
                    DateTimeOffset.UtcNow,
                    null),
                cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(
                "Database setup failed. Exception type: {ExceptionType}",
                exception.GetType().FullName);

            await stateStore.WriteAsync(
                new DatabaseSetupStateDocument(
                    DatabaseSetupState.Failed,
                    DateTimeOffset.UtcNow,
                    "Database setup failed. Retry after correcting the deployment configuration."),
                CancellationToken.None);

            throw;
        }
    }
}

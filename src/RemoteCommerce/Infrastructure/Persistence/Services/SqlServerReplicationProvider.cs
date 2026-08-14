namespace RemoteCommerce.Infrastructure.Persistence.Services;

/// <summary>Validates the SQL Server primary/replica topology without coupling replication to EF Core entities.</summary>
/// <param name="databaseProvider">The selected SQL Server database provider.</param>
public sealed class SqlServerReplicationProvider(IDatabaseProvider databaseProvider) : IDatabaseReplicationProvider
{
    /// <inheritdoc />
    public string ProviderName => "SqlServer";

    /// <inheritdoc />
    public async Task ValidateAsync(CancellationToken cancellationToken = default)
    {
        if (databaseProvider.Topology != DatabaseTopology.PrimaryReplica)
        {
            throw new InvalidOperationException(
                "SQL Server replication validation requires a PrimaryReplica database topology.");
        }

        await using var primary = new SqlConnection(
            databaseProvider.GetConnectionString(DatabaseEndpoint.Primary));
        await primary.OpenAsync(cancellationToken);

        await using var replica = new SqlConnection(
            databaseProvider.GetConnectionString(DatabaseEndpoint.Replica));
        await replica.OpenAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (databaseProvider.Topology != DatabaseTopology.PrimaryReplica)
        {
            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}

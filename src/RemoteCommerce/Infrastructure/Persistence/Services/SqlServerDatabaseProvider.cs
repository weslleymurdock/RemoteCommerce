namespace RemoteCommerce.Infrastructure.Persistence.Services;

/// <summary>Provides the SQL Server implementation of the relational database strategy.</summary>
/// <param name="configuration">The deployment configuration source used for topology metadata.</param>
/// <param name="secretProvider">The deployment secret boundary used to resolve connection strings.</param>
public sealed class SqlServerDatabaseProvider(
    IConfiguration configuration,
    ISecretProvider secretProvider) : IDatabaseProvider
{
    private const string LocalDbConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=RemoteCommerce;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True";

    /// <inheritdoc />
    public string Name => "SqlServer";

    /// <inheritdoc />
    public DatabaseTopology Topology => ResolveTopology();

    /// <inheritdoc />
    public bool RequiresSetup => Topology == DatabaseTopology.PrimaryReplica;

    /// <inheritdoc />
    public string GetConnectionString(DatabaseEndpoint endpoint)
    {
        var connectionStrings = configuration.GetSection("ConnectionStrings").GetChildren().ToArray();
        if (connectionStrings.Length == 0)
        {
            return LocalDbConnectionString;
        }

        var connectionName = ResolveConnectionName(endpoint, connectionStrings);
        var value = secretProvider.Get($"ConnectionStrings:{connectionName}");

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"The SQL Server connection string '{connectionName}' is not configured.");
        }

        return value;
    }

    /// <inheritdoc />
    public void ConfigureDbContext(DbContextOptionsBuilder options, string? migrationsAssembly = null)
    {
        options.UseSqlServer(
            GetConnectionString(DatabaseEndpoint.Primary),
            sql =>
            {
                if (!string.IsNullOrWhiteSpace(migrationsAssembly))
                {
                    sql.MigrationsAssembly(migrationsAssembly);
                }
            });
    }

    /// <inheritdoc />
    public async Task ValidateAsync(CancellationToken cancellationToken = default)
    {
        _ = GetConnectionString(DatabaseEndpoint.Primary);

        if (Topology != DatabaseTopology.PrimaryReplica)
        {
            return;
        }

        _ = GetConnectionString(DatabaseEndpoint.Replica);

        await using var primary = new SqlConnection(GetConnectionString(DatabaseEndpoint.Primary));
        await primary.OpenAsync(cancellationToken);
        await primary.CloseAsync();

        await using var replica = new SqlConnection(GetConnectionString(DatabaseEndpoint.Replica));
        await replica.OpenAsync(cancellationToken);
    }

    private DatabaseTopology ResolveTopology()
    {
        var configured = configuration["Persistence:Database:Topology"];
        if (!string.IsNullOrWhiteSpace(configured))
        {
            if (Enum.TryParse<DatabaseTopology>(configured, true, out var topology))
            {
                return topology;
            }

            throw new InvalidOperationException(
                $"Unsupported database topology '{configured}'. Supported values are Single and PrimaryReplica.");
        }

        var connectionCount = configuration.GetSection("ConnectionStrings").GetChildren().Count();
        if (connectionCount <= 1)
        {
            return DatabaseTopology.Single;
        }

        throw new InvalidOperationException(
            "Multiple connection strings require an explicit Persistence:Database:Topology value.");
    }

    private string ResolveConnectionName(
        DatabaseEndpoint endpoint,
        IReadOnlyCollection<IConfigurationSection> connectionStrings)
    {
        if (Topology == DatabaseTopology.Single && connectionStrings.Count == 1)
        {
            return connectionStrings.Single().Key;
        }

        var configuredName = endpoint == DatabaseEndpoint.Primary
            ? configuration["Persistence:Database:PrimaryConnectionName"]
            : configuration["Persistence:Database:ReplicaConnectionName"];

        var connectionName = string.IsNullOrWhiteSpace(configuredName)
            ? endpoint == DatabaseEndpoint.Primary ? "Primary" : "Replica"
            : configuredName;

        if (connectionStrings.All(section => !string.Equals(section.Key, connectionName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                $"The configured SQL Server endpoint '{connectionName}' does not exist.");
        }

        return connectionName;
    }
}

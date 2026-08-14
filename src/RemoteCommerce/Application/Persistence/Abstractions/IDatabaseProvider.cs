namespace RemoteCommerce.Application.Persistence.Abstractions;

/// <summary>Identifies a database endpoint in the selected topology.</summary>
public enum DatabaseEndpoint
{
    /// <summary>The writable primary endpoint.</summary>
    Primary = 0,

    /// <summary>A read-only replica endpoint.</summary>
    Replica = 1
}

/// <summary>Provides the relational persistence strategy selected for the host.</summary>
public interface IDatabaseProvider
{
    /// <summary>Gets the stable provider identifier.</summary>
    string Name { get; }

    /// <summary>Gets the selected topology.</summary>
    DatabaseTopology Topology { get; }

    /// <summary>Gets whether the deployment requires provider setup before normal use.</summary>
    bool RequiresSetup { get; }

    /// <summary>Gets a deployment-managed connection string for an endpoint.</summary>
    /// <param name="endpoint">The endpoint to resolve.</param>
    /// <returns>The connection string for the requested endpoint.</returns>
    string GetConnectionString(DatabaseEndpoint endpoint);

    /// <summary>Validates the provider configuration and endpoint connectivity.</summary>
    /// <param name="cancellationToken">The token used to cancel validation.</param>
    /// <returns>A task representing the validation operation.</returns>
    Task ValidateAsync(CancellationToken cancellationToken = default);
}

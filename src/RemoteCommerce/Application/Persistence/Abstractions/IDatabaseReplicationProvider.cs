namespace RemoteCommerce.Application.Persistence.Abstractions;

/// <summary>Provides provider-aware replication operations for a database topology.</summary>
public interface IDatabaseReplicationProvider
{
    /// <summary>Gets the database provider identifier handled by this replication strategy.</summary>
    string ProviderName { get; }

    /// <summary>Validates the configured primary and replica endpoints.</summary>
    /// <param name="cancellationToken">The token used to cancel validation.</param>
    /// <returns>A task representing the validation operation.</returns>
    Task ValidateAsync(CancellationToken cancellationToken = default);

    /// <summary>Initializes provider-specific replication metadata when required.</summary>
    /// <param name="cancellationToken">The token used to cancel initialization.</param>
    /// <returns>A task representing the initialization operation.</returns>
    Task InitializeAsync(CancellationToken cancellationToken = default);
}

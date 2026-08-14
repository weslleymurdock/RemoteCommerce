namespace RemoteCommerce.Application.Persistence.Commands;

/// <summary>Validates and initializes the configured primary/replica database topology.</summary>
public sealed record ConfigureDatabaseReplicationCommand : ICommand<Unit>;

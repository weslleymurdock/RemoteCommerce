namespace RemoteCommerce.Application.Persistence.Queries;

/// <summary>Gets the current database topology setup state.</summary>
public sealed record GetDatabaseSetupStateQuery : IQuery<DatabaseSetupState>;

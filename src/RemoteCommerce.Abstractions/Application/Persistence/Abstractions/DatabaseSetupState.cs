namespace RemoteCommerce.Application.Persistence.Abstractions;

/// <summary>Represents the persisted state of required database topology setup.</summary>
public enum DatabaseSetupState
{
    /// <summary>No database setup is required.</summary>
    NotRequired = 0,

    /// <summary>The database topology requires administrator setup.</summary>
    Required = 1,

    /// <summary>Setup is currently being validated or initialized.</summary>
    InProgress = 2,

    /// <summary>Setup completed successfully.</summary>
    Configured = 3,

    /// <summary>The last setup attempt failed and can be retried.</summary>
    Failed = 4
}

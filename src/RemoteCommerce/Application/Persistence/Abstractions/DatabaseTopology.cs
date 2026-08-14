namespace RemoteCommerce.Application.Persistence.Abstractions;

/// <summary>Identifies the database topology used by one RemoteCommerce store.</summary>
public enum DatabaseTopology
{
    /// <summary>Uses one writable relational database endpoint.</summary>
    Single = 0,

    /// <summary>Uses one writable primary endpoint and one or more read replicas.</summary>
    PrimaryReplica = 1
}

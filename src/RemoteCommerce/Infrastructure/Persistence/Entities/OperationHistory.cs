namespace RemoteCommerce.Infrastructure.Persistence.Entities;

/// <summary>Stores an immutable serialized snapshot of a persisted entity mutation.</summary>
public sealed class OperationHistory
{
    /// <summary>Gets or sets the history record identifier.</summary>
    public long Id { get; set; }

    /// <summary>Gets or sets the persisted entity type name.</summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>Gets or sets the serialized entity identity.</summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>Gets or sets the mutation operation type.</summary>
    public string OperationType { get; set; } = string.Empty;

    /// <summary>Gets or sets the UTC mutation timestamp.</summary>
    public DateTimeOffset OccurredAt { get; set; }

    /// <summary>Gets or sets the actor identifier when available.</summary>
    public Guid? UserId { get; set; }

    /// <summary>Gets or sets the actor name when available.</summary>
    public string Actor { get; set; } = string.Empty;

    /// <summary>Gets or sets the request correlation identifier.</summary>
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>Gets or sets the request IP address when available.</summary>
    public string? IpAddress { get; set; }

    /// <summary>Gets or sets the serialized state before the mutation.</summary>
    public string PreviousState { get; set; } = string.Empty;

    /// <summary>Gets or sets the serialized state after the mutation when applicable.</summary>
    public string? NewState { get; set; }
}

namespace RemoteCommerce.Infrastructure.Persistence.Entities;

/// <summary>Stores the latest diagnostic failure associated with a plugin lifecycle operation.</summary>
public sealed class PluginLifecycleError
{
    /// <summary>Gets or sets the persistent error identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the stable plugin identifier.</summary>
    public string PluginId { get; set; } = string.Empty;

    /// <summary>Gets or sets the lifecycle operation that failed.</summary>
    public string Operation { get; set; } = string.Empty;

    /// <summary>Gets or sets the diagnostic error category.</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Gets or sets the human-readable error message.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional exception type name.</summary>
    public string? ExceptionType { get; set; }

    /// <summary>Gets or sets the UTC timestamp at which the error was recorded.</summary>
    public DateTimeOffset CreatedAt { get; set; }
}

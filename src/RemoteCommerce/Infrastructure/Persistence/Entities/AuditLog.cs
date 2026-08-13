namespace RemoteCommerce.Infrastructure.Persistence.Entities;

/// <summary>Records a security-sensitive administrative operation.</summary>
public sealed class AuditLog
{
    /// <summary>Gets or sets the audit record identifier.</summary>
    public long Id { get; set; }

    /// <summary>Gets or sets the authenticated actor identifier, when available.</summary>
    public Guid? UserId { get; set; }

    /// <summary>Gets or sets the actor display name captured at the time of the operation.</summary>
    public string Actor { get; set; } = "system";

    /// <summary>Gets or sets the operation name.</summary>
    public string Operation { get; set; } = string.Empty;

    /// <summary>Gets or sets the resource affected by the operation.</summary>
    public string Resource { get; set; } = string.Empty;

    /// <summary>Gets or sets the outcome of the operation.</summary>
    public string Result { get; set; } = "Success";

    /// <summary>Gets or sets diagnostic context that never contains secret values.</summary>
    public string? Context { get; set; }

    /// <summary>Gets or sets the UTC timestamp of the operation.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

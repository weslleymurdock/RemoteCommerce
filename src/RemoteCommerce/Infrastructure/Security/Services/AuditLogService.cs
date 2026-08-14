namespace RemoteCommerce.Infrastructure.Security.Services;

/// <summary>Persists security-sensitive administrative audit events without secret values.</summary>
/// <param name="db">The scoped persistence context used by the current application operation.</param>
public sealed class AuditLogService(CommerceDbContext db) : IAuditLogService
{
    /// <inheritdoc />
    public async Task WriteAsync(
        string operation,
        string resource,
        Guid? userId,
        string actor,
        string result,
        string? context = null,
        CancellationToken cancellationToken = default)
    {
        db.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Actor = string.IsNullOrWhiteSpace(actor) ? "system" : actor,
            Operation = operation,
            Resource = resource,
            Result = result,
            Context = context,
            CreatedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync(cancellationToken);
    }
}

using Microsoft.EntityFrameworkCore;
using RemoteCommerce.Infrastructure.Persistence;
using RemoteCommerce.Infrastructure.Persistence.Entities;

namespace RemoteCommerce.Application.Security;

/// <summary>Persists security-sensitive administrative audit events without secret values.</summary>
/// <param name="dbFactory">The factory used to persist audit records.</param>
public sealed class AuditLogService(IDbContextFactory<CommerceDbContext> dbFactory) : IAuditLogService
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
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
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

/// <summary>Defines the audit logging contract for administrative operations.</summary>
public interface IAuditLogService
{
    /// <summary>Writes an administrative audit event.</summary>
    /// <param name="operation">The operation identifier.</param>
    /// <param name="resource">The affected resource.</param>
    /// <param name="userId">The actor identifier when available.</param>
    /// <param name="actor">The actor display name.</param>
    /// <param name="result">The operation outcome.</param>
    /// <param name="context">Non-sensitive diagnostic context.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>A task that completes when the audit record is persisted.</returns>
    Task WriteAsync(string operation, string resource, Guid? userId, string actor, string result, string? context = null, CancellationToken cancellationToken = default);
}

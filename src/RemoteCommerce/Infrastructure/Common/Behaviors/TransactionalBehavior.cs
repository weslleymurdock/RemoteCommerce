namespace RemoteCommerce.Infrastructure.Common.Behaviors;

/// <summary>Wraps transactional application commands in the scoped EF Core transaction.</summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed class TransactionalBehavior<TRequest, TResponse>(
    CommerceDbContext db,
    ILogger<TransactionalBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ITransactionalCommand)
        {
            return await next(cancellationToken);
        }

        if (db.Database.CurrentTransaction is not null)
        {
            return await next(cancellationToken);
        }

        var requestName = typeof(TRequest).Name;
        await using var transaction = await db.Database.BeginTransactionAsync(
            cancellationToken);

        try
        {
            var response = await next(cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return response;
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Transactional application request {RequestName} failed and will be rolled back.",
                requestName);
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
        finally
        {
            logger.LogDebug(
                "Transactional application request {RequestName} completed.",
                requestName);
        }
    }
}

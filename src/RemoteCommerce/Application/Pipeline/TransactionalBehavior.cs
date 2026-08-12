namespace RemoteCommerce.Application.Pipeline;

/// <summary>Wraps transactional application commands in an EF Core SQL transaction.</summary>
/// <typeparam name="TRequest">The MediatR request type.</typeparam>
/// <typeparam name="TResponse">The MediatR response type.</typeparam>
/// <param name="db">The scoped commerce persistence context shared by the command handler.</param>
public sealed class TransactionalBehavior<TRequest, TResponse>(CommerceDbContext db) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <summary>Executes transactional commands inside a database transaction and leaves queries outside write transactions.</summary>
    /// <param name="request">The current request.</param>
    /// <param name="next">The next pipeline delegate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response returned by the next pipeline stage.</returns>
    /// <exception cref="Exception">Propagates handler or persistence failures after rolling back the transaction.</exception>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not ITransactionalCommand)
        {
            return await next(cancellationToken);
        }

        if (db.Database.CurrentTransaction is not null)
        {
            return await next(cancellationToken);
        }

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var response = await next(cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return response;
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }
}

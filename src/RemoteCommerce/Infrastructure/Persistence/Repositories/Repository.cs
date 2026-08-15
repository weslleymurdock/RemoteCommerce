namespace RemoteCommerce.Infrastructure.Persistence.Repositories;

/// <summary>Implements the provider-independent repository contract with the host DbContext.</summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public sealed class Repository<TEntity>(
    CommerceDbContext dbContext,
    ILogger<Repository<TEntity>> logger) : IRepository<TEntity>
    where TEntity : class
{
    /// <inheritdoc />
    public Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool tracking,
        CancellationToken cancellationToken,
        params Expression<Func<TEntity, object>>[] includes)
    {
        return ExecuteAsync(
            "FirstOrDefault",
            async () =>
            {
                IQueryable<TEntity> query = BuildQuery(
                    tracking,
                    includes);
                return await query.SingleOrDefaultAsync(
                    predicate,
                    cancellationToken);
            });
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<TEntity>> ListAsync(
        Expression<Func<TEntity, bool>>? predicate,
        Expression<Func<TEntity, object>>? orderBy,
        bool descending,
        int? skip,
        int? take,
        bool tracking,
        CancellationToken cancellationToken,
        params Expression<Func<TEntity, object>>[] includes)
    {
        return ExecuteAsync(
            "List",
            async () =>
            {
                IQueryable<TEntity> query = BuildQuery(
                    tracking,
                    includes);

                if (predicate is not null)
                {
                    query = query.Where(predicate);
                }

                if (orderBy is not null)
                {
                    query = descending
                        ? query.OrderByDescending(orderBy)
                        : query.OrderBy(orderBy);
                }

                if (skip.HasValue)
                {
                    query = query.Skip(skip.Value);
                }

                if (take.HasValue)
                {
                    query = query.Take(take.Value);
                }

                return (IReadOnlyList<TEntity>)await query.ToListAsync(
                    cancellationToken);
            });
    }

    /// <inheritdoc />
    public Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate,
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(
            "Count",
            async () =>
            {
                IQueryable<TEntity> query = dbContext.Set<TEntity>();

                if (predicate is not null)
                {
                    query = query.Where(predicate);
                }

                return await query.CountAsync(cancellationToken);
            });
    }

    /// <inheritdoc />
    public Task AddAsync(
        TEntity entity,
        CancellationToken cancellationToken)
    {
        return ExecuteAsync(
            "Add",
            async () =>
            {
                await dbContext.Set<TEntity>().AddAsync(
                    entity,
                    cancellationToken);
            });
    }

    /// <inheritdoc />
    public void Remove(TEntity entity)
    {
        try
        {
            dbContext.Set<TEntity>().Remove(entity);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Repository operation {Operation} failed for {EntityType}.",
                "Remove",
                typeof(TEntity).Name);
            throw;
        }
        finally
        {
            logger.LogDebug(
                "Repository operation {Operation} completed for {EntityType}.",
                "Remove",
                typeof(TEntity).Name);
        }
    }

    /// <inheritdoc />
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return ExecuteAsync(
            "SaveChanges",
            () => dbContext.SaveChangesAsync(cancellationToken));
    }

    private IQueryable<TEntity> BuildQuery(
        bool tracking,
        IReadOnlyCollection<Expression<Func<TEntity, object>>> includes)
    {
        IQueryable<TEntity> query = dbContext.Set<TEntity>();

        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        foreach (Expression<Func<TEntity, object>> include in includes)
        {
            query = query.Include(include);
        }

        return query;
    }

    private async Task<T> ExecuteAsync<T>(
        string operation,
        Func<Task<T>> action)
    {
        try
        {
            return await action();
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Repository operation {Operation} failed for {EntityType}.",
                operation,
                typeof(TEntity).Name);
            throw;
        }
        finally
        {
            logger.LogDebug(
                "Repository operation {Operation} completed for {EntityType}.",
                operation,
                typeof(TEntity).Name);
        }
    }

    private Task ExecuteAsync(
        string operation,
        Func<Task> action)
    {
        return ExecuteAsync(
            operation,
            async () =>
            {
                await action();
                return true;
            });
    }
}

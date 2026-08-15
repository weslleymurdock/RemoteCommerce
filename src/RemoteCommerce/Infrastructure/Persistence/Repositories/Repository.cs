namespace RemoteCommerce.Infrastructure.Persistence.Repositories;

/// <summary>Implements the provider-independent repository contract with the host DbContext.</summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public sealed class Repository<TEntity>(CommerceDbContext dbContext) : IRepository<TEntity>
    where TEntity : class
{
    /// <inheritdoc />
    public async Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool tracking,
        CancellationToken cancellationToken,
        params Expression<Func<TEntity, object>>[] includes)
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

        return await query.SingleOrDefaultAsync(predicate, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TEntity>> ListAsync(
        Expression<Func<TEntity, bool>>? predicate,
        Expression<Func<TEntity, object>>? orderBy,
        bool descending,
        int? skip,
        int? take,
        bool tracking,
        CancellationToken cancellationToken,
        params Expression<Func<TEntity, object>>[] includes)
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

        return await query.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate,
        CancellationToken cancellationToken)
    {
        IQueryable<TEntity> query = dbContext.Set<TEntity>();

        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        return query.CountAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task AddAsync(
        TEntity entity,
        CancellationToken cancellationToken)
    {
        return dbContext.Set<TEntity>().AddAsync(entity, cancellationToken).AsTask();
    }

    /// <inheritdoc />
    public void Remove(TEntity entity)
    {
        dbContext.Set<TEntity>().Remove(entity);
    }

    /// <inheritdoc />
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}

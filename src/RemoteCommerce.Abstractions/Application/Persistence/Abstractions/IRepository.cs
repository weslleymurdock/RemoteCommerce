namespace RemoteCommerce.Application.Persistence.Abstractions;

/// <summary>Defines a provider-independent repository for a persistence entity.</summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public interface IRepository<TEntity>
    where TEntity : class
{
    /// <summary>Gets an entity matching the predicate.</summary>
    /// <param name="predicate">The entity predicate.</param>
    /// <param name="tracking">Whether the entity should be tracked.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="includes">Navigation properties to load.</param>
    /// <returns>The matching entity, or null.</returns>
    Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool tracking,
        CancellationToken cancellationToken,
        params Expression<Func<TEntity, object>>[] includes);

    /// <summary>Gets entities matching a predicate.</summary>
    /// <param name="predicate">The optional entity predicate.</param>
    /// <param name="orderBy">The optional ordering expression.</param>
    /// <param name="descending">Whether ordering is descending.</param>
    /// <param name="skip">The optional number of records to skip.</param>
    /// <param name="take">The optional number of records to take.</param>
    /// <param name="tracking">Whether the entities should be tracked.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="includes">Navigation properties to load.</param>
    /// <returns>The matching entities.</returns>
    Task<IReadOnlyList<TEntity>> ListAsync(
        Expression<Func<TEntity, bool>>? predicate,
        Expression<Func<TEntity, object>>? orderBy,
        bool descending,
        int? skip,
        int? take,
        bool tracking,
        CancellationToken cancellationToken,
        params Expression<Func<TEntity, object>>[] includes);

    /// <summary>Counts entities matching a predicate.</summary>
    /// <param name="predicate">The optional entity predicate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of matching entities.</returns>
    Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate,
        CancellationToken cancellationToken);

    /// <summary>Adds an entity to the persistence unit.</summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task AddAsync(
        TEntity entity,
        CancellationToken cancellationToken);

    /// <summary>Marks an entity for removal using the persistence boundary's delete semantics.</summary>
    /// <param name="entity">The entity to remove.</param>
    void Remove(TEntity entity);

    /// <summary>Persists the current unit of work.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of state entries written.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

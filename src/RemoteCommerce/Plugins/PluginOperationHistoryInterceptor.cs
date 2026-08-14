namespace RemoteCommerce.Plugins;

/// <summary>Captures plugin persistence mutations into the host operation-history boundary.</summary>
/// <param name="commerceDb">The scoped host DbContext that owns operation history.</param>
/// <param name="applicationContext">The request and actor context used for history metadata.</param>
/// <param name="pluginId">The stable plugin identifier.</param>
public sealed class PluginOperationHistoryInterceptor(
    CommerceDbContext commerceDb,
    IApplicationContext applicationContext,
    string pluginId) : SaveChangesInterceptor
{
    /// <summary>Captures plugin changes before EF Core persists them.</summary>
    /// <param name="eventData">The EF Core save event.</param>
    /// <param name="result">The current interception result.</param>
    /// <returns>The unchanged interception result.</returns>
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        Capture(eventData.Context);
        return result;
    }

    /// <summary>Captures plugin changes before EF Core persists them asynchronously.</summary>
    /// <param name="eventData">The EF Core save event.</param>
    /// <param name="result">The current interception result.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The unchanged interception result.</returns>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Capture(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private void Capture(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var entries = context.ChangeTracker.Entries()
            .Where(entry => entry.State is EntityState.Modified or EntityState.Deleted)
            .ToArray();

        foreach (var entry in entries)
        {
            var previousState = SerializeState(entry.OriginalValues.Properties.ToDictionary(
                property => property.Name,
                property => entry.OriginalValues[property]));
            var wasDeleted = entry.State == EntityState.Deleted;
            var operationType = wasDeleted ? "Delete" : "Update";

            if (wasDeleted && entry.Entity is IPluginSoftDeletable softDeletable)
            {
                softDeletable.IsDisabled = true;
                entry.State = EntityState.Modified;
                operationType = "SoftDelete";
            }

            var newState = entry.State == EntityState.Modified
                ? SerializeState(entry.CurrentValues.Properties.ToDictionary(
                    property => property.Name,
                    property => entry.CurrentValues[property]))
                : null;

            commerceDb.OperationHistories.Add(new OperationHistory
            {
                EntityType = $"{pluginId}:{entry.Metadata.ClrType.FullName ?? entry.Metadata.ClrType.Name}",
                EntityId = SerializeEntityId(entry),
                OperationType = operationType,
                OccurredAt = DateTimeOffset.UtcNow,
                UserId = applicationContext.UserId,
                Actor = applicationContext.Actor,
                CorrelationId = applicationContext.CorrelationId,
                IpAddress = applicationContext.IpAddress,
                PreviousState = previousState,
                NewState = newState
            });
        }
    }

    private static string SerializeEntityId(EntityEntry entry)
    {
        var key = entry.Metadata.FindPrimaryKey();
        if (key is null)
        {
            return string.Empty;
        }

        var values = key.Properties.ToDictionary(
            property => property.Name,
            property => entry.Property(property.Name).CurrentValue);
        return JsonSerializer.Serialize(values);
    }

    private static string SerializeState(IReadOnlyDictionary<string, object?> values)
    {
        var redacted = values.ToDictionary(
            pair => pair.Key,
            pair => IsSensitive(pair.Key) ? "[REDACTED]" : pair.Value);
        return JsonSerializer.Serialize(redacted);
    }

    private static bool IsSensitive(string propertyName)
        => propertyName.Contains("Password", StringComparison.OrdinalIgnoreCase)
            || propertyName.Contains("Secret", StringComparison.OrdinalIgnoreCase)
            || propertyName.Contains("Token", StringComparison.OrdinalIgnoreCase)
            || propertyName.Contains("ApiKey", StringComparison.OrdinalIgnoreCase)
            || propertyName.Contains("ConnectionString", StringComparison.OrdinalIgnoreCase);
}

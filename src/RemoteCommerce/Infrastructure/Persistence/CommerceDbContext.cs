namespace RemoteCommerce.Infrastructure.Persistence;

/// <summary>Provides the EF Core persistence boundary for RemoteCommerce.</summary>
/// <param name="options">The options configured for the database context.</param>
/// <param name="applicationContext">The application context used to populate operation history metadata.</param>
public sealed class CommerceDbContext(DbContextOptions<CommerceDbContext> options, IApplicationContext applicationContext) : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    /// <summary>Gets the installed plugin records.</summary>
    public DbSet<PluginInstallation> PluginInstallations => Set<PluginInstallation>();
    /// <summary>Gets the retained plugin package versions.</summary>
    public DbSet<PluginVersion> PluginVersions => Set<PluginVersion>();
    /// <summary>Gets the declared plugin dependencies.</summary>
    public DbSet<PluginDependency> PluginDependencies => Set<PluginDependency>();
    /// <summary>Gets the plugin lifecycle diagnostic records.</summary>
    public DbSet<PluginLifecycleError> PluginLifecycleErrors => Set<PluginLifecycleError>();
    /// <summary>Gets the plugin configuration values.</summary>
    public DbSet<PluginSetting> PluginSettings => Set<PluginSetting>();
    /// <summary>Gets the editable site settings.</summary>
    public DbSet<SiteSettings> SiteSettings => Set<SiteSettings>();
    /// <summary>Gets the administrative audit records.</summary>
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    /// <summary>Gets imported localization resource metadata.</summary>
    public DbSet<LocalizationResource> LocalizationResources => Set<LocalizationResource>();
    /// <summary>Gets immutable serialized persistence operation history records.</summary>
    public DbSet<OperationHistory> OperationHistories => Set<OperationHistory>();
    /// <summary>Gets catalog products.</summary>
    public DbSet<Product> Products => Set<Product>();
    /// <summary>Gets catalog categories.</summary>
    public DbSet<Category> Categories => Set<Category>();
    /// <summary>Gets catalog brands.</summary>
    public DbSet<Brand> Brands => Set<Brand>();
    /// <summary>Gets catalog tags.</summary>
    public DbSet<RemoteTag> Tags => Set<RemoteTag>();
    /// <summary>Gets product attributes.</summary>
    public DbSet<ProductAttribute> ProductAttributes => Set<ProductAttribute>();
    /// <summary>Gets product attribute values.</summary>
    public DbSet<ProductAttributeValue> ProductAttributeValues => Set<ProductAttributeValue>();
    /// <summary>Gets product attribute assignments.</summary>
    public DbSet<ProductAttributeAssignment> ProductAttributeAssignments => Set<ProductAttributeAssignment>();
    /// <summary>Gets product variants.</summary>
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    /// <summary>Gets variant attribute assignments.</summary>
    public DbSet<ProductVariantAttribute> ProductVariantAttributes => Set<ProductVariantAttribute>();
    /// <summary>Gets product metadata.</summary>
    public DbSet<ProductMetadata> ProductMetadata => Set<ProductMetadata>();
    /// <summary>Gets product media references.</summary>
    public DbSet<ProductMedia> ProductMedia => Set<ProductMedia>();
    /// <summary>Gets product-category relationships.</summary>
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    /// <summary>Gets product-tag relationships.</summary>
    public DbSet<ProductTag> ProductTags => Set<ProductTag>();

    /// <inheritdoc />
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        PreparePersistenceChanges();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    /// <inheritdoc />
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        PreparePersistenceChanges();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>Configures the relational model used by the commerce host.</summary>
    /// <param name="modelBuilder">The model builder used to configure entities.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("commerce");
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CommerceDbContext).Assembly);
    }

    private void PreparePersistenceChanges()
    {
        var entries = ChangeTracker
            .Entries()
            .Where(
                entry =>
                    entry.Entity is not OperationHistory &&
                    (entry.State == EntityState.Modified ||
                     entry.State == EntityState.Deleted))
            .ToArray();

        foreach (var entry in entries)
        {
            var previousState = SerializeState(
                entry.OriginalValues.Properties.ToDictionary(
                    property => property.Name,
                    property => entry.OriginalValues[property]));
            var wasDeleted = entry.State == EntityState.Deleted;
            var operationType = wasDeleted ? "Delete" : "Update";

            if (entry.Entity is RemoteCommerce.Domain.Shared.Abstractions.ISoftDeletable softDeletable &&
                wasDeleted)
            {
                softDeletable.IsDeleted = true;
                softDeletable.DeletedAt = DateTimeOffset.UtcNow;
                softDeletable.IsDisabled = true;
                entry.State = EntityState.Modified;
                operationType = "SoftDelete";
            }

            var newState = entry.State == EntityState.Modified
                ? SerializeState(
                    entry.CurrentValues.Properties.ToDictionary(
                        property => property.Name,
                        property => entry.CurrentValues[property]))
                : null;

            OperationHistories.Add(
                new OperationHistory
                {
                    EntityType = entry.Metadata.ClrType.FullName ?? entry.Metadata.ClrType.Name,
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

        return JsonSerializer.Serialize(
            key.Properties.ToDictionary(
                property => property.Name,
                property => entry.Property(property.Name).CurrentValue));
    }

    private static string SerializeState(IReadOnlyDictionary<string, object?> values)
    {
        return JsonSerializer.Serialize(
            values.ToDictionary(
                pair => pair.Key,
                pair => IsSensitive(pair.Key) ? "[REDACTED]" : pair.Value));
    }

    private static bool IsSensitive(string propertyName)
    {
        return propertyName.Contains(
                   "Password",
                   StringComparison.OrdinalIgnoreCase) ||
               propertyName.Contains(
                   "Secret",
                   StringComparison.OrdinalIgnoreCase) ||
               propertyName.Contains(
                   "Token",
                   StringComparison.OrdinalIgnoreCase) ||
               propertyName.Contains(
                   "ApiKey",
                   StringComparison.OrdinalIgnoreCase) ||
               propertyName.Contains(
                   "ConnectionString",
                   StringComparison.OrdinalIgnoreCase);
    }
}

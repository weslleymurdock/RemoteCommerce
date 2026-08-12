namespace RemoteCommerce.Infrastructure.Persistence;

/// <summary>Provides the EF Core persistence boundary for RemoteCommerce.</summary>
/// <param name="options">The options configured for the database context.</param>
/// <param name="applicationContext">The application context used to populate operation history metadata.</param>
public sealed class CommerceDbContext(
    DbContextOptions<CommerceDbContext> options,
    IApplicationContext applicationContext)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
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

        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.IsDeleted).IsRequired();
            entity.Property(x => x.DeletedAt);
            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<ApplicationRole>(entity =>
        {
            entity.Property(x => x.Description).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.IsDeleted).IsRequired();
            entity.Property(x => x.DeletedAt);
            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<PluginInstallation>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.PluginId).IsUnique();
            entity.Property(x => x.PluginId).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Version).HasMaxLength(50).IsRequired();
            entity.Property(x => x.PackagePath).HasMaxLength(2048).IsRequired();
            entity.Property(x => x.State).IsRequired();
            entity.Property(x => x.DesiredState).IsRequired();
            entity.Property(x => x.PackageHash).HasMaxLength(64).IsRequired();
            entity.Property(x => x.PendingVersion).HasMaxLength(50);
            entity.Property(x => x.LastError).HasMaxLength(4000);
            entity.Property(x => x.InstalledAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.Property(x => x.IsDeleted).IsRequired();
            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<PluginVersion>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.PluginId, x.Version }).IsUnique();
            entity.Property(x => x.PluginId).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Version).HasMaxLength(50).IsRequired();
            entity.Property(x => x.PackagePath).HasMaxLength(2048).IsRequired();
            entity.Property(x => x.PackageHash).HasMaxLength(64).IsRequired();
            entity.Property(x => x.IsDeleted).IsRequired();
            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<PluginDependency>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.PluginId, x.DependencyPluginId }).IsUnique();
            entity.Property(x => x.PluginId).HasMaxLength(200).IsRequired();
            entity.Property(x => x.DependencyPluginId).HasMaxLength(200).IsRequired();
            entity.Property(x => x.MinimumVersion).HasMaxLength(50).IsRequired();
            entity.Property(x => x.MaximumVersion).HasMaxLength(50);
            entity.Property(x => x.IsDeleted).IsRequired();
            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<PluginLifecycleError>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.PluginId, x.CreatedAt });
            entity.Property(x => x.PluginId).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Operation).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Category).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Message).HasMaxLength(4000).IsRequired();
            entity.Property(x => x.ExceptionType).HasMaxLength(500);
        });

        modelBuilder.Entity<PluginSetting>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.PluginId, x.Key }).IsUnique();
            entity.Property(x => x.PluginId).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Key).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Value).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(x => x.Metadata).HasColumnType("nvarchar(max)");
            entity.Property(x => x.IsDeleted).IsRequired();
            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<SiteSettings>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SiteName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.SiteDescription).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.PublicUrl).HasMaxLength(2048).IsRequired();
            entity.Property(x => x.TimeZone).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Culture).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Locale).HasMaxLength(20).IsRequired();
            entity.Property(x => x.IsDeleted).IsRequired();
            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.CreatedAt);
            entity.HasIndex(x => x.UserId);
            entity.Property(x => x.Actor).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Operation).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Resource).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Result).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Context).HasColumnType("nvarchar(max)");
        });

        modelBuilder.Entity<LocalizationResource>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.Culture, x.ResourceType, x.Version }).IsUnique();
            entity.HasIndex(x => new { x.Culture, x.ResourceType, x.IsActive });
            entity.Property(x => x.Culture).HasMaxLength(20).IsRequired();
            entity.Property(x => x.ResourceType).HasMaxLength(500).IsRequired();
            entity.Property(x => x.ContentHash).HasMaxLength(64).IsRequired();
            entity.Property(x => x.IsDeleted).IsRequired();
            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<OperationHistory>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.EntityType, x.EntityId, x.OccurredAt });
            entity.Property(x => x.EntityType).HasMaxLength(500).IsRequired();
            entity.Property(x => x.EntityId).HasMaxLength(500).IsRequired();
            entity.Property(x => x.OperationType).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Actor).HasMaxLength(256).IsRequired();
            entity.Property(x => x.CorrelationId).HasMaxLength(256).IsRequired();
            entity.Property(x => x.IpAddress).HasMaxLength(64);
            entity.Property(x => x.PreviousState).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(x => x.NewState).HasColumnType("nvarchar(max)");
        });
    }

    private void PreparePersistenceChanges()
    {
        var entries = ChangeTracker.Entries()
            .Where(entry => entry.Entity is not OperationHistory && (entry.State == EntityState.Modified || entry.State == EntityState.Deleted))
            .ToArray();

        foreach (var entry in entries)
        {
            var previousState = SerializeState(entry.OriginalValues.Properties.ToDictionary(property => property.Name, property => entry.OriginalValues[property]));
            var operationType = entry.State == EntityState.Deleted ? "Delete" : "Update";

            if (entry.Entity is ISoftDeletable softDeletable && entry.State == EntityState.Deleted)
            {
                softDeletable.IsDeleted = true;
                softDeletable.DeletedAt = DateTimeOffset.UtcNow;
                entry.State = EntityState.Modified;
                operationType = "SoftDelete";
            }

            var newState = entry.State == EntityState.Modified
                ? SerializeState(entry.CurrentValues.Properties.ToDictionary(property => property.Name, property => entry.CurrentValues[property]))
                : null;

            OperationHistories.Add(new OperationHistory
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
                NewState = newState,
            });
        }
    }

    private static string SerializeEntityId(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
    {
        var key = entry.Metadata.FindPrimaryKey();
        if (key is null)
        {
            return string.Empty;
        }

        var values = key.Properties.ToDictionary(property => property.Name, property => entry.Property(property.Name).CurrentValue);
        return JsonSerializer.Serialize(values);
    }

    private static string SerializeState(IReadOnlyDictionary<string, object?> values)
    {
        var redacted = values.ToDictionary(pair => pair.Key, pair => IsSensitive(pair.Key) ? "[REDACTED]" : pair.Value);
        return JsonSerializer.Serialize(redacted);
    }

    private static bool IsSensitive(string propertyName)
        => propertyName.Contains("Password", StringComparison.OrdinalIgnoreCase)
            || propertyName.Contains("Secret", StringComparison.OrdinalIgnoreCase)
            || propertyName.Contains("Token", StringComparison.OrdinalIgnoreCase)
            || propertyName.Contains("ApiKey", StringComparison.OrdinalIgnoreCase)
            || propertyName.Contains("ConnectionString", StringComparison.OrdinalIgnoreCase);
}

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
    public DbSet<Tag> Tags => Set<Tag>();
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
    public override int SaveChanges(bool acceptAllChangesOnSuccess) { PreparePersistenceChanges(); return base.SaveChanges(acceptAllChangesOnSuccess); }
    /// <inheritdoc />
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default) { PreparePersistenceChanges(); return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken); }

    /// <summary>Configures the relational model used by the commerce host.</summary>
    /// <param name="modelBuilder">The model builder used to configure entities.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("commerce");
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<ApplicationUser>(entity => { entity.Property(x => x.DisplayName).HasMaxLength(200).IsRequired(); entity.Property(x => x.IsDisabled).IsRequired(); entity.HasQueryFilter(x => !x.IsDisabled); });
        modelBuilder.Entity<ApplicationRole>(entity => { entity.Property(x => x.Description).HasMaxLength(1000).IsRequired(); entity.Property(x => x.IsDisabled).IsRequired(); entity.HasQueryFilter(x => !x.IsDisabled); });
        modelBuilder.Entity<PluginInstallation>(entity => { entity.HasKey(x => x.Id); entity.HasIndex(x => x.PluginId).IsUnique(); entity.Property(x => x.PluginId).HasMaxLength(200).IsRequired(); entity.Property(x => x.Version).HasMaxLength(50).IsRequired(); entity.Property(x => x.PackagePath).HasMaxLength(2048).IsRequired(); entity.Property(x => x.State).IsRequired(); entity.Property(x => x.DesiredState).IsRequired(); entity.Property(x => x.PackageHash).HasMaxLength(64).IsRequired(); entity.Property(x => x.PendingVersion).HasMaxLength(50); entity.Property(x => x.LastError).HasMaxLength(4000); entity.Property(x => x.InstalledAt).IsRequired(); entity.Property(x => x.UpdatedAt).IsRequired(); entity.Property(x => x.IsDisabled).IsRequired(); entity.HasQueryFilter(x => !x.IsDisabled); });
        modelBuilder.Entity<PluginVersion>(entity => { entity.HasKey(x => x.Id); entity.HasIndex(x => new { x.PluginId, x.Version }).IsUnique(); entity.Property(x => x.PluginId).HasMaxLength(200).IsRequired(); entity.Property(x => x.Version).HasMaxLength(50).IsRequired(); entity.Property(x => x.PackagePath).HasMaxLength(2048).IsRequired(); entity.Property(x => x.PackageHash).HasMaxLength(64).IsRequired(); entity.Property(x => x.IsDisabled).IsRequired(); entity.HasQueryFilter(x => !x.IsDisabled); });
        modelBuilder.Entity<PluginDependency>(entity => { entity.HasKey(x => x.Id); entity.HasIndex(x => new { x.PluginId, x.DependencyPluginId }).IsUnique(); entity.Property(x => x.PluginId).HasMaxLength(200).IsRequired(); entity.Property(x => x.DependencyPluginId).HasMaxLength(200).IsRequired(); entity.Property(x => x.MinimumVersion).HasMaxLength(50).IsRequired(); entity.Property(x => x.MaximumVersion).HasMaxLength(50); entity.Property(x => x.IsDisabled).IsRequired(); entity.HasQueryFilter(x => !x.IsDisabled); });
        modelBuilder.Entity<PluginLifecycleError>(entity => { entity.HasKey(x => x.Id); entity.HasIndex(x => new { x.PluginId, x.CreatedAt }); entity.Property(x => x.PluginId).HasMaxLength(200).IsRequired(); entity.Property(x => x.Operation).HasMaxLength(100).IsRequired(); entity.Property(x => x.Category).HasMaxLength(100).IsRequired(); entity.Property(x => x.Message).HasMaxLength(4000).IsRequired(); entity.Property(x => x.ExceptionType).HasMaxLength(500); });
        modelBuilder.Entity<PluginSetting>(entity => { entity.HasKey(x => x.Id); entity.HasIndex(x => new { x.PluginId, x.Key }).IsUnique(); entity.Property(x => x.PluginId).HasMaxLength(200).IsRequired(); entity.Property(x => x.Key).HasMaxLength(200).IsRequired(); entity.Property(x => x.Value).HasColumnType("nvarchar(max)").IsRequired(); entity.Property(x => x.Metadata).HasColumnType("nvarchar(max)"); entity.Property(x => x.IsDisabled).IsRequired(); entity.HasQueryFilter(x => !x.IsDisabled); });
        modelBuilder.Entity<SiteSettings>(entity => { entity.HasKey(x => x.Id); entity.Property(x => x.SiteName).HasMaxLength(200).IsRequired(); entity.Property(x => x.SiteDescription).HasMaxLength(2000).IsRequired(); entity.Property(x => x.PublicUrl).HasMaxLength(2048).IsRequired(); entity.Property(x => x.TimeZone).HasMaxLength(100).IsRequired(); entity.Property(x => x.Culture).HasMaxLength(20).IsRequired(); entity.Property(x => x.Locale).HasMaxLength(20).IsRequired(); entity.Property(x => x.IsDisabled).IsRequired(); entity.HasQueryFilter(x => !x.IsDisabled); });
        modelBuilder.Entity<AuditLog>(entity => { entity.HasKey(x => x.Id); entity.HasIndex(x => x.CreatedAt); entity.HasIndex(x => x.UserId); entity.Property(x => x.Actor).HasMaxLength(256).IsRequired(); entity.Property(x => x.Operation).HasMaxLength(200).IsRequired(); entity.Property(x => x.Resource).HasMaxLength(500).IsRequired(); entity.Property(x => x.Result).HasMaxLength(50).IsRequired(); entity.Property(x => x.Context).HasColumnType("nvarchar(max)"); });
        modelBuilder.Entity<LocalizationResource>(entity => { entity.HasKey(x => x.Id); entity.HasIndex(x => new { x.Culture, x.ResourceType, x.Version }).IsUnique(); entity.HasIndex(x => new { x.Culture, x.ResourceType, x.IsActive }); entity.Property(x => x.Culture).HasMaxLength(20).IsRequired(); entity.Property(x => x.ResourceType).HasMaxLength(500).IsRequired(); entity.Property(x => x.ContentHash).HasMaxLength(64).IsRequired(); entity.Property(x => x.IsDisabled).IsRequired(); entity.HasQueryFilter(x => !x.IsDisabled); });
        modelBuilder.Entity<OperationHistory>(entity => { entity.HasKey(x => x.Id); entity.HasIndex(x => new { x.EntityType, x.EntityId, x.OccurredAt }); entity.Property(x => x.EntityType).HasMaxLength(500).IsRequired(); entity.Property(x => x.EntityId).HasMaxLength(500).IsRequired(); entity.Property(x => x.OperationType).HasMaxLength(50).IsRequired(); entity.Property(x => x.Actor).HasMaxLength(256).IsRequired(); entity.Property(x => x.CorrelationId).HasMaxLength(256).IsRequired(); entity.Property(x => x.IpAddress).HasMaxLength(64); entity.Property(x => x.PreviousState).HasColumnType("nvarchar(max)").IsRequired(); entity.Property(x => x.NewState).HasColumnType("nvarchar(max)"); });
        ConfigureCatalog(modelBuilder);
    }

    private static void ConfigureCatalog(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CatalogEntity>().UseTpcMappingStrategy();
        modelBuilder.Entity<CatalogEntity>().HasKey(x => x.Id);
        modelBuilder.Entity<CatalogEntity>().HasQueryFilter(x => !x.IsDisabled);
        modelBuilder.Entity<Product>(entity => { entity.HasIndex(x => x.Slug).IsUnique(); entity.HasIndex(x => x.Sku).IsUnique().HasFilter("[Sku] IS NOT NULL"); entity.Property(x => x.Name).HasMaxLength(200).IsRequired(); entity.Property(x => x.Slug).HasMaxLength(200).IsRequired(); entity.Property(x => x.Sku).HasMaxLength(100); entity.Property(x => x.Currency).HasMaxLength(3).IsRequired(); entity.Property(x => x.Price).HasPrecision(18, 4); entity.Property(x => x.CompareAtPrice).HasPrecision(18, 4); entity.HasOne(x => x.Brand).WithMany().HasForeignKey(x => x.BrandId).OnDelete(DeleteBehavior.Restrict); });
        modelBuilder.Entity<Category>(entity => { entity.HasIndex(x => x.Slug).IsUnique(); entity.HasIndex(x => x.ParentId); entity.Property(x => x.Name).HasMaxLength(200).IsRequired(); entity.Property(x => x.Slug).HasMaxLength(200).IsRequired(); entity.HasOne(x => x.Parent).WithMany(x => x.Children).HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict); });
        modelBuilder.Entity<Brand>(entity => { entity.HasIndex(x => x.Slug).IsUnique(); entity.Property(x => x.Name).HasMaxLength(200).IsRequired(); entity.Property(x => x.Slug).HasMaxLength(200).IsRequired(); });
        modelBuilder.Entity<Tag>(entity => { entity.HasIndex(x => x.Slug).IsUnique(); entity.Property(x => x.Name).HasMaxLength(200).IsRequired(); entity.Property(x => x.Slug).HasMaxLength(200).IsRequired(); });
        modelBuilder.Entity<ProductAttribute>(entity => { entity.HasIndex(x => x.Slug).IsUnique(); entity.Property(x => x.Name).HasMaxLength(200).IsRequired(); entity.Property(x => x.Slug).HasMaxLength(200).IsRequired(); });
        modelBuilder.Entity<ProductAttributeValue>(entity => { entity.HasIndex(x => new { x.ProductAttributeId, x.Slug }).IsUnique(); entity.Property(x => x.Value).HasMaxLength(200).IsRequired(); entity.Property(x => x.Slug).HasMaxLength(200).IsRequired(); entity.HasOne(x => x.ProductAttribute).WithMany(x => x.Values).HasForeignKey(x => x.ProductAttributeId).OnDelete(DeleteBehavior.Cascade); });
        modelBuilder.Entity<ProductVariant>(entity => { entity.HasIndex(x => x.Sku).IsUnique(); entity.Property(x => x.Sku).HasMaxLength(100).IsRequired(); entity.Property(x => x.Price).HasPrecision(18, 4); entity.Property(x => x.CompareAtPrice).HasPrecision(18, 4); entity.Property(x => x.StockQuantity).HasPrecision(18, 4); entity.HasOne(x => x.Product).WithMany(x => x.Variants).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade); });
        modelBuilder.Entity<ProductMetadata>(entity => { entity.HasIndex(x => new { x.ProductId, x.Key }).IsUnique(); entity.Property(x => x.Key).HasMaxLength(200).IsRequired(); entity.Property(x => x.Value).HasColumnType("nvarchar(max)").IsRequired(); entity.HasOne<Product>().WithMany(x => x.Metadata).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade); });
        modelBuilder.Entity<ProductMedia>(entity => { entity.HasIndex(x => new { x.ProductId, x.SortOrder }); entity.Property(x => x.AltText).HasMaxLength(500); });
        modelBuilder.Entity<ProductCategory>(entity => { entity.HasIndex(x => new { x.ProductId, x.CategoryId }).IsUnique(); entity.HasOne<Product>().WithMany(x => x.Categories).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade); entity.HasOne<Category>().WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Cascade); });
        modelBuilder.Entity<ProductTag>(entity => { entity.HasIndex(x => new { x.ProductId, x.TagId }).IsUnique(); entity.HasOne<Product>().WithMany(x => x.Tags).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade); entity.HasOne(x => x.Tag).WithMany(x => x.Products).HasForeignKey(x => x.TagId).OnDelete(DeleteBehavior.Cascade); });
        modelBuilder.Entity<ProductAttributeAssignment>(entity => { entity.HasIndex(x => new { x.ProductId, x.ProductAttributeId, x.ProductAttributeValueId }).IsUnique(); });
        modelBuilder.Entity<ProductVariantAttribute>(entity => { entity.HasIndex(x => new { x.ProductVariantId, x.ProductAttributeId, x.ProductAttributeValueId }).IsUnique(); });
    }

    private void PreparePersistenceChanges()
    {
        var entries = ChangeTracker.Entries().Where(entry => entry.Entity is not OperationHistory && (entry.State == EntityState.Modified || entry.State == EntityState.Deleted)).ToArray();
        foreach (var entry in entries)
        {
            var previousState = SerializeState(entry.OriginalValues.Properties.ToDictionary(property => property.Name, property => entry.OriginalValues[property]));
            var wasDeleted = entry.State == EntityState.Deleted;
            var operationType = wasDeleted ? "Delete" : "Update";
            if (entry.Entity is ISoftDeletable softDeletable && wasDeleted) { softDeletable.IsDisabled = true; entry.State = EntityState.Modified; operationType = "SoftDelete"; }
            var newState = entry.State == EntityState.Modified ? SerializeState(entry.CurrentValues.Properties.ToDictionary(property => property.Name, property => entry.CurrentValues[property])) : null;
            OperationHistories.Add(new OperationHistory { EntityType = entry.Metadata.ClrType.FullName ?? entry.Metadata.ClrType.Name, EntityId = SerializeEntityId(entry), OperationType = operationType, OccurredAt = DateTimeOffset.UtcNow, UserId = applicationContext.UserId, Actor = applicationContext.Actor, CorrelationId = applicationContext.CorrelationId, IpAddress = applicationContext.IpAddress, PreviousState = previousState, NewState = newState });
        }
    }
    private static string SerializeEntityId(EntityEntry entry) { var key = entry.Metadata.FindPrimaryKey(); if (key is null) return string.Empty; return JsonSerializer.Serialize(key.Properties.ToDictionary(property => property.Name, property => entry.Property(property.Name).CurrentValue)); }
    private static string SerializeState(IReadOnlyDictionary<string, object?> values) => JsonSerializer.Serialize(values.ToDictionary(pair => pair.Key, pair => IsSensitive(pair.Key) ? "[REDACTED]" : pair.Value));
    private static bool IsSensitive(string propertyName) => propertyName.Contains("Password", StringComparison.OrdinalIgnoreCase) || propertyName.Contains("Secret", StringComparison.OrdinalIgnoreCase) || propertyName.Contains("Token", StringComparison.OrdinalIgnoreCase) || propertyName.Contains("ApiKey", StringComparison.OrdinalIgnoreCase) || propertyName.Contains("ConnectionString", StringComparison.OrdinalIgnoreCase);
}

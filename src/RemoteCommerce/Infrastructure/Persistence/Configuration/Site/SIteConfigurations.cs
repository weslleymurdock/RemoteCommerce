using SharpCompress.Common;

namespace RemoteCommerce.Infrastructure.Persistence.Configuration.Site;

/// <summary>
/// Class that configures the SiteSettings entity for EF Core.
/// </summary>
public class SiteSettingsConfigiruation : IEntityTypeConfiguration<SiteSettings>
{
    /// <summary>
    /// Method that configures the entity for EF Core.
    /// </summary>
    /// <param name="entity">The <see cref="EntityTypeBuilder{SiteSettings}"/> instance.</param>
    public void Configure(EntityTypeBuilder<SiteSettings> entity)
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.SiteName).HasMaxLength(200).IsRequired();
        entity.Property(x => x.SiteDescription).HasMaxLength(2000).IsRequired();
        entity.Property(x => x.PublicUrl).HasMaxLength(2048).IsRequired();
        entity.Property(x => x.TimeZone).HasMaxLength(100).IsRequired();
        entity.Property(x => x.Culture).HasMaxLength(20).IsRequired();
        entity.Property(x => x.Locale).HasMaxLength(20).IsRequired();
        entity.Property(x => x.IsDisabled).IsRequired();
        entity.HasQueryFilter(x => !x.IsDisabled);
    } 
}

/// <summary>
/// Class that configures the Audit Log entity for EF Core.
/// </summary>
public class AuditLogConfigiruation : IEntityTypeConfiguration<AuditLog>
{
    /// <summary>
    /// Method that configures the entity for EF Core.
    /// </summary>
    /// <param name="entity">The <see cref="EntityTypeBuilder{AuditLog}"/> instance.</param>
    public void Configure(EntityTypeBuilder<AuditLog> entity)
    {
        entity.HasKey(x => x.Id);
        entity.HasIndex(x => x.CreatedAt);
        entity.HasIndex(x => x.UserId);
        entity.Property(x => x.Actor).HasMaxLength(256).IsRequired();
        entity.Property(x => x.Operation).HasMaxLength(200).IsRequired();
        entity.Property(x => x.Resource).HasMaxLength(500).IsRequired();
        entity.Property(x => x.Result).HasMaxLength(50).IsRequired();
        entity.Property(x => x.Context).HasColumnType("nvarchar(max)");
    }
}


/// <summary>
/// Class that configures the Localization Resource entity for EF Core.
/// </summary>
public class LocalizationResourceConfigiruation : IEntityTypeConfiguration<LocalizationResource>
{
    /// <summary>
    /// Method that configures the entity for EF Core.
    /// </summary>
    /// <param name="entity">The <see cref="EntityTypeBuilder{LocalizationResource}"/> instance.</param>
    public void Configure(EntityTypeBuilder<LocalizationResource> entity)
    {
        entity.HasKey(x => x.Id);
        entity.HasIndex(x => new { x.Culture, x.ResourceType, x.Version }).IsUnique();
        entity.HasIndex(x => new { x.Culture, x.ResourceType, x.IsActive });
        entity.Property(x => x.Culture).HasMaxLength(20).IsRequired();
        entity.Property(x => x.ResourceType).HasMaxLength(500).IsRequired();
        entity.Property(x => x.ContentHash).HasMaxLength(64).IsRequired();
        entity.Property(x => x.IsDisabled).IsRequired();
        entity.HasQueryFilter(x => !x.IsDisabled);
    }
}


/// <summary>
/// Class that configures the Operation History entity for EF Core.
/// </summary>
public class OperationHistoryConfiguration : IEntityTypeConfiguration<OperationHistory>
{
    /// <summary>
    /// Method that configures the entity for EF Core.
    /// </summary>
    /// <param name="entity">The <see cref="EntityTypeBuilder{OperationHistory}"/> instance.</param>
    public void Configure(EntityTypeBuilder<OperationHistory> entity)
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
    }
}
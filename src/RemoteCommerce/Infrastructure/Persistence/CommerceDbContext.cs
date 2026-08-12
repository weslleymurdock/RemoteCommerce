using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RemoteCommerce.Infrastructure.Persistence.Entities;

namespace RemoteCommerce.Infrastructure.Persistence;

/// <summary>Provides the EF Core persistence boundary for RemoteCommerce.</summary>
/// <param name="options">The options configured for the database context.</param>
public sealed class CommerceDbContext(DbContextOptions<CommerceDbContext> options)
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

    /// <summary>Configures the relational model used by the commerce host.</summary>
    /// <param name="modelBuilder">The model builder used to configure entities.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("commerce");
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<ApplicationRole>(entity =>
        {
            entity.Property(x => x.Description).HasMaxLength(1000).IsRequired();
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
        });

        modelBuilder.Entity<PluginVersion>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.PluginId, x.Version }).IsUnique();
            entity.Property(x => x.PluginId).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Version).HasMaxLength(50).IsRequired();
            entity.Property(x => x.PackagePath).HasMaxLength(2048).IsRequired();
            entity.Property(x => x.PackageHash).HasMaxLength(64).IsRequired();
        });

        modelBuilder.Entity<PluginDependency>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.PluginId, x.DependencyPluginId }).IsUnique();
            entity.Property(x => x.PluginId).HasMaxLength(200).IsRequired();
            entity.Property(x => x.DependencyPluginId).HasMaxLength(200).IsRequired();
            entity.Property(x => x.MinimumVersion).HasMaxLength(50).IsRequired();
            entity.Property(x => x.MaximumVersion).HasMaxLength(50);
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
        });
    }
}

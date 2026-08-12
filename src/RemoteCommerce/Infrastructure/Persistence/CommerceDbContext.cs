using Microsoft.EntityFrameworkCore;
using RemoteCommerce.Infrastructure.Persistence.Entities;

namespace RemoteCommerce.Infrastructure.Persistence;

/// <summary>Provides the EF Core persistence boundary for RemoteCommerce.</summary>
/// <param name="options">The options configured for the database context.</param>
public sealed class CommerceDbContext(DbContextOptions<CommerceDbContext> options) : DbContext(options)
{
    /// <summary>Gets the installed plugin records.</summary>
    public DbSet<PluginInstallation> PluginInstallations => Set<PluginInstallation>();

    /// <summary>Configures the relational model used by the commerce host.</summary>
    /// <param name="modelBuilder">The model builder used to configure entities.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("commerce");
        modelBuilder.Entity<PluginInstallation>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.PluginId).IsUnique();
            entity.Property(x => x.PluginId).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Version).HasMaxLength(50).IsRequired();
            entity.Property(x => x.PackagePath).HasMaxLength(1024).IsRequired();
            entity.Property(x => x.State).IsRequired();
            entity.Property(x => x.InstalledAt).IsRequired();
        });
        base.OnModelCreating(modelBuilder);
    }
}

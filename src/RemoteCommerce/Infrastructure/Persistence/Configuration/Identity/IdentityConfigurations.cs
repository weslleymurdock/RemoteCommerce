using SharpCompress.Common;

namespace RemoteCommerce.Infrastructure.Persistence.Configuration.Identity;
/// <summary>
/// Class that configures the ApplicationUser entity for EF Core.
/// </summary>
public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    /// <summary>
    /// Method called by EF Core to configure the entity.
    /// </summary>
    /// <param name="builder">The <see cref="EntityTypeBuilder{ApplicationUser}"/> instance.</param>
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.IsDisabled).IsRequired();
        builder.HasQueryFilter(x => !x.IsDisabled);
        builder.HasIndex(x => x.UserName).IsUnique();
        builder.HasIndex(x => x.Email).IsUnique();
        builder.Property(x => x.UserName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.NormalizedUserName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
        builder.Property(x => x.NormalizedEmail).HasMaxLength(256).IsRequired();
        builder.Property(x => x.PasswordHash).HasMaxLength(1024);
        builder.Property(x => x.SecurityStamp).HasMaxLength(1024);
        builder.Property(x => x.ConcurrencyStamp).HasMaxLength(1024);
        builder.Property(x => x.PhoneNumber).HasMaxLength(50);
        builder.Property(x => x.TwoFactorEnabled).IsRequired();
        builder.Property(x => x.LockoutEnd);
        builder.Property(x => x.LockoutEnabled).IsRequired();
        builder.Property(x => x.AccessFailedCount).IsRequired();
    }
}

/// <summary>
/// Class that configures the ApplicationRole entity for EF Core.
/// </summary>
public class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    /// <summary>
    /// Method called by EF Core to configure the entity.
    /// </summary>
    /// <param name="builder">The <see cref="EntityTypeBuilder{ApplicationRole}"/> instance.</param>
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.NormalizedName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.ConcurrencyStamp).HasMaxLength(1024);
        builder.Property(x => x.IsDisabled).IsRequired();
        builder.HasQueryFilter(x => !x.IsDisabled);
        builder.HasIndex(x => x.Name).IsUnique();
    }
}
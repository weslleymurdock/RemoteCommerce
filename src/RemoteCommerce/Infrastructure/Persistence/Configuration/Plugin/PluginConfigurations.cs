namespace RemoteCommerce.Infrastructure.Persistence.Configuration.Plugin;
/// <summary>
/// Class that configures the PluginInstallation entity for EF Core.
/// </summary>
public class PluginInstallationConfiguration : IEntityTypeConfiguration<PluginInstallation>
{
    /// <summary>
    /// Method called by EF Core to configure the entity.
    /// </summary>
    /// <param name="builder">The <see cref="EntityTypeBuilder{PluginInstallation}"/> instance.</param>
    public void Configure(EntityTypeBuilder<PluginInstallation> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.PluginId).IsUnique();

        builder.Property(x => x.PluginId).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Version).HasMaxLength(50).IsRequired();
        builder.Property(x => x.PackagePath).HasMaxLength(2048).IsRequired();
        builder.Property(x => x.State).IsRequired();
        builder.Property(x => x.DesiredState).IsRequired();
        builder.Property(x => x.PackageHash).HasMaxLength(64).IsRequired();
        builder.Property(x => x.PendingVersion).HasMaxLength(50);
        builder.Property(x => x.LastError).HasMaxLength(4000);
        builder.Property(x => x.InstalledAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
        builder.Property(x => x.IsDisabled).IsRequired();

        builder.HasQueryFilter(x => !x.IsDisabled);
    }
}
/// <summary>
/// Class that configures the PluginVersion entity for EF Core.
/// </summary>
public class PluginVersionConfiguration : IEntityTypeConfiguration<PluginVersion>
{
    /// <summary>
    /// Method called by EF Core to configure the entity.
    /// </summary>
    /// <param name="builder">The <see cref="EntityTypeBuilder{PluginVersion}"/> instance.</param>
    public void Configure(EntityTypeBuilder<PluginVersion> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.PluginId, x.Version }).IsUnique();

        builder.Property(x => x.PluginId).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Version).HasMaxLength(50).IsRequired();
        builder.Property(x => x.PackagePath).HasMaxLength(2048).IsRequired();
        builder.Property(x => x.PackageHash).HasMaxLength(64).IsRequired();
        builder.Property(x => x.IsDisabled).IsRequired();

        builder.HasQueryFilter(x => !x.IsDisabled);
    }
}

/// <summary>
/// Class that configures the PluginDependency entity for EF Core.
/// </summary>
public class PluginDependencyConfiguration : IEntityTypeConfiguration<PluginDependency>
{
    /// <summary>
    /// Method called by EF Core to configure the entity.
    /// </summary>
    /// <param name="builder">The <see cref="EntityTypeBuilder{PluginDependency}"/> instance.</param>
    public void Configure(EntityTypeBuilder<PluginDependency> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.PluginId, x.DependencyPluginId }).IsUnique();

        builder.Property(x => x.PluginId).HasMaxLength(200).IsRequired();
        builder.Property(x => x.DependencyPluginId).HasMaxLength(200).IsRequired();
        builder.Property(x => x.MinimumVersion).HasMaxLength(50).IsRequired();
        builder.Property(x => x.MaximumVersion).HasMaxLength(50);
        builder.Property(x => x.IsDisabled).IsRequired();

        builder.HasQueryFilter(x => !x.IsDisabled);
    }
}

/// <summary>
/// Class that configures the PluginLifecycleError entity for EF Core.
/// </summary>
public class PluginLifecycleErrorConfiguration : IEntityTypeConfiguration<PluginLifecycleError>
{
    /// <summary>
    /// Method called by EF Core to configure the entity.
    /// </summary>
    /// <param name="builder">The <see cref="EntityTypeBuilder{PluginLifecycleError}"/> instance.</param>
    public void Configure(EntityTypeBuilder<PluginLifecycleError> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.PluginId, x.CreatedAt });

        builder.Property(x => x.PluginId).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Operation).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Category).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Message).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.ExceptionType).HasMaxLength(500);
    }
}

/// <summary>
/// Class that configures the PluginSetting entity for EF Core.
/// </summary>
public class PluginSettingConfiguration : IEntityTypeConfiguration<PluginSetting>
{
    /// <summary>
    /// Method called by EF Core to configure the entity.
    /// </summary>
    /// <param name="builder">The <see cref="EntityTypeBuilder{PluginSetting}"/> instance.</param>
    public void Configure(EntityTypeBuilder<PluginSetting> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.PluginId, x.Key }).IsUnique();

        builder.Property(x => x.PluginId).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Key).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Value).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(x => x.Metadata).HasColumnType("nvarchar(max)");
        builder.Property(x => x.IsDisabled).IsRequired();

        builder.HasQueryFilter(x => !x.IsDisabled);
    }
}
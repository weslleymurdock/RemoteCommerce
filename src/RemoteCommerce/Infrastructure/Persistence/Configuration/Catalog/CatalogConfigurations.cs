using SharpCompress.Common;

namespace RemoteCommerce.Infrastructure.Persistence.Configuration.Catalog;


/// <summary>
/// Class that configures the Catalog entity for EF Core.
/// </summary>
public class CatalogConfiguration : IEntityTypeConfiguration<CatalogEntity>
{
    /// <summary>
    /// Method called by EF Core to configure the entity.
    /// </summary>
    /// <param name="builder">The <see cref="EntityTypeBuilder{CatalogEntity}"/> instance.</param>
    public void Configure(EntityTypeBuilder<CatalogEntity> builder)
    {
        builder.UseTpcMappingStrategy();
        builder.HasKey(x => x.Id);
        builder.HasQueryFilter(x => !x.IsDisabled);
    }
}

/// <summary>
/// Class that configures the Product entity for EF Core.
/// </summary>
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    /// <summary>
    /// Method called by EF Core to configure the entity.
    /// </summary>
    /// <param name="builder">The <see cref="EntityTypeBuilder{Product}"/> instance.</param>
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasIndex(x => x.Slug).IsUnique(); 
        builder.HasIndex(x => x.Sku).IsUnique().HasFilter("[Sku] IS NOT NULL"); 
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired(); 
        builder.Property(x => x.Slug).HasMaxLength(200).IsRequired(); 
        builder.Property(x => x.Sku).HasMaxLength(100); 
        builder.Property(x => x.Currency).HasMaxLength(3).IsRequired(); 
        builder.Property(x => x.Price).HasPrecision(18, 4); 
        builder.Property(x => x.CompareAtPrice).HasPrecision(18, 4); 
        builder.HasOne(x => x.Brand).WithMany().HasForeignKey(x => x.BrandId).OnDelete(DeleteBehavior.Restrict);
    }
}

/// <summary>
/// Class that configures the Category entity for EF Core.
/// </summary>
public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    /// <summary>
    /// Method called by EF Core to configure the entity.
    /// </summary>
    /// <param name="builder">The <see cref="EntityTypeBuilder{Category}"/> instance.</param>
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => x.ParentId);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(200).IsRequired();
        builder.HasOne(x => x.Parent).WithMany(x => x.Children).HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict);
    }
}

/// <summary>
/// Class that configures the Brand entity for EF Core.
/// </summary>
public class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    /// <summary>
    /// Method called by EF Core to configure the entity.
    /// </summary>
    /// <param name="builder">The <see cref="EntityTypeBuilder{Brand}"/> instance.</param>
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(200).IsRequired();
    }
}

/// <summary>
/// Class that configures the Tag entity for EF Core.
/// </summary>
public class TagConfiguration : IEntityTypeConfiguration<RemoteTag>
{
    /// <summary>
    /// Method called by EF Core to configure the entity.
    /// </summary>
    /// <param name="builder">The <see cref="EntityTypeBuilder{RemoteTag}"/> instance.</param>
    public void Configure(EntityTypeBuilder<RemoteTag> builder)
    {
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(200).IsRequired();
    }
}


/// <summary>
/// Class that configures the Product attribute entity for EF Core.
/// </summary>
public class ProductAttributeConfiguration : IEntityTypeConfiguration<ProductAttribute>
{
    /// <summary>
    /// Method called by EF Core to configure the entity.
    /// </summary>
    /// <param name="builder">The <see cref="EntityTypeBuilder{ProductAttribute}"/> instance.</param>
    public void Configure(EntityTypeBuilder<ProductAttribute> builder)
    {
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(200).IsRequired();
    }
}

/// <summary>
/// Class that configures the Product attribute value entity for EF Core.
/// </summary>
public class ProductAttributeValueConfiguration : IEntityTypeConfiguration<ProductAttributeValue>
{
    /// <summary>
    /// Method called by EF Core to configure the entity.
    /// </summary>
    /// <param name="builder">The <see cref="EntityTypeBuilder{ProductAttributeValue}"/> instance.</param>
    public void Configure(EntityTypeBuilder<ProductAttributeValue> builder)
    {
        builder.HasIndex(x => new { x.ProductAttributeId, x.Slug }).IsUnique();
        builder.Property(x => x.Value).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(200).IsRequired();
        builder.HasOne(x => x.ProductAttribute).WithMany(x => x.Values).HasForeignKey(x => x.ProductAttributeId).OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// Class that configures the Product variant entity for EF Core.
/// </summary>
public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    /// <summary>
    /// Method called by EF Core to configure the entity.
    /// </summary>
    /// <param name="builder">The <see cref="EntityTypeBuilder{ProductVariant}"/> instance.</param>
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.HasIndex(x => x.Sku).IsUnique();
        builder.Property(x => x.Sku).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Price).HasPrecision(18, 4);
        builder.Property(x => x.CompareAtPrice).HasPrecision(18, 4);
        builder.Property(x => x.StockQuantity).HasPrecision(18, 4);
        builder.HasOne(x => x.Product).WithMany(x => x.Variants).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
    }
}
 
/// <summary>
/// Class that configures the Product metadata entity for EF Core.
/// </summary>
public class ProductMetadataConfiguration : IEntityTypeConfiguration<ProductMetadata>
{
    /// <summary>
    /// Method called by EF Core to configure the entity.
    /// </summary>
    /// <param name="builder">The <see cref="EntityTypeBuilder{ProductMetadata}"/> instance.</param>
    public void Configure(EntityTypeBuilder<ProductMetadata> builder)
    {
        builder.HasIndex(x => new { x.ProductId, x.Key }).IsUnique();
        builder.Property(x => x.Key).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Value).HasColumnType("nvarchar(max)").IsRequired();
        builder.HasOne<Product>().WithMany(x => x.Metadata).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
    }
}
 
/// <summary>
/// Class that configures the Product media entity for EF Core.
/// </summary>
public class ProductMediaConfiguration : IEntityTypeConfiguration<ProductMedia>
{
    /// <summary>
    /// Method called by EF Core to configure the entity.
    /// </summary>
    /// <param name="builder">The <see cref="EntityTypeBuilder{ProductMedia}"/> instance.</param>
    public void Configure(EntityTypeBuilder<ProductMedia> builder)
    {
        builder.HasIndex(x => new { x.ProductId, x.SortOrder });
        builder.Property(x => x.AltText).HasMaxLength(500);
    }
}
 
/// <summary>
/// Class that configures the Product category entity for EF Core.
/// </summary>
public class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    /// <summary>
    /// Method called by EF Core to configure the entity.
    /// </summary>
    /// <param name="builder">The <see cref="EntityTypeBuilder{ProductCategory}"/> instance.</param>
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.HasIndex(x => new { x.ProductId, x.CategoryId }).IsUnique(); 
        builder.HasOne<Product>().WithMany(x => x.Categories).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade); 
        builder.HasOne<Category>().WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Cascade);
    }
}
 
/// <summary>
/// Class that configures the Product tag entity for EF Core.
/// </summary>
public class ProductTagConfiguration : IEntityTypeConfiguration<ProductTag>
{
    /// <summary>
    /// Method called by EF Core to configure the entity.
    /// </summary>
    /// <param name="builder">The <see cref="EntityTypeBuilder{ProductTag}"/> instance.</param>
    public void Configure(EntityTypeBuilder<ProductTag> builder)
    {
        builder.HasIndex(x => new { x.ProductId, x.TagId }).IsUnique(); 
        builder.HasOne<Product>().WithMany(x => x.Tags).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade); 
        builder.HasOne(x => x.Tag).WithMany(x => x.Products).HasForeignKey(x => x.TagId).OnDelete(DeleteBehavior.Cascade);
    }
}
 
/// <summary>
/// Class that configures the Product attribute assignment entity for EF Core.
/// </summary>
public class ProductAttributeAssignmentConfiguration : IEntityTypeConfiguration<ProductAttributeAssignment>
{
    /// <summary>
    /// Method called by EF Core to configure the entity.
    /// </summary>
    /// <param name="builder">The <see cref="EntityTypeBuilder{ProductAttributeAssignment}"/> instance.</param>
    public void Configure(EntityTypeBuilder<ProductAttributeAssignment> builder)
    {
        builder.HasIndex(x => new { x.ProductId, x.ProductAttributeId, x.ProductAttributeValueId }).IsUnique();
    }
}
 
/// <summary>
/// Class that configures the Product variant attribute assignment entity for EF Core.
/// </summary>
public class ProductVariantAttributeConfiguration : IEntityTypeConfiguration<ProductVariantAttribute>
{
    /// <summary>
    /// Method called by EF Core to configure the entity.
    /// </summary>
    /// <param name="builder">The <see cref="EntityTypeBuilder{ProductVariantAttribute}"/> instance.</param>
    public void Configure(EntityTypeBuilder<ProductVariantAttribute> builder)
    {
        builder.HasIndex(x => new { x.ProductVariantId, x.ProductAttributeId, x.ProductAttributeValueId }).IsUnique();
    }
}

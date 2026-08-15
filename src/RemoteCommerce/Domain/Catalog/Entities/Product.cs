namespace RemoteCommerce.Domain.Catalog.Entities;

/// <summary>Represents a catalog product aggregate.</summary>
public sealed class Product : CatalogEntity
{
    /// <summary>Gets or sets the product display name.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Gets or sets the URL slug.</summary>
    public string Slug { get; set; } = string.Empty;
    /// <summary>Gets or sets the optional product SKU.</summary>
    public string? Sku { get; set; }
    /// <summary>Gets or sets the short description.</summary>
    public string ShortDescription { get; set; } = string.Empty;
    /// <summary>Gets or sets the full description.</summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>Gets or sets the lifecycle status.</summary>
    public ProductStatus Status { get; set; } = ProductStatus.Draft;
    /// <summary>Gets or sets the product type.</summary>
    public ProductType ProductType { get; set; } = ProductType.Simple;
    /// <summary>Gets or sets the current catalog price.</summary>
    public decimal Price { get; set; }
    /// <summary>Gets or sets the optional comparison price.</summary>
    public decimal? CompareAtPrice { get; set; }
    /// <summary>Gets or sets the ISO currency code.</summary>
    public string Currency { get; set; } = "USD";
    /// <summary>Gets or sets the optional brand identifier.</summary>
    public Guid? BrandId { get; set; }
    /// <summary>Gets or sets the optional brand.</summary>
    public Brand? Brand { get; set; }
    /// <summary>Gets the category relationships.</summary>
    public ICollection<ProductCategory> Categories { get; } = [];
    /// <summary>Gets the tag relationships.</summary>
    public ICollection<ProductTag> Tags { get; } = [];
    /// <summary>Gets the attribute assignments.</summary>
    public ICollection<ProductAttributeAssignment> Attributes { get; } = [];
    /// <summary>Gets the product variants.</summary>
    public ICollection<ProductVariant> Variants { get; } = [];
    /// <summary>Gets the metadata records.</summary>
    public ICollection<ProductMetadata> Metadata { get; } = [];
    /// <summary>Gets the media references.</summary>
    public ICollection<ProductMedia> Media { get; } = [];
}

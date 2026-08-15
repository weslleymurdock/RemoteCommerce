namespace RemoteCommerce.Domain.Catalog.Entities;

/// <summary>Represents a purchasable variation of a product for catalog purposes.</summary>
public sealed class ProductVariant : CatalogEntity
{
    /// <summary>Gets or sets the owning product identifier.</summary>
    public Guid ProductId { get; set; }
    /// <summary>Gets or sets the variant SKU.</summary>
    public string Sku { get; set; } = string.Empty;
    /// <summary>Gets or sets the variant price.</summary>
    public decimal Price { get; set; }
    /// <summary>Gets or sets the optional comparison price.</summary>
    public decimal? CompareAtPrice { get; set; }
    /// <summary>Gets or sets the basic stock quantity.</summary>
    public decimal StockQuantity { get; set; }
    /// <summary>Gets or sets whether stock is managed for this variant.</summary>
    public bool ManageStock { get; set; } = true;
    /// <summary>Gets or sets the variant lifecycle status.</summary>
    public ProductVariantStatus Status { get; set; } = ProductVariantStatus.Draft;
    /// <summary>Gets or sets the owning product.</summary>
    public Product? Product { get; set; }
    /// <summary>Gets the variant attribute assignments.</summary>
    public ICollection<ProductVariantAttribute> Attributes { get; } = [];
}

/// <summary>Represents extensible metadata attached to a product.</summary>
public sealed class ProductMetadata : CatalogEntity
{
    /// <summary>Gets or sets the owning product identifier.</summary>
    public Guid ProductId { get; set; }
    /// <summary>Gets or sets the validated metadata key.</summary>
    public string Key { get; set; } = string.Empty;
    /// <summary>Gets or sets the metadata value type.</summary>
    public MetadataValueType Type { get; set; }
    /// <summary>Gets or sets the serialized metadata value.</summary>
    public string Value { get; set; } = string.Empty;
}

/// <summary>Represents a media-provider reference associated with a product.</summary>
public sealed class ProductMedia : CatalogEntity
{
    /// <summary>Gets or sets the owning product identifier.</summary>
    public Guid ProductId { get; set; }
    /// <summary>Gets or sets the media provider identifier.</summary>
    public Guid MediaId { get; set; }
    /// <summary>Gets or sets the media role.</summary>
    public ProductMediaRole Role { get; set; } = ProductMediaRole.Gallery;
    /// <summary>Gets or sets the display ordering.</summary>
    public int SortOrder { get; set; }
    /// <summary>Gets or sets alternative text.</summary>
    public string AltText { get; set; } = string.Empty;
}

/// <summary>Associates a product with a category.</summary>
public sealed class ProductCategory : CatalogEntity
{
    /// <summary>Gets or sets the product identifier.</summary>
    public Guid ProductId { get; set; }
    /// <summary>Gets or sets the category identifier.</summary>
    public Guid CategoryId { get; set; }
}

/// <summary>Associates a product with a tag.</summary>
public sealed class ProductTag : CatalogEntity
{
    /// <summary>Gets or sets the product identifier.</summary>
    public Guid ProductId { get; set; }
    /// <summary>Gets or sets the tag identifier.</summary>
    public Guid TagId { get; set; }
    /// <summary>Gets or sets the tag relationship.</summary>
    public Tag? Tag { get; set; }
}

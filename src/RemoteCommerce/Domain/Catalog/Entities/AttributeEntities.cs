namespace RemoteCommerce.Domain.Catalog.Entities;

/// <summary>Represents a configurable product attribute.</summary>
public sealed class ProductAttribute : CatalogEntity
{
    /// <summary>Gets or sets the attribute name.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Gets or sets the attribute slug.</summary>
    public string Slug { get; set; } = string.Empty;
    /// <summary>Gets or sets extensible attribute metadata as JSON.</summary>
    public string MetadataJson { get; set; } = "{}";
    /// <summary>Gets the allowed values.</summary>
    public ICollection<ProductAttributeValue> Values { get; } = [];
}

/// <summary>Represents one value belonging to a product attribute.</summary>
public sealed class ProductAttributeValue : CatalogEntity
{
    /// <summary>Gets or sets the owning attribute identifier.</summary>
    public Guid ProductAttributeId { get; set; }
    /// <summary>Gets or sets the attribute value.</summary>
    public string Value { get; set; } = string.Empty;
    /// <summary>Gets or sets the value slug.</summary>
    public string Slug { get; set; } = string.Empty;
    /// <summary>Gets or sets the owning attribute.</summary>
    public ProductAttribute? ProductAttribute { get; set; }
}

/// <summary>Associates a product with an allowed attribute value.</summary>
public sealed class ProductAttributeAssignment : CatalogEntity
{
    /// <summary>Gets or sets the product identifier.</summary>
    public Guid ProductId { get; set; }
    /// <summary>Gets or sets the attribute identifier.</summary>
    public Guid ProductAttributeId { get; set; }
    /// <summary>Gets or sets the attribute value identifier.</summary>
    public Guid ProductAttributeValueId { get; set; }
}

/// <summary>Associates a variant with an attribute value.</summary>
public sealed class ProductVariantAttribute : CatalogEntity
{
    /// <summary>Gets or sets the variant identifier.</summary>
    public Guid ProductVariantId { get; set; }
    /// <summary>Gets or sets the attribute identifier.</summary>
    public Guid ProductAttributeId { get; set; }
    /// <summary>Gets or sets the attribute value identifier.</summary>
    public Guid ProductAttributeValueId { get; set; }
}

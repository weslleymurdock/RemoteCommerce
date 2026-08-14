namespace RemoteCommerce.Domain.Catalog.Entities;

/// <summary>Represents a hierarchical product category.</summary>
public sealed class Category : CatalogEntity
{
    /// <summary>Gets or sets the category name.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Gets or sets the category slug.</summary>
    public string Slug { get; set; } = string.Empty;
    /// <summary>Gets or sets the category description.</summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>Gets or sets the optional parent category identifier.</summary>
    public Guid? ParentId { get; set; }
    /// <summary>Gets or sets the optional category image reference.</summary>
    public Guid? ImageMediaId { get; set; }
    /// <summary>Gets or sets the display order.</summary>
    public int DisplayOrder { get; set; }
    /// <summary>Gets or sets the parent category.</summary>
    public Category? Parent { get; set; }
    /// <summary>Gets the child categories.</summary>
    public ICollection<Category> Children { get; } = [];
}

/// <summary>Represents a product brand.</summary>
public sealed class Brand : CatalogEntity
{
    /// <summary>Gets or sets the brand name.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Gets or sets the brand slug.</summary>
    public string Slug { get; set; } = string.Empty;
    /// <summary>Gets or sets the brand description.</summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>Gets or sets the optional logo media reference.</summary>
    public Guid? LogoMediaId { get; set; }
}

/// <summary>Represents a product tag.</summary>
public sealed class Tag : CatalogEntity
{
    /// <summary>Gets or sets the tag name.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Gets or sets the tag slug.</summary>
    public string Slug { get; set; } = string.Empty;
    /// <summary>Gets or sets the tag description.</summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>Gets product relationships using this tag.</summary>
    public ICollection<ProductTag> Products { get; } = [];
}

namespace RemoteCommerce.Application.Catalog.Requests;

/// <summary>Represents the payload for creating a product.</summary>
public class CreateProductRequest
{
    /// <summary>Gets or sets the product name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the URL slug.</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional product SKU.</summary>
    public string? Sku { get; set; }

    /// <summary>Gets or sets the short description.</summary>
    public string ShortDescription { get; set; } = string.Empty;

    /// <summary>Gets or sets the product description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the product status.</summary>
    public ProductStatus Status { get; set; }

    /// <summary>Gets or sets the product type.</summary>
    public ProductType ProductType { get; set; }

    /// <summary>Gets or sets the product price.</summary>
    public decimal Price { get; set; }

    /// <summary>Gets or sets the optional comparison price.</summary>
    public decimal? CompareAtPrice { get; set; }

    /// <summary>Gets or sets the ISO currency code.</summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional brand identifier.</summary>
    public Guid? BrandId { get; set; }
}

/// <summary>Represents the payload for updating a product.</summary>
public sealed class UpdateProductRequest : CreateProductRequest
{
    /// <summary>Gets or sets the product identifier.</summary>
    public Guid Id { get; set; }
}

/// <summary>Represents the payload for creating a category.</summary>
public class CreateCategoryRequest
{
    /// <summary>Gets or sets the category name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the category slug.</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>Gets or sets the category description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional parent identifier.</summary>
    public Guid? ParentId { get; set; }

    /// <summary>Gets or sets the display order.</summary>
    public int DisplayOrder { get; set; }
}

/// <summary>Represents the payload for updating a category.</summary>
public sealed class UpdateCategoryRequest : CreateCategoryRequest
{
    /// <summary>Gets or sets the category identifier.</summary>
    public Guid Id { get; set; }
}

/// <summary>Represents the payload for creating a brand.</summary>
public class CreateBrandRequest
{
    /// <summary>Gets or sets the brand name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the brand slug.</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>Gets or sets the brand description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional logo media identifier.</summary>
    public Guid? LogoMediaId { get; set; }
}

/// <summary>Represents the payload for updating a brand.</summary>
public sealed class UpdateBrandRequest : CreateBrandRequest
{
    /// <summary>Gets or sets the brand identifier.</summary>
    public Guid Id { get; set; }
}

/// <summary>Represents the payload for creating a tag.</summary>
public class CreateTagRequest
{
    /// <summary>Gets or sets the tag name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the tag slug.</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>Gets or sets the tag description.</summary>
    public string Description { get; set; } = string.Empty;
}

/// <summary>Represents the payload for updating a tag.</summary>
public sealed class UpdateTagRequest : CreateTagRequest
{
    /// <summary>Gets or sets the tag identifier.</summary>
    public Guid Id { get; set; }
}

/// <summary>Represents the payload for creating a product variation.</summary>
public class CreateProductVariantRequest
{
    /// <summary>Gets or sets the owning product identifier.</summary>
    public Guid ProductId { get; set; }

    /// <summary>Gets or sets the variant SKU.</summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>Gets or sets the variant price.</summary>
    public decimal Price { get; set; }

    /// <summary>Gets or sets the optional comparison price.</summary>
    public decimal? CompareAtPrice { get; set; }

    /// <summary>Gets or sets the stock quantity.</summary>
    public decimal StockQuantity { get; set; }

    /// <summary>Gets or sets whether stock is managed.</summary>
    public bool ManageStock { get; set; }

    /// <summary>Gets or sets the variant status.</summary>
    public ProductVariantStatus Status { get; set; }
}

/// <summary>Represents the payload for updating a product variation.</summary>
public sealed class UpdateProductVariantRequest : CreateProductVariantRequest
{
    /// <summary>Gets or sets the variant identifier.</summary>
    public Guid Id { get; set; }
}

/// <summary>Represents the payload for creating or replacing metadata.</summary>
public sealed class UpsertProductMetadataRequest
{
    /// <summary>Gets or sets the owning product identifier.</summary>
    public Guid ProductId { get; set; }

    /// <summary>Gets or sets the metadata key.</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Gets or sets the metadata value type.</summary>
    public MetadataValueType Type { get; set; }

    /// <summary>Gets or sets the metadata value.</summary>
    public string Value { get; set; } = string.Empty;
}

/// <summary>Represents product listing filters and pagination.</summary>
public sealed class ProductListRequest
{
    /// <summary>Gets or sets the page number.</summary>
    public int Page { get; set; } = 1;

    /// <summary>Gets or sets the requested page size.</summary>
    public int PageSize { get; set; } = 20;

    /// <summary>Gets or sets the optional search text.</summary>
    public string? Search { get; set; }

    /// <summary>Gets or sets the optional product status filter.</summary>
    public ProductStatus? Status { get; set; }

    /// <summary>Gets or sets the optional category filter.</summary>
    public Guid? CategoryId { get; set; }

    /// <summary>Gets or sets the optional brand filter.</summary>
    public Guid? BrandId { get; set; }

    /// <summary>Gets or sets the optional tag filter.</summary>
    public string? Tag { get; set; }

    /// <summary>Gets or sets the optional SKU filter.</summary>
    public string? Sku { get; set; }

    /// <summary>Gets or sets the optional product type filter.</summary>
    public ProductType? ProductType { get; set; }
}

/// <summary>Represents a request containing a product identifier.</summary>
public sealed class ProductIdRequest
{
    /// <summary>Gets or sets the product identifier.</summary>
    public Guid Id { get; set; }
}

/// <summary>Represents a request containing a product and variation identifier.</summary>
public sealed class ProductVariationIdRequest
{
    /// <summary>Gets or sets the product identifier.</summary>
    public Guid ProductId { get; set; }

    /// <summary>Gets or sets the variation identifier.</summary>
    public Guid VariationId { get; set; }
}

/// <summary>Represents a request containing a product and metadata key.</summary>
public sealed class ProductMetadataKeyRequest
{
    /// <summary>Gets or sets the product identifier.</summary>
    public Guid ProductId { get; set; }

    /// <summary>Gets or sets the metadata key.</summary>
    public string Key { get; set; } = string.Empty;
}

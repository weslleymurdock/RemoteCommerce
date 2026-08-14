namespace RemoteCommerce.Application.Catalog.Models;

/// <summary>Represents a product.</summary>
public sealed record ProductModel(Guid Id, string Name, string Slug, string? Sku, string ShortDescription, string Description, ProductStatus Status, ProductType ProductType, decimal Price, decimal? CompareAtPrice, string Currency, Guid? BrandId, DateTime CreatedAt, DateTime UpdatedAt);
/// <summary>Represents a category.</summary>
public sealed record CategoryModel(Guid Id, string Name, string Slug, string Description, Guid? ParentId, int DisplayOrder);
/// <summary>Represents a brand.</summary>
public sealed record BrandModel(Guid Id, string Name, string Slug, string Description, Guid? LogoMediaId);
/// <summary>Represents a tag.</summary>
public sealed record TagModel(Guid Id, string Name, string Slug, string Description);
/// <summary>Represents an attribute.</summary>
public sealed record AttributeModel(Guid Id, string Name, string Slug, IReadOnlyList<AttributeValueModel> Values);
/// <summary>Represents an attribute value.</summary>
public sealed record AttributeValueModel(Guid Id, string Value, string Slug);
/// <summary>Represents a product variant.</summary>
public sealed record ProductVariantModel(Guid Id, Guid ProductId, string Sku, decimal Price, decimal? CompareAtPrice, decimal StockQuantity, bool ManageStock, ProductVariantStatus Status);
/// <summary>Represents product metadata.</summary>
public sealed record ProductMetadataModel(Guid Id, Guid ProductId, string Key, MetadataValueType Type, string Value);

namespace RemoteCommerce.Application.Catalog.Commands;


/// <summary>Creates a product.</summary>
public sealed record CreateProductCommand(CreateProductRequest data) : IRequest<ProductModel>, ITransactionalCommand;
/// <summary>Updates a product.</summary>
public sealed record UpdateProductCommand(Guid Id, string Name, string Slug, string? Sku, string ShortDescription, string Description, ProductStatus Status, ProductType ProductType, decimal Price, decimal? CompareAtPrice, string Currency, Guid? BrandId) : IRequest<ProductModel?>, ITransactionalCommand;
/// <summary>Deletes a product.</summary>
public sealed record DeleteProductCommand(Guid Id) : IRequest, ITransactionalCommand;
/// <summary>Publishes a product.</summary>
public sealed record PublishProductCommand(Guid Id) : IRequest<ProductModel?>, ITransactionalCommand;
/// <summary>Archives a product.</summary>
public sealed record ArchiveProductCommand(Guid Id) : IRequest<ProductModel?>, ITransactionalCommand;

/// <summary>Creates a category.</summary>
public sealed record CreateCategoryCommand(string Name, string Slug, string Description, Guid? ParentId, int DisplayOrder) : IRequest<CategoryModel>, ITransactionalCommand;
/// <summary>Updates a category.</summary>
public sealed record UpdateCategoryCommand(Guid Id, string Name, string Slug, string Description, Guid? ParentId, int DisplayOrder) : IRequest<CategoryModel?>, ITransactionalCommand;
/// <summary>Deletes a category.</summary>
public sealed record DeleteCategoryCommand(Guid Id) : IRequest, ITransactionalCommand;
/// <summary>Creates a brand.</summary>
public sealed record CreateBrandCommand(string Name, string Slug, string Description, Guid? LogoMediaId) : IRequest<BrandModel>, ITransactionalCommand;
/// <summary>Updates a brand.</summary>
public sealed record UpdateBrandCommand(Guid Id, string Name, string Slug, string Description, Guid? LogoMediaId) : IRequest<BrandModel?>, ITransactionalCommand;
/// <summary>Deletes a brand.</summary>
public sealed record DeleteBrandCommand(Guid Id) : IRequest, ITransactionalCommand;
/// <summary>Creates a tag.</summary>
public sealed record CreateTagCommand(string Name, string Slug, string Description) : IRequest<TagModel>, ITransactionalCommand;
/// <summary>Updates a tag.</summary>
public sealed record UpdateTagCommand(Guid Id, string Name, string Slug, string Description) : IRequest<TagModel?>, ITransactionalCommand;
/// <summary>Deletes a tag.</summary>
public sealed record DeleteTagCommand(Guid Id) : IRequest, ITransactionalCommand;
/// <summary>Creates a product variant.</summary>
public sealed record CreateProductVariantCommand(Guid ProductId, string Sku, decimal Price, decimal? CompareAtPrice, decimal StockQuantity, bool ManageStock, ProductVariantStatus Status) : IRequest<ProductVariantModel>, ITransactionalCommand;
/// <summary>Updates a product variant.</summary>
public sealed record UpdateProductVariantCommand(Guid ProductId, Guid Id, string Sku, decimal Price, decimal? CompareAtPrice, decimal StockQuantity, bool ManageStock, ProductVariantStatus Status) : IRequest<ProductVariantModel?>, ITransactionalCommand;
/// <summary>Deletes a product variant.</summary>
public sealed record DeleteProductVariantCommand(Guid ProductId, Guid Id) : IRequest, ITransactionalCommand;

/// <summary>Creates or replaces product metadata.</summary>
public sealed record UpsertProductMetadataCommand(Guid ProductId, string Key, MetadataValueType Type, string Value) : IRequest<ProductMetadataModel>, ITransactionalCommand;
/// <summary>Deletes product metadata.</summary>
public sealed record DeleteProductMetadataCommand(Guid ProductId, string Key) : IRequest, ITransactionalCommand;

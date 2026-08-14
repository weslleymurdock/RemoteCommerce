namespace RemoteCommerce.Application.Catalog;

/// <summary>Defines the catalog application service boundary.</summary>
public interface ICatalogService
{
    /// <summary>Creates a product.</summary>
    Task<ProductModel> CreateProductAsync(CreateProductCommand command, CancellationToken cancellationToken);
    /// <summary>Updates a product.</summary>
    Task<ProductModel?> UpdateProductAsync(UpdateProductCommand command, CancellationToken cancellationToken);
    /// <summary>Soft-deletes a product.</summary>
    Task DeleteProductAsync(Guid id, CancellationToken cancellationToken);
    /// <summary>Gets a product.</summary>
    Task<ProductModel?> GetProductAsync(Guid id, CancellationToken cancellationToken);
    /// <summary>Lists products using bounded pagination and filters.</summary>
    Task<PagedResult<ProductModel>> ListProductsAsync(ProductListQuery query, CancellationToken cancellationToken);
    /// <summary>Gets categories.</summary>
    Task<IReadOnlyList<CategoryModel>> GetCategoriesAsync(CancellationToken cancellationToken);
    /// <summary>Gets brands.</summary>
    Task<IReadOnlyList<BrandModel>> GetBrandsAsync(CancellationToken cancellationToken);
    /// <summary>Gets tags.</summary>
    Task<IReadOnlyList<TagModel>> GetTagsAsync(CancellationToken cancellationToken);
    /// <summary>Gets attributes.</summary>
    Task<IReadOnlyList<AttributeModel>> GetAttributesAsync(CancellationToken cancellationToken);
    /// <summary>Creates a category.</summary>
    Task<CategoryModel> CreateCategoryAsync(CreateCategoryCommand command, CancellationToken cancellationToken);
    /// <summary>Creates a brand.</summary>
    Task<BrandModel> CreateBrandAsync(CreateBrandCommand command, CancellationToken cancellationToken);
    /// <summary>Creates a tag.</summary>
    Task<TagModel> CreateTagAsync(CreateTagCommand command, CancellationToken cancellationToken);
}

/// <summary>Represents a bounded page of records.</summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
/// <summary>Represents a product API/application model.</summary>
public sealed record ProductModel(Guid Id, string Name, string Slug, string? Sku, string ShortDescription, string Description, ProductStatus Status, ProductType ProductType, decimal Price, decimal? CompareAtPrice, string Currency, Guid? BrandId, DateTime CreatedAt, DateTime UpdatedAt);
/// <summary>Represents a category application model.</summary>
public sealed record CategoryModel(Guid Id, string Name, string Slug, string Description, Guid? ParentId, int DisplayOrder);
/// <summary>Represents a brand application model.</summary>
public sealed record BrandModel(Guid Id, string Name, string Slug, string Description, Guid? LogoMediaId);
/// <summary>Represents a tag application model.</summary>
public sealed record TagModel(Guid Id, string Name, string Slug, string Description);
/// <summary>Represents an attribute application model.</summary>
public sealed record AttributeModel(Guid Id, string Name, string Slug, IReadOnlyList<AttributeValueModel> Values);
/// <summary>Represents an attribute value application model.</summary>
public sealed record AttributeValueModel(Guid Id, string Value, string Slug);
/// <summary>Creates a catalog product.</summary>
public sealed record CreateProductCommand(string Name, string Slug, string? Sku, string ShortDescription, string Description, ProductStatus Status, ProductType ProductType, decimal Price, decimal? CompareAtPrice, string Currency, Guid? BrandId) : IRequest<ProductModel>, ITransactionalCommand;
/// <summary>Updates a catalog product.</summary>
public sealed record UpdateProductCommand(Guid Id, string Name, string Slug, string? Sku, string ShortDescription, string Description, ProductStatus Status, ProductType ProductType, decimal Price, decimal? CompareAtPrice, string Currency, Guid? BrandId) : IRequest<ProductModel?>, ITransactionalCommand;
/// <summary>Deletes a catalog product.</summary>
public sealed record DeleteProductCommand(Guid Id) : IRequest, ITransactionalCommand;
/// <summary>Publishes a product.</summary>
public sealed record PublishProductCommand(Guid Id) : IRequest<ProductModel?>, ITransactionalCommand;
/// <summary>Archives a product.</summary>
public sealed record ArchiveProductCommand(Guid Id) : IRequest<ProductModel?>, ITransactionalCommand;
/// <summary>Lists products.</summary>
public sealed record ProductListQuery(int Page = 1, int PageSize = 20, string? Search = null, ProductStatus? Status = null, Guid? CategoryId = null, Guid? BrandId = null, string? Tag = null, string? Sku = null, ProductType? ProductType = null) : IRequest<PagedResult<ProductModel>>;
/// <summary>Creates a category.</summary>
public sealed record CreateCategoryCommand(string Name, string Slug, string Description, Guid? ParentId, int DisplayOrder) : IRequest<CategoryModel>, ITransactionalCommand;
/// <summary>Creates a brand.</summary>
public sealed record CreateBrandCommand(string Name, string Slug, string Description, Guid? LogoMediaId) : IRequest<BrandModel>, ITransactionalCommand;
/// <summary>Creates a tag.</summary>
public sealed record CreateTagCommand(string Name, string Slug, string Description) : IRequest<TagModel>, ITransactionalCommand;

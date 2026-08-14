namespace RemoteCommerce.Application.Catalog;

/// <summary>Defines the application boundary for catalog use cases.</summary>
public interface ICatalogService
{
    /// <summary>Creates a product.</summary><param name="command">The product data.</param><param name="cancellationToken">The cancellation token.</param><returns>The created product.</returns>
    Task<ProductModel> CreateProductAsync(CreateProductCommand command, CancellationToken cancellationToken);
    /// <summary>Updates a product.</summary><param name="command">The product data.</param><param name="cancellationToken">The cancellation token.</param><returns>The updated product, or null.</returns>
    Task<ProductModel?> UpdateProductAsync(UpdateProductCommand command, CancellationToken cancellationToken);
    /// <summary>Soft-deletes a product.</summary><param name="id">The product identifier.</param><param name="cancellationToken">The cancellation token.</param>
    Task DeleteProductAsync(Guid id, CancellationToken cancellationToken);
    /// <summary>Gets a product.</summary><param name="id">The product identifier.</param><param name="cancellationToken">The cancellation token.</param><returns>The product, or null.</returns>
    Task<ProductModel?> GetProductAsync(Guid id, CancellationToken cancellationToken);
    /// <summary>Lists products.</summary><param name="query">The filters and paging options.</param><param name="cancellationToken">The cancellation token.</param><returns>A bounded page.</returns>
    Task<PagedResult<ProductModel>> ListProductsAsync(ProductListQuery query, CancellationToken cancellationToken);
    /// <summary>Gets categories.</summary><param name="cancellationToken">The cancellation token.</param><returns>The categories.</returns>
    Task<IReadOnlyList<CategoryModel>> GetCategoriesAsync(CancellationToken cancellationToken);
    /// <summary>Gets brands.</summary><param name="cancellationToken">The cancellation token.</param><returns>The brands.</returns>
    Task<IReadOnlyList<BrandModel>> GetBrandsAsync(CancellationToken cancellationToken);
    /// <summary>Gets tags.</summary><param name="cancellationToken">The cancellation token.</param><returns>The tags.</returns>
    Task<IReadOnlyList<TagModel>> GetTagsAsync(CancellationToken cancellationToken);
    /// <summary>Gets attributes.</summary><param name="cancellationToken">The cancellation token.</param><returns>The attributes.</returns>
    Task<IReadOnlyList<AttributeModel>> GetAttributesAsync(CancellationToken cancellationToken);
    /// <summary>Creates a category.</summary><param name="command">The category data.</param><param name="cancellationToken">The cancellation token.</param><returns>The created category.</returns>
    Task<CategoryModel> CreateCategoryAsync(CreateCategoryCommand command, CancellationToken cancellationToken);
    /// <summary>Updates a category.</summary><param name="command">The category data.</param><param name="cancellationToken">The cancellation token.</param><returns>The updated category, or null.</returns>
    Task<CategoryModel?> UpdateCategoryAsync(UpdateCategoryCommand command, CancellationToken cancellationToken);
    /// <summary>Soft-deletes a category.</summary><param name="id">The category identifier.</param><param name="cancellationToken">The cancellation token.</param>
    Task DeleteCategoryAsync(Guid id, CancellationToken cancellationToken);
    /// <summary>Creates a brand.</summary><param name="command">The brand data.</param><param name="cancellationToken">The cancellation token.</param><returns>The created brand.</returns>
    Task<BrandModel> CreateBrandAsync(CreateBrandCommand command, CancellationToken cancellationToken);
    /// <summary>Updates a brand.</summary><param name="command">The brand data.</param><param name="cancellationToken">The cancellation token.</param><returns>The updated brand, or null.</returns>
    Task<BrandModel?> UpdateBrandAsync(UpdateBrandCommand command, CancellationToken cancellationToken);
    /// <summary>Soft-deletes a brand.</summary><param name="id">The brand identifier.</param><param name="cancellationToken">The cancellation token.</param>
    Task DeleteBrandAsync(Guid id, CancellationToken cancellationToken);
    /// <summary>Creates a tag.</summary><param name="command">The tag data.</param><param name="cancellationToken">The cancellation token.</param><returns>The created tag.</returns>
    Task<TagModel> CreateTagAsync(CreateTagCommand command, CancellationToken cancellationToken);
    /// <summary>Updates a tag.</summary><param name="command">The tag data.</param><param name="cancellationToken">The cancellation token.</param><returns>The updated tag, or null.</returns>
    Task<TagModel?> UpdateTagAsync(UpdateTagCommand command, CancellationToken cancellationToken);
    /// <summary>Soft-deletes a tag.</summary><param name="id">The tag identifier.</param><param name="cancellationToken">The cancellation token.</param>
    Task DeleteTagAsync(Guid id, CancellationToken cancellationToken);
    /// <summary>Creates a product variant.</summary><param name="command">The variant data.</param><param name="cancellationToken">The cancellation token.</param><returns>The created variant.</returns>
    Task<ProductVariantModel> CreateVariantAsync(CreateProductVariantCommand command, CancellationToken cancellationToken);
    /// <summary>Updates a product variant.</summary><param name="command">The variant data.</param><param name="cancellationToken">The cancellation token.</param><returns>The updated variant, or null.</returns>
    Task<ProductVariantModel?> UpdateVariantAsync(UpdateProductVariantCommand command, CancellationToken cancellationToken);
    /// <summary>Soft-deletes a product variant.</summary><param name="productId">The owning product.</param><param name="variantId">The variant identifier.</param><param name="cancellationToken">The cancellation token.</param>
    Task DeleteVariantAsync(Guid productId, Guid variantId, CancellationToken cancellationToken);
    /// <summary>Gets variants for a product.</summary><param name="productId">The product identifier.</param><param name="cancellationToken">The cancellation token.</param><returns>The product variants.</returns>
    Task<IReadOnlyList<ProductVariantModel>> GetVariantsAsync(Guid productId, CancellationToken cancellationToken);
    /// <summary>Gets product metadata.</summary><param name="productId">The product identifier.</param><param name="cancellationToken">The cancellation token.</param><returns>The metadata records.</returns>
    Task<IReadOnlyList<ProductMetadataModel>> GetMetadataAsync(Guid productId, CancellationToken cancellationToken);
    /// <summary>Creates or replaces product metadata.</summary><param name="command">The metadata data.</param><param name="cancellationToken">The cancellation token.</param><returns>The metadata record.</returns>
    Task<ProductMetadataModel> UpsertMetadataAsync(UpsertProductMetadataCommand command, CancellationToken cancellationToken);
    /// <summary>Deletes product metadata.</summary><param name="productId">The product identifier.</param><param name="key">The metadata key.</param><param name="cancellationToken">The cancellation token.</param>
    Task DeleteMetadataAsync(Guid productId, string key, CancellationToken cancellationToken);
}

/// <summary>Represents a bounded page of records.</summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
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
/// <summary>Creates a product.</summary>
public sealed record CreateProductCommand(string Name, string Slug, string? Sku, string ShortDescription, string Description, ProductStatus Status, ProductType ProductType, decimal Price, decimal? CompareAtPrice, string Currency, Guid? BrandId) : IRequest<ProductModel>, ITransactionalCommand;
/// <summary>Updates a product.</summary>
public sealed record UpdateProductCommand(Guid Id, string Name, string Slug, string? Sku, string ShortDescription, string Description, ProductStatus Status, ProductType ProductType, decimal Price, decimal? CompareAtPrice, string Currency, Guid? BrandId) : IRequest<ProductModel?>, ITransactionalCommand;
/// <summary>Deletes a product.</summary>
public sealed record DeleteProductCommand(Guid Id) : IRequest, ITransactionalCommand;
/// <summary>Publishes a product.</summary>
public sealed record PublishProductCommand(Guid Id) : IRequest<ProductModel?>, ITransactionalCommand;
/// <summary>Archives a product.</summary>
public sealed record ArchiveProductCommand(Guid Id) : IRequest<ProductModel?>, ITransactionalCommand;
/// <summary>Lists products.</summary>
public sealed record ProductListQuery(int Page = 1, int PageSize = 20, string? Search = null, ProductStatus? Status = null, Guid? CategoryId = null, Guid? BrandId = null, string? Tag = null, string? Sku = null, ProductType? ProductType = null) : IRequest<PagedResult<ProductModel>>;
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
/// <summary>Lists variants.</summary>
public sealed record ProductVariantListQuery(Guid ProductId) : IRequest<IReadOnlyList<ProductVariantModel>>;
/// <summary>Lists product metadata.</summary>
public sealed record ProductMetadataQuery(Guid ProductId) : IRequest<IReadOnlyList<ProductMetadataModel>>;
/// <summary>Creates or replaces product metadata.</summary>
public sealed record UpsertProductMetadataCommand(Guid ProductId, string Key, MetadataValueType Type, string Value) : IRequest<ProductMetadataModel>, ITransactionalCommand;
/// <summary>Deletes product metadata.</summary>
public sealed record DeleteProductMetadataCommand(Guid ProductId, string Key) : IRequest, ITransactionalCommand;

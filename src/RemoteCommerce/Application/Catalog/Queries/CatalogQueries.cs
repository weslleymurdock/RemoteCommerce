namespace RemoteCommerce.Application.Catalog.Queries;

/// <summary>Gets a product by identifier.</summary>
public sealed record GetProductQuery(Guid Id) : IRequest<ProductModel?>;
 
/// <summary>Reads categories.</summary>
public sealed record GetCategoriesQuery : IRequest<IReadOnlyList<CategoryModel>>;
/// <summary>Reads brands.</summary>
public sealed record GetBrandsQuery : IRequest<IReadOnlyList<BrandModel>>;
/// <summary>Reads tags.</summary>
public sealed record GetTagsQuery : IRequest<IReadOnlyList<TagModel>>;
/// <summary>Reads attributes.</summary>
public sealed record GetAttributesQuery : IRequest<IReadOnlyList<AttributeModel>>;

/// <summary>Lists products.</summary>
public sealed record ProductListQuery(int Page = 1, int PageSize = 20, string? Search = null, ProductStatus? Status = null, Guid? CategoryId = null, Guid? BrandId = null, string? Tag = null, string? Sku = null, ProductType? ProductType = null) : IRequest<PagedResult<ProductModel>>;

/// <summary>Lists variants.</summary>
public sealed record ProductVariantListQuery(Guid ProductId) : IRequest<IReadOnlyList<ProductVariantModel>>;

/// <summary>Lists product metadata.</summary>
public sealed record ProductMetadataQuery(Guid ProductId) : IRequest<IReadOnlyList<ProductMetadataModel>>;
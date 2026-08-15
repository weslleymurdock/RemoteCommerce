namespace RemoteCommerce.Application.Catalog.Queries;

/// <summary>Gets a product by identifier.</summary>
public sealed record GetProductQuery(ProductIdRequest Request) : IRequest<Result<ProductModel>>;

/// <summary>Lists catalog products.</summary>
public sealed record ProductListQuery(ProductListRequest Request) : IRequest<Result<PagedResult<ProductModel>>>;

/// <summary>Lists categories.</summary>
public sealed record GetCategoriesQuery : IRequest<Result<IReadOnlyList<CategoryModel>>>;

/// <summary>Gets a category by identifier.</summary>
public sealed record GetCategoryQuery(ProductIdRequest Request) : IRequest<Result<CategoryModel>>;

/// <summary>Lists brands.</summary>
public sealed record GetBrandsQuery : IRequest<Result<IReadOnlyList<BrandModel>>>;

/// <summary>Gets a brand by identifier.</summary>
public sealed record GetBrandQuery(ProductIdRequest Request) : IRequest<Result<BrandModel>>;

/// <summary>Lists tags.</summary>
public sealed record GetTagsQuery : IRequest<Result<IReadOnlyList<TagModel>>>;

/// <summary>Gets a tag by identifier.</summary>
public sealed record GetTagQuery(ProductIdRequest Request) : IRequest<Result<TagModel>>;

/// <summary>Lists product attributes.</summary>
public sealed record GetAttributesQuery : IRequest<Result<IReadOnlyList<AttributeModel>>>;

/// <summary>Gets product variations.</summary>
public sealed record ProductVariantListQuery(ProductIdRequest Request) : IRequest<Result<IReadOnlyList<ProductVariantModel>>>;

/// <summary>Gets a product variation.</summary>
public sealed record GetProductVariantQuery(ProductVariationIdRequest Request) : IRequest<Result<ProductVariantModel>>;

/// <summary>Lists product metadata.</summary>
public sealed record ProductMetadataQuery(ProductIdRequest Request) : IRequest<Result<IReadOnlyList<ProductMetadataModel>>>;

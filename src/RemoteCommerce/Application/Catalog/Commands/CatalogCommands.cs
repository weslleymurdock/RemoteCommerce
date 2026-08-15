namespace RemoteCommerce.Application.Catalog.Commands;

/// <summary>Creates a product from an operation request.</summary>
public sealed record CreateProductCommand(CreateProductRequest Request) : IRequest<Result<ProductModel>>, ITransactionalCommand;

/// <summary>Updates a product from an operation request.</summary>
public sealed record UpdateProductCommand(UpdateProductRequest Request) : IRequest<Result<ProductModel>>, ITransactionalCommand;

/// <summary>Deletes a product from an operation request.</summary>
public sealed record DeleteProductCommand(ProductIdRequest Request) : IRequest<Result>, ITransactionalCommand;

/// <summary>Publishes a product from an operation request.</summary>
public sealed record PublishProductCommand(ProductIdRequest Request) : IRequest<Result<ProductModel>>, ITransactionalCommand;

/// <summary>Archives a product from an operation request.</summary>
public sealed record ArchiveProductCommand(ProductIdRequest Request) : IRequest<Result<ProductModel>>, ITransactionalCommand;

/// <summary>Creates a category from an operation request.</summary>
public sealed record CreateCategoryCommand(CreateCategoryRequest Request) : IRequest<Result<CategoryModel>>, ITransactionalCommand;

/// <summary>Updates a category from an operation request.</summary>
public sealed record UpdateCategoryCommand(UpdateCategoryRequest Request) : IRequest<Result<CategoryModel>>, ITransactionalCommand;

/// <summary>Deletes a category from an operation request.</summary>
public sealed record DeleteCategoryCommand(ProductIdRequest Request) : IRequest<Result>, ITransactionalCommand;

/// <summary>Creates a brand from an operation request.</summary>
public sealed record CreateBrandCommand(CreateBrandRequest Request) : IRequest<Result<BrandModel>>, ITransactionalCommand;

/// <summary>Updates a brand from an operation request.</summary>
public sealed record UpdateBrandCommand(UpdateBrandRequest Request) : IRequest<Result<BrandModel>>, ITransactionalCommand;

/// <summary>Deletes a brand from an operation request.</summary>
public sealed record DeleteBrandCommand(ProductIdRequest Request) : IRequest<Result>, ITransactionalCommand;

/// <summary>Creates a tag from an operation request.</summary>
public sealed record CreateTagCommand(CreateTagRequest Request) : IRequest<Result<TagModel>>, ITransactionalCommand;

/// <summary>Updates a tag from an operation request.</summary>
public sealed record UpdateTagCommand(UpdateTagRequest Request) : IRequest<Result<TagModel>>, ITransactionalCommand;

/// <summary>Deletes a tag from an operation request.</summary>
public sealed record DeleteTagCommand(ProductIdRequest Request) : IRequest<Result>, ITransactionalCommand;

/// <summary>Creates a product variation from an operation request.</summary>
public sealed record CreateProductVariantCommand(CreateProductVariantRequest Request) : IRequest<Result<ProductVariantModel>>, ITransactionalCommand;

/// <summary>Updates a product variation from an operation request.</summary>
public sealed record UpdateProductVariantCommand(UpdateProductVariantRequest Request) : IRequest<Result<ProductVariantModel>>, ITransactionalCommand;

/// <summary>Deletes a product variation from an operation request.</summary>
public sealed record DeleteProductVariantCommand(ProductVariationIdRequest Request) : IRequest<Result>, ITransactionalCommand;

/// <summary>Creates or replaces product metadata from an operation request.</summary>
public sealed record UpsertProductMetadataCommand(UpsertProductMetadataRequest Request) : IRequest<Result<ProductMetadataModel>>, ITransactionalCommand;

/// <summary>Deletes product metadata from an operation request.</summary>
public sealed record DeleteProductMetadataCommand(ProductMetadataKeyRequest Request) : IRequest<Result>, ITransactionalCommand;

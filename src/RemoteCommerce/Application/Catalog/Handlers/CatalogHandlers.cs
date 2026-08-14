namespace RemoteCommerce.Application.Catalog.Handlers;

/// <summary>Handles catalog commands and queries through the application service.</summary>
public sealed class CatalogHandlers(ICatalogService catalog) :
    IRequestHandler<CreateProductCommand, ProductModel>, IRequestHandler<UpdateProductCommand, ProductModel?>, IRequestHandler<DeleteProductCommand>,
    IRequestHandler<PublishProductCommand, ProductModel?>, IRequestHandler<ArchiveProductCommand, ProductModel?>, IRequestHandler<ProductListQuery, PagedResult<ProductModel>>,
    IRequestHandler<CreateCategoryCommand, CategoryModel>, IRequestHandler<UpdateCategoryCommand, CategoryModel?>, IRequestHandler<DeleteCategoryCommand>,
    IRequestHandler<CreateBrandCommand, BrandModel>, IRequestHandler<UpdateBrandCommand, BrandModel?>, IRequestHandler<DeleteBrandCommand>,
    IRequestHandler<CreateTagCommand, TagModel>, IRequestHandler<UpdateTagCommand, TagModel?>, IRequestHandler<DeleteTagCommand>,
    IRequestHandler<CreateProductVariantCommand, ProductVariantModel>, IRequestHandler<UpdateProductVariantCommand, ProductVariantModel?>, IRequestHandler<DeleteProductVariantCommand>,
    IRequestHandler<ProductVariantListQuery, IReadOnlyList<ProductVariantModel>>, IRequestHandler<ProductMetadataQuery, IReadOnlyList<ProductMetadataModel>>,
    IRequestHandler<UpsertProductMetadataCommand, ProductMetadataModel>, IRequestHandler<DeleteProductMetadataCommand>
{
    /// <inheritdoc />
    public Task<ProductModel> Handle(CreateProductCommand r, CancellationToken c) => catalog.CreateProductAsync(r, c);
    /// <inheritdoc />
    public Task<ProductModel?> Handle(UpdateProductCommand r, CancellationToken c) => catalog.UpdateProductAsync(r, c);
    /// <inheritdoc />
    public Task Handle(DeleteProductCommand r, CancellationToken c) => catalog.DeleteProductAsync(r.Id, c);
    /// <inheritdoc />
    public async Task<ProductModel?> Handle(PublishProductCommand r, CancellationToken c) => await ChangeStatus(r.Id, ProductStatus.Published, c);
    /// <inheritdoc />
    public async Task<ProductModel?> Handle(ArchiveProductCommand r, CancellationToken c) => await ChangeStatus(r.Id, ProductStatus.Archived, c);
    /// <inheritdoc />
    public Task<PagedResult<ProductModel>> Handle(ProductListQuery r, CancellationToken c) => catalog.ListProductsAsync(r, c);
    /// <inheritdoc />
    public Task<CategoryModel> Handle(CreateCategoryCommand r, CancellationToken c) => catalog.CreateCategoryAsync(r, c);
    /// <inheritdoc />
    public Task<CategoryModel?> Handle(UpdateCategoryCommand r, CancellationToken c) => catalog.UpdateCategoryAsync(r, c);
    /// <inheritdoc />
    public Task Handle(DeleteCategoryCommand r, CancellationToken c) => catalog.DeleteCategoryAsync(r.Id, c);
    /// <inheritdoc />
    public Task<BrandModel> Handle(CreateBrandCommand r, CancellationToken c) => catalog.CreateBrandAsync(r, c);
    /// <inheritdoc />
    public Task<BrandModel?> Handle(UpdateBrandCommand r, CancellationToken c) => catalog.UpdateBrandAsync(r, c);
    /// <inheritdoc />
    public Task Handle(DeleteBrandCommand r, CancellationToken c) => catalog.DeleteBrandAsync(r.Id, c);
    /// <inheritdoc />
    public Task<TagModel> Handle(CreateTagCommand r, CancellationToken c) => catalog.CreateTagAsync(r, c);
    /// <inheritdoc />
    public Task<TagModel?> Handle(UpdateTagCommand r, CancellationToken c) => catalog.UpdateTagAsync(r, c);
    /// <inheritdoc />
    public Task Handle(DeleteTagCommand r, CancellationToken c) => catalog.DeleteTagAsync(r.Id, c);
    /// <inheritdoc />
    public Task<ProductVariantModel> Handle(CreateProductVariantCommand r, CancellationToken c) => catalog.CreateVariantAsync(r, c);
    /// <inheritdoc />
    public Task<ProductVariantModel?> Handle(UpdateProductVariantCommand r, CancellationToken c) => catalog.UpdateVariantAsync(r, c);
    /// <inheritdoc />
    public Task Handle(DeleteProductVariantCommand r, CancellationToken c) => catalog.DeleteVariantAsync(r.ProductId, r.Id, c);
    /// <inheritdoc />
    public Task<IReadOnlyList<ProductVariantModel>> Handle(ProductVariantListQuery r, CancellationToken c) => catalog.GetVariantsAsync(r.ProductId, c);
    /// <inheritdoc />
    public Task<IReadOnlyList<ProductMetadataModel>> Handle(ProductMetadataQuery r, CancellationToken c) => catalog.GetMetadataAsync(r.ProductId, c);
    /// <inheritdoc />
    public Task<ProductMetadataModel> Handle(UpsertProductMetadataCommand r, CancellationToken c) => catalog.UpsertMetadataAsync(r, c);
    /// <inheritdoc />
    public Task Handle(DeleteProductMetadataCommand r, CancellationToken c) => catalog.DeleteMetadataAsync(r.ProductId, r.Key, c);

    private async Task<ProductModel?> ChangeStatus(Guid id, ProductStatus status, CancellationToken cancellationToken)
    {
        var current = await catalog.GetProductAsync(id, cancellationToken);
        if (current is null) return null;
        if (current.Status == ProductStatus.Archived && status == ProductStatus.Published) throw new ValidationException("An archived product cannot be published directly.");
        if (current.Status == status) return current;
        return await catalog.UpdateProductAsync(new UpdateProductCommand(current.Id, current.Name, current.Slug, current.Sku, current.ShortDescription, current.Description, status, current.ProductType, current.Price, current.CompareAtPrice, current.Currency, current.BrandId), cancellationToken);
    }
}

/// <summary>Validates product creation.</summary>
public sealed class ProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    /// <summary>Initializes product rules.</summary>
    public ProductCommandValidator() { RuleFor(x => x.Name).NotEmpty().MaximumLength(200); RuleFor(x => x.Slug).NotEmpty().Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$").MaximumLength(200); RuleFor(x => x.Sku).MaximumLength(100).When(x => x.Sku is not null); RuleFor(x => x.Price).GreaterThanOrEqualTo(0); RuleFor(x => x.CompareAtPrice).GreaterThanOrEqualTo(0).When(x => x.CompareAtPrice.HasValue); RuleFor(x => x.Currency).Length(3); }
}
/// <summary>Validates product updates.</summary>
public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    /// <summary>Initializes product update rules.</summary>
    public UpdateProductCommandValidator() { RuleFor(x => x.Id).NotEmpty(); RuleFor(x => x.Name).NotEmpty().MaximumLength(200); RuleFor(x => x.Slug).NotEmpty().Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$").MaximumLength(200); RuleFor(x => x.Price).GreaterThanOrEqualTo(0); RuleFor(x => x.CompareAtPrice).GreaterThanOrEqualTo(0).When(x => x.CompareAtPrice.HasValue); RuleFor(x => x.Currency).Length(3); }
}
/// <summary>Validates category creation.</summary>
public sealed class CategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    /// <summary>Initializes category creation rules.</summary>
    public CategoryCommandValidator() { RuleFor(x => x.Name).NotEmpty().MaximumLength(200); RuleFor(x => x.Slug).NotEmpty().Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$").MaximumLength(200); RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0); }
}
/// <summary>Validates category updates.</summary>
public sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    /// <summary>Initializes category update rules.</summary>
    public UpdateCategoryCommandValidator() { RuleFor(x => x.Id).NotEmpty(); RuleFor(x => x.Name).NotEmpty().MaximumLength(200); RuleFor(x => x.Slug).NotEmpty().Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$").MaximumLength(200); RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0); }
}
/// <summary>Validates brand creation.</summary>
public sealed class CreateBrandCommandValidator : AbstractValidator<CreateBrandCommand>
{
    /// <summary>Initializes brand creation rules.</summary>
    public CreateBrandCommandValidator() { RuleFor(x => x.Name).NotEmpty().MaximumLength(200); RuleFor(x => x.Slug).NotEmpty().Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$").MaximumLength(200); }
}
/// <summary>Validates brand updates.</summary>
public sealed class UpdateBrandCommandValidator : AbstractValidator<UpdateBrandCommand>
{
    /// <summary>Initializes brand update rules.</summary>
    public UpdateBrandCommandValidator() { RuleFor(x => x.Id).NotEmpty(); RuleFor(x => x.Name).NotEmpty().MaximumLength(200); RuleFor(x => x.Slug).NotEmpty().Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$").MaximumLength(200); }
}
/// <summary>Validates tag creation.</summary>
public sealed class CreateTagCommandValidator : AbstractValidator<CreateTagCommand>
{
    /// <summary>Initializes tag creation rules.</summary>
    public CreateTagCommandValidator() { RuleFor(x => x.Name).NotEmpty().MaximumLength(200); RuleFor(x => x.Slug).NotEmpty().Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$").MaximumLength(200); }
}
/// <summary>Validates tag updates.</summary>
public sealed class UpdateTagCommandValidator : AbstractValidator<UpdateTagCommand>
{
    /// <summary>Initializes tag update rules.</summary>
    public UpdateTagCommandValidator() { RuleFor(x => x.Id).NotEmpty(); RuleFor(x => x.Name).NotEmpty().MaximumLength(200); RuleFor(x => x.Slug).NotEmpty().Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$").MaximumLength(200); }
}
/// <summary>Validates variant creation.</summary>
public sealed class CreateProductVariantCommandValidator : AbstractValidator<CreateProductVariantCommand>
{
    /// <summary>Initializes variant rules.</summary>
    public CreateProductVariantCommandValidator() { RuleFor(x => x.ProductId).NotEmpty(); RuleFor(x => x.Sku).NotEmpty().MaximumLength(100); RuleFor(x => x.Price).GreaterThanOrEqualTo(0); RuleFor(x => x.CompareAtPrice).GreaterThanOrEqualTo(0).When(x => x.CompareAtPrice.HasValue); RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0); }
}
/// <summary>Validates variant updates.</summary>
public sealed class UpdateProductVariantCommandValidator : AbstractValidator<UpdateProductVariantCommand>
{
    /// <summary>Initializes variant update rules.</summary>
    public UpdateProductVariantCommandValidator() { RuleFor(x => x.ProductId).NotEmpty(); RuleFor(x => x.Id).NotEmpty(); RuleFor(x => x.Sku).NotEmpty().MaximumLength(100); RuleFor(x => x.Price).GreaterThanOrEqualTo(0); RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0); }
}
/// <summary>Validates product metadata.</summary>
public sealed class ProductMetadataValidator : AbstractValidator<UpsertProductMetadataCommand>
{
    /// <summary>Initializes metadata rules and rejects secret-like keys.</summary>
    public ProductMetadataValidator() { RuleFor(x => x.ProductId).NotEmpty(); RuleFor(x => x.Key).NotEmpty().MaximumLength(200).Must(x => !x.Contains("password", StringComparison.OrdinalIgnoreCase) && !x.Contains("secret", StringComparison.OrdinalIgnoreCase) && !x.Contains("token", StringComparison.OrdinalIgnoreCase) && !x.Contains("connectionstring", StringComparison.OrdinalIgnoreCase)); RuleFor(x => x.Value).NotEmpty().MaximumLength(10000); }
}

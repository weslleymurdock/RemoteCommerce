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
    /// <inheritdoc /> public Task<ProductModel> Handle(CreateProductCommand r, CancellationToken c) => catalog.CreateProductAsync(r, c);
    /// <inheritdoc /> public Task<ProductModel?> Handle(UpdateProductCommand r, CancellationToken c) => catalog.UpdateProductAsync(r, c);
    /// <inheritdoc /> public Task Handle(DeleteProductCommand r, CancellationToken c) => catalog.DeleteProductAsync(r.Id, c);
    /// <inheritdoc /> public async Task<ProductModel?> Handle(PublishProductCommand r, CancellationToken c) => await ChangeStatus(r.Id, ProductStatus.Published, c);
    /// <inheritdoc /> public async Task<ProductModel?> Handle(ArchiveProductCommand r, CancellationToken c) => await ChangeStatus(r.Id, ProductStatus.Archived, c);
    /// <inheritdoc /> public Task<PagedResult<ProductModel>> Handle(ProductListQuery r, CancellationToken c) => catalog.ListProductsAsync(r, c);
    /// <inheritdoc /> public Task<CategoryModel> Handle(CreateCategoryCommand r, CancellationToken c) => catalog.CreateCategoryAsync(r, c);
    /// <inheritdoc /> public Task<CategoryModel?> Handle(UpdateCategoryCommand r, CancellationToken c) => catalog.UpdateCategoryAsync(r, c);
    /// <inheritdoc /> public Task Handle(DeleteCategoryCommand r, CancellationToken c) => catalog.DeleteCategoryAsync(r.Id, c);
    /// <inheritdoc /> public Task<BrandModel> Handle(CreateBrandCommand r, CancellationToken c) => catalog.CreateBrandAsync(r, c);
    /// <inheritdoc /> public Task<BrandModel?> Handle(UpdateBrandCommand r, CancellationToken c) => catalog.UpdateBrandAsync(r, c);
    /// <inheritdoc /> public Task Handle(DeleteBrandCommand r, CancellationToken c) => catalog.DeleteBrandAsync(r.Id, c);
    /// <inheritdoc /> public Task<TagModel> Handle(CreateTagCommand r, CancellationToken c) => catalog.CreateTagAsync(r, c);
    /// <inheritdoc /> public Task<TagModel?> Handle(UpdateTagCommand r, CancellationToken c) => catalog.UpdateTagAsync(r, c);
    /// <inheritdoc /> public Task Handle(DeleteTagCommand r, CancellationToken c) => catalog.DeleteTagAsync(r.Id, c);
    /// <inheritdoc /> public Task<ProductVariantModel> Handle(CreateProductVariantCommand r, CancellationToken c) => catalog.CreateVariantAsync(r, c);
    /// <inheritdoc /> public Task<ProductVariantModel?> Handle(UpdateProductVariantCommand r, CancellationToken c) => catalog.UpdateVariantAsync(r, c);
    /// <inheritdoc /> public Task Handle(DeleteProductVariantCommand r, CancellationToken c) => catalog.DeleteVariantAsync(r.ProductId, r.Id, c);
    /// <inheritdoc /> public Task<IReadOnlyList<ProductVariantModel>> Handle(ProductVariantListQuery r, CancellationToken c) => catalog.GetVariantsAsync(r.ProductId, c);
    /// <inheritdoc /> public Task<IReadOnlyList<ProductMetadataModel>> Handle(ProductMetadataQuery r, CancellationToken c) => catalog.GetMetadataAsync(r.ProductId, c);
    /// <inheritdoc /> public Task<ProductMetadataModel> Handle(UpsertProductMetadataCommand r, CancellationToken c) => catalog.UpsertMetadataAsync(r, c);
    /// <inheritdoc /> public Task Handle(DeleteProductMetadataCommand r, CancellationToken c) => catalog.DeleteMetadataAsync(r.ProductId, r.Key, c);

    private async Task<ProductModel?> ChangeStatus(Guid id, ProductStatus status, CancellationToken cancellationToken)
    {
        var current = await catalog.GetProductAsync(id, cancellationToken);
        return current is null ? null : await catalog.UpdateProductAsync(new UpdateProductCommand(current.Id, current.Name, current.Slug, current.Sku, current.ShortDescription, current.Description, status, current.ProductType, current.Price, current.CompareAtPrice, current.Currency, current.BrandId), cancellationToken);
    }
}

/// <summary>Validates catalog input.</summary>
public sealed class CatalogCommandValidator : AbstractValidator<CreateProductCommand>, IValidator<UpdateProductCommand>, IValidator<CreateCategoryCommand>, IValidator<UpdateCategoryCommand>, IValidator<CreateBrandCommand>, IValidator<UpdateBrandCommand>, IValidator<CreateTagCommand>, IValidator<UpdateTagCommand>, IValidator<CreateProductVariantCommand>, IValidator<UpdateProductVariantCommand>, IValidator<UpsertProductMetadataCommand>
{
    /// <summary>Initializes the catalog validators.</summary>
    public CatalogCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$").MaximumLength(200);
    }

    Task<ValidationResult> IValidator<UpdateProductCommand>.ValidateAsync(UpdateProductCommand instance, CancellationToken cancellationToken) => ValidateProduct(instance.Name, instance.Slug, instance.Price, instance.Currency);
    Task<ValidationResult> IValidator<CreateCategoryCommand>.ValidateAsync(CreateCategoryCommand instance, CancellationToken cancellationToken) => ValidateNameSlug(instance.Name, instance.Slug);
    Task<ValidationResult> IValidator<UpdateCategoryCommand>.ValidateAsync(UpdateCategoryCommand instance, CancellationToken cancellationToken) => ValidateNameSlug(instance.Name, instance.Slug);
    Task<ValidationResult> IValidator<CreateBrandCommand>.ValidateAsync(CreateBrandCommand instance, CancellationToken cancellationToken) => ValidateNameSlug(instance.Name, instance.Slug);
    Task<ValidationResult> IValidator<UpdateBrandCommand>.ValidateAsync(UpdateBrandCommand instance, CancellationToken cancellationToken) => ValidateNameSlug(instance.Name, instance.Slug);
    Task<ValidationResult> IValidator<CreateTagCommand>.ValidateAsync(CreateTagCommand instance, CancellationToken cancellationToken) => ValidateNameSlug(instance.Name, instance.Slug);
    Task<ValidationResult> IValidator<UpdateTagCommand>.ValidateAsync(UpdateTagCommand instance, CancellationToken cancellationToken) => ValidateNameSlug(instance.Name, instance.Slug);
    Task<ValidationResult> IValidator<CreateProductVariantCommand>.ValidateAsync(CreateProductVariantCommand instance, CancellationToken cancellationToken) => ValidateVariant(instance.Sku, instance.Price, instance.CompareAtPrice);
    Task<ValidationResult> IValidator<UpdateProductVariantCommand>.ValidateAsync(UpdateProductVariantCommand instance, CancellationToken cancellationToken) => ValidateVariant(instance.Sku, instance.Price, instance.CompareAtPrice);
    Task<ValidationResult> IValidator<UpsertProductMetadataCommand>.ValidateAsync(UpsertProductMetadataCommand instance, CancellationToken cancellationToken) => ValidateMetadata(instance.Key, instance.Value);

    private static Task<ValidationResult> ValidateProduct(string name, string slug, decimal price, string currency) => new(new InlineValidator<UpdateProductCommand> { });
    private static Task<ValidationResult> ValidateNameSlug(string name, string slug) => Task.FromResult(new ValidationResult(string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(slug) ? [new ValidationFailure("Name", "Name and slug are required.")] : []));
    private static Task<ValidationResult> ValidateVariant(string sku, decimal price, decimal? compareAtPrice) => Task.FromResult(new ValidationResult(string.IsNullOrWhiteSpace(sku) || price < 0 || compareAtPrice < 0 ? [new ValidationFailure("Variant", "SKU and non-negative prices are required.")] : []));
    private static Task<ValidationResult> ValidateMetadata(string key, string value) => Task.FromResult(new ValidationResult(string.IsNullOrWhiteSpace(key) || key.Length > 200 || value.Length > 10000 ? [new ValidationFailure("Metadata", "Metadata key/value is invalid.")] : []));
}

/// <summary>Validates product creation.</summary>
public sealed class ProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    /// <summary>Initializes product rules.</summary>
    public ProductCommandValidator() { RuleFor(x => x.Name).NotEmpty().MaximumLength(200); RuleFor(x => x.Slug).NotEmpty().Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$").MaximumLength(200); RuleFor(x => x.Sku).MaximumLength(100).When(x => x.Sku is not null); RuleFor(x => x.Price).GreaterThanOrEqualTo(0); RuleFor(x => x.CompareAtPrice).GreaterThanOrEqualTo(0).When(x => x.CompareAtPrice.HasValue); RuleFor(x => x.Currency).Length(3); }
}

/// <summary>Validates metadata keys against reserved secret names.</summary>
public sealed class ProductMetadataValidator : AbstractValidator<UpsertProductMetadataCommand>
{
    /// <summary>Initializes metadata rules.</summary>
    public ProductMetadataValidator() { RuleFor(x => x.Key).NotEmpty().MaximumLength(200).Must(x => !x.Contains("password", StringComparison.OrdinalIgnoreCase) && !x.Contains("secret", StringComparison.OrdinalIgnoreCase) && !x.Contains("token", StringComparison.OrdinalIgnoreCase) && !x.Contains("connectionstring", StringComparison.OrdinalIgnoreCase)); RuleFor(x => x.Value).NotEmpty().MaximumLength(10000); }
}

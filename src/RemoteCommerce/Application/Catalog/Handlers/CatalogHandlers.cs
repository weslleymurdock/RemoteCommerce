namespace RemoteCommerce.Application.Catalog.Handlers;

/// <summary>Handles catalog requests through the catalog application service.</summary>
public sealed class CatalogHandlers(ICatalogService catalog) : IRequestHandler<CreateProductCommand, ProductModel>, IRequestHandler<UpdateProductCommand, ProductModel?>, IRequestHandler<DeleteProductCommand>, IRequestHandler<PublishProductCommand, ProductModel?>, IRequestHandler<ArchiveProductCommand, ProductModel?>, IRequestHandler<ProductListQuery, PagedResult<ProductModel>>, IRequestHandler<CreateCategoryCommand, CategoryModel>, IRequestHandler<CreateBrandCommand, BrandModel>, IRequestHandler<CreateTagCommand, TagModel>
{
    /// <inheritdoc />
    public Task<ProductModel> Handle(CreateProductCommand request, CancellationToken cancellationToken) => catalog.CreateProductAsync(request, cancellationToken);
    /// <inheritdoc />
    public Task<ProductModel?> Handle(UpdateProductCommand request, CancellationToken cancellationToken) => catalog.UpdateProductAsync(request, cancellationToken);
    /// <inheritdoc />
    public Task Handle(DeleteProductCommand request, CancellationToken cancellationToken) => catalog.DeleteProductAsync(request.Id, cancellationToken);
    /// <inheritdoc />
    public async Task<ProductModel?> Handle(PublishProductCommand request, CancellationToken cancellationToken) => await ChangeStatus(request.Id, ProductStatus.Published, cancellationToken);
    /// <inheritdoc />
    public async Task<ProductModel?> Handle(ArchiveProductCommand request, CancellationToken cancellationToken) => await ChangeStatus(request.Id, ProductStatus.Archived, cancellationToken);
    /// <inheritdoc />
    public Task<PagedResult<ProductModel>> Handle(ProductListQuery request, CancellationToken cancellationToken) => catalog.ListProductsAsync(request, cancellationToken);
    /// <inheritdoc />
    public Task<CategoryModel> Handle(CreateCategoryCommand request, CancellationToken cancellationToken) => catalog.CreateCategoryAsync(request, cancellationToken);
    /// <inheritdoc />
    public Task<BrandModel> Handle(CreateBrandCommand request, CancellationToken cancellationToken) => catalog.CreateBrandAsync(request, cancellationToken);
    /// <inheritdoc />
    public Task<TagModel> Handle(CreateTagCommand request, CancellationToken cancellationToken) => catalog.CreateTagAsync(request, cancellationToken);

    private async Task<ProductModel?> ChangeStatus(Guid id, ProductStatus status, CancellationToken cancellationToken)
    {
        var current = await catalog.GetProductAsync(id, cancellationToken);
        return current is null ? null : await catalog.UpdateProductAsync(new UpdateProductCommand(current.Id, current.Name, current.Slug, current.Sku, current.ShortDescription, current.Description, status, current.ProductType, current.Price, current.CompareAtPrice, current.Currency, current.BrandId), cancellationToken);
    }
}

/// <summary>Validates product creation input.</summary>
public sealed class ProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    /// <summary>Initializes validation rules.</summary>
    public ProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$").MaximumLength(200);
        RuleFor(x => x.Sku).MaximumLength(100).When(x => x.Sku is not null);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CompareAtPrice).GreaterThanOrEqualTo(0).When(x => x.CompareAtPrice.HasValue);
        RuleFor(x => x.Currency).Length(3);
    }
}

/// <summary>Validates product updates.</summary>
public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    /// <summary>Initializes validation rules.</summary>
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$").MaximumLength(200);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Currency).Length(3);
    }
}

/// <summary>Validates category creation.</summary>
public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    /// <summary>Initializes validation rules.</summary>
    public CreateCategoryCommandValidator() { RuleFor(x => x.Name).NotEmpty().MaximumLength(200); RuleFor(x => x.Slug).NotEmpty().Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$").MaximumLength(200); RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0); }
}

/// <summary>Validates brand creation.</summary>
public sealed class CreateBrandCommandValidator : AbstractValidator<CreateBrandCommand>
{
    /// <summary>Initializes validation rules.</summary>
    public CreateBrandCommandValidator() { RuleFor(x => x.Name).NotEmpty().MaximumLength(200); RuleFor(x => x.Slug).NotEmpty().Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$").MaximumLength(200); }
}

/// <summary>Validates tag creation.</summary>
public sealed class CreateTagCommandValidator : AbstractValidator<CreateTagCommand>
{
    /// <summary>Initializes validation rules.</summary>
    public CreateTagCommandValidator() { RuleFor(x => x.Name).NotEmpty().MaximumLength(200); RuleFor(x => x.Slug).NotEmpty().Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$").MaximumLength(200); }
}

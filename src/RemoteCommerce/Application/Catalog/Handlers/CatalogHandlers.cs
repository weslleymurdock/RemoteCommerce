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
    IRequestHandler<UpsertProductMetadataCommand, ProductMetadataModel>, IRequestHandler<DeleteProductMetadataCommand>, IRequestHandler<GetProductQuery, ProductModel?>,
    IRequestHandler<GetCategoriesQuery, IReadOnlyList<CategoryModel>>, IRequestHandler<GetBrandsQuery, IReadOnlyList<BrandModel>>, IRequestHandler<GetTagsQuery, IReadOnlyList<TagModel>>, IRequestHandler<GetAttributesQuery, IReadOnlyList<AttributeModel>>
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
    /// <inheritdoc />
    public Task<ProductModel?> Handle(GetProductQuery request, CancellationToken cancellationToken) => catalog.GetProductAsync(request.Id, cancellationToken);
    /// <inheritdoc />
    public Task<IReadOnlyList<CategoryModel>> Handle(GetCategoriesQuery r, CancellationToken c) => catalog.GetCategoriesAsync(c);
    /// <inheritdoc />
    public Task<IReadOnlyList<BrandModel>> Handle(GetBrandsQuery r, CancellationToken c) => catalog.GetBrandsAsync(c);
    /// <inheritdoc />
    public Task<IReadOnlyList<TagModel>> Handle(GetTagsQuery r, CancellationToken c) => catalog.GetTagsAsync(c);
    /// <inheritdoc />
    public Task<IReadOnlyList<AttributeModel>> Handle(GetAttributesQuery r, CancellationToken c) => catalog.GetAttributesAsync(c);
    private async Task<ProductModel?> ChangeStatus(Guid id, ProductStatus status, CancellationToken cancellationToken)
    {
        var current = await catalog.GetProductAsync(id, cancellationToken);
        if (current is null) return null;
        if (current.Status == ProductStatus.Archived && status == ProductStatus.Published) throw new ValidationException("An archived product cannot be published directly.");
        if (current.Status == status) return current;
        return await catalog.UpdateProductAsync(new UpdateProductCommand(current.Id, current.Name, current.Slug, current.Sku, current.ShortDescription, current.Description, status, current.ProductType, current.Price, current.CompareAtPrice, current.Currency, current.BrandId), cancellationToken);
    }
}

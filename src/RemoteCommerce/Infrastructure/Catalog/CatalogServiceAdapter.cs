namespace RemoteCommerce.Infrastructure.Catalog;

/// <summary>Adapts transport-independent catalog requests to the legacy catalog persistence service.</summary>
public sealed class CatalogServiceAdapter(CatalogService inner) : ICatalogService
{
    /// <inheritdoc />
    public Task<ProductModel> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken)
    {
        return inner.CreateProductAsync(new CreateProductCommand(request), cancellationToken);
    }

    /// <inheritdoc />
    public Task<ProductModel?> UpdateProductAsync(UpdateProductRequest request, CancellationToken cancellationToken)
    {
        return inner.UpdateProductAsync(new UpdateProductCommand(request), cancellationToken);
    }

    /// <inheritdoc />
    public Task DeleteProductAsync(Guid id, CancellationToken cancellationToken)
    {
        return inner.DeleteProductAsync(id, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ProductModel?> GetProductAsync(Guid id, CancellationToken cancellationToken)
    {
        return inner.GetProductAsync(id, cancellationToken);
    }

    /// <inheritdoc />
    public Task<PagedResult<ProductModel>> ListProductsAsync(ProductListRequest request, CancellationToken cancellationToken)
    {
        return inner.ListProductsAsync(new ProductListQuery(request), cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<CategoryModel>> GetCategoriesAsync(CancellationToken cancellationToken)
    {
        return inner.GetCategoriesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<BrandModel>> GetBrandsAsync(CancellationToken cancellationToken)
    {
        return inner.GetBrandsAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<TagModel>> GetTagsAsync(CancellationToken cancellationToken)
    {
        return inner.GetTagsAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<AttributeModel>> GetAttributesAsync(CancellationToken cancellationToken)
    {
        return inner.GetAttributesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<CategoryModel> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        return inner.CreateCategoryAsync(new CreateCategoryCommand(request), cancellationToken);
    }

    /// <inheritdoc />
    public Task<CategoryModel?> UpdateCategoryAsync(UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        return inner.UpdateCategoryAsync(new UpdateCategoryCommand(request), cancellationToken);
    }

    /// <inheritdoc />
    public Task DeleteCategoryAsync(Guid id, CancellationToken cancellationToken)
    {
        return inner.DeleteCategoryAsync(id, cancellationToken);
    }

    /// <inheritdoc />
    public Task<BrandModel> CreateBrandAsync(CreateBrandRequest request, CancellationToken cancellationToken)
    {
        return inner.CreateBrandAsync(new CreateBrandCommand(request), cancellationToken);
    }

    /// <inheritdoc />
    public Task<BrandModel?> UpdateBrandAsync(UpdateBrandRequest request, CancellationToken cancellationToken)
    {
        return inner.UpdateBrandAsync(new UpdateBrandCommand(request), cancellationToken);
    }

    /// <inheritdoc />
    public Task DeleteBrandAsync(Guid id, CancellationToken cancellationToken)
    {
        return inner.DeleteBrandAsync(id, cancellationToken);
    }

    /// <inheritdoc />
    public Task<TagModel> CreateTagAsync(CreateTagRequest request, CancellationToken cancellationToken)
    {
        return inner.CreateTagAsync(new CreateTagCommand(request), cancellationToken);
    }

    /// <inheritdoc />
    public Task<TagModel?> UpdateTagAsync(UpdateTagRequest request, CancellationToken cancellationToken)
    {
        return inner.UpdateTagAsync(new UpdateTagCommand(request), cancellationToken);
    }

    /// <inheritdoc />
    public Task DeleteTagAsync(Guid id, CancellationToken cancellationToken)
    {
        return inner.DeleteTagAsync(id, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ProductVariantModel> CreateVariantAsync(CreateProductVariantRequest request, CancellationToken cancellationToken)
    {
        return inner.CreateVariantAsync(new CreateProductVariantCommand(request), cancellationToken);
    }

    /// <inheritdoc />
    public Task<ProductVariantModel?> UpdateVariantAsync(UpdateProductVariantRequest request, CancellationToken cancellationToken)
    {
        return inner.UpdateVariantAsync(new UpdateProductVariantCommand(request), cancellationToken);
    }

    /// <inheritdoc />
    public Task DeleteVariantAsync(Guid productId, Guid variantId, CancellationToken cancellationToken)
    {
        return inner.DeleteVariantAsync(productId, variantId, cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<ProductVariantModel>> GetVariantsAsync(Guid productId, CancellationToken cancellationToken)
    {
        return inner.GetVariantsAsync(productId, cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<ProductMetadataModel>> GetMetadataAsync(Guid productId, CancellationToken cancellationToken)
    {
        return inner.GetMetadataAsync(productId, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ProductMetadataModel> UpsertMetadataAsync(UpsertProductMetadataRequest request, CancellationToken cancellationToken)
    {
        return inner.UpsertMetadataAsync(new UpsertProductMetadataCommand(request), cancellationToken);
    }

    /// <inheritdoc />
    public Task DeleteMetadataAsync(Guid productId, string key, CancellationToken cancellationToken)
    {
        return inner.DeleteMetadataAsync(productId, key, cancellationToken);
    }
}

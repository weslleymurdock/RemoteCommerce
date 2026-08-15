namespace RemoteCommerce.Application.Catalog.Abstractions;

/// <summary>Defines the application boundary for catalog use cases.</summary>
public interface ICatalogService
{
    /// <summary>Creates a product.</summary>
    /// <param name="request">The product data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created product.</returns>
    Task<ProductModel> CreateProductAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken);

    /// <summary>Updates a product.</summary>
    /// <param name="request">The product data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated product, or null.</returns>
    Task<ProductModel?> UpdateProductAsync(
        UpdateProductRequest request,
        CancellationToken cancellationToken);

    /// <summary>Soft-deletes a product.</summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task DeleteProductAsync(
        Guid id,
        CancellationToken cancellationToken);

    /// <summary>Gets a product.</summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The product, or null.</returns>
    Task<ProductModel?> GetProductAsync(
        Guid id,
        CancellationToken cancellationToken);

    /// <summary>Lists products.</summary>
    /// <param name="request">The filters and paging options.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A bounded page.</returns>
    Task<PagedResult<ProductModel>> ListProductsAsync(
        ProductListRequest request,
        CancellationToken cancellationToken);

    /// <summary>Gets categories.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The categories.</returns>
    Task<IReadOnlyList<CategoryModel>> GetCategoriesAsync(
        CancellationToken cancellationToken);

    /// <summary>Gets brands.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The brands.</returns>
    Task<IReadOnlyList<BrandModel>> GetBrandsAsync(
        CancellationToken cancellationToken);

    /// <summary>Gets tags.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The tags.</returns>
    Task<IReadOnlyList<TagModel>> GetTagsAsync(
        CancellationToken cancellationToken);

    /// <summary>Gets attributes.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The attributes.</returns>
    Task<IReadOnlyList<AttributeModel>> GetAttributesAsync(
        CancellationToken cancellationToken);

    /// <summary>Creates a category.</summary>
    /// <param name="request">The category data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created category.</returns>
    Task<CategoryModel> CreateCategoryAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken);

    /// <summary>Updates a category.</summary>
    /// <param name="request">The category data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated category, or null.</returns>
    Task<CategoryModel?> UpdateCategoryAsync(
        UpdateCategoryRequest request,
        CancellationToken cancellationToken);

    /// <summary>Soft-deletes a category.</summary>
    /// <param name="id">The category identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task DeleteCategoryAsync(
        Guid id,
        CancellationToken cancellationToken);

    /// <summary>Creates a brand.</summary>
    /// <param name="request">The brand data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created brand.</returns>
    Task<BrandModel> CreateBrandAsync(
        CreateBrandRequest request,
        CancellationToken cancellationToken);

    /// <summary>Updates a brand.</summary>
    /// <param name="request">The brand data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated brand, or null.</returns>
    Task<BrandModel?> UpdateBrandAsync(
        UpdateBrandRequest request,
        CancellationToken cancellationToken);

    /// <summary>Soft-deletes a brand.</summary>
    /// <param name="id">The brand identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task DeleteBrandAsync(
        Guid id,
        CancellationToken cancellationToken);

    /// <summary>Creates a tag.</summary>
    /// <param name="request">The tag data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created tag.</returns>
    Task<TagModel> CreateTagAsync(
        CreateTagRequest request,
        CancellationToken cancellationToken);

    /// <summary>Updates a tag.</summary>
    /// <param name="request">The tag data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated tag, or null.</returns>
    Task<TagModel?> UpdateTagAsync(
        UpdateTagRequest request,
        CancellationToken cancellationToken);

    /// <summary>Soft-deletes a tag.</summary>
    /// <param name="id">The tag identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task DeleteTagAsync(
        Guid id,
        CancellationToken cancellationToken);

    /// <summary>Creates a product variant.</summary>
    /// <param name="request">The variant data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created variant.</returns>
    Task<ProductVariantModel> CreateVariantAsync(
        CreateProductVariantRequest request,
        CancellationToken cancellationToken);

    /// <summary>Updates a product variant.</summary>
    /// <param name="request">The variant data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated variant, or null.</returns>
    Task<ProductVariantModel?> UpdateVariantAsync(
        UpdateProductVariantRequest request,
        CancellationToken cancellationToken);

    /// <summary>Soft-deletes a product variant.</summary>
    /// <param name="productId">The owning product.</param>
    /// <param name="variantId">The variant identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task DeleteVariantAsync(
        Guid productId,
        Guid variantId,
        CancellationToken cancellationToken);

    /// <summary>Gets variants for a product.</summary>
    /// <param name="productId">The product identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The product variants.</returns>
    Task<IReadOnlyList<ProductVariantModel>> GetVariantsAsync(
        Guid productId,
        CancellationToken cancellationToken);

    /// <summary>Gets product metadata.</summary>
    /// <param name="productId">The product identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The metadata records.</returns>
    Task<IReadOnlyList<ProductMetadataModel>> GetMetadataAsync(
        Guid productId,
        CancellationToken cancellationToken);

    /// <summary>Creates or replaces product metadata.</summary>
    /// <param name="request">The metadata data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The metadata record.</returns>
    Task<ProductMetadataModel> UpsertMetadataAsync(
        UpsertProductMetadataRequest request,
        CancellationToken cancellationToken);

    /// <summary>Deletes product metadata.</summary>
    /// <param name="productId">The product identifier.</param>
    /// <param name="key">The metadata key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task DeleteMetadataAsync(
        Guid productId,
        string key,
        CancellationToken cancellationToken);
}

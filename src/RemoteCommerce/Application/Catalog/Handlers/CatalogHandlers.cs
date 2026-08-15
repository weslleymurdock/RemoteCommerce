namespace RemoteCommerce.Application.Catalog.Handlers;

/// <summary>Handles catalog commands and queries through the catalog application service.</summary>
public sealed class CatalogHandlers(ICatalogService catalog) :
    IRequestHandler<CreateProductCommand, Result<ProductModel>>,
    IRequestHandler<UpdateProductCommand, Result<ProductModel>>,
    IRequestHandler<DeleteProductCommand, Result>,
    IRequestHandler<PublishProductCommand, Result<ProductModel>>,
    IRequestHandler<ArchiveProductCommand, Result<ProductModel>>,
    IRequestHandler<ProductListQuery, Result<PagedResult<ProductModel>>>,
    IRequestHandler<GetProductQuery, Result<ProductModel>>,
    IRequestHandler<CreateCategoryCommand, Result<CategoryModel>>,
    IRequestHandler<UpdateCategoryCommand, Result<CategoryModel>>,
    IRequestHandler<DeleteCategoryCommand, Result>,
    IRequestHandler<GetCategoriesQuery, Result<IReadOnlyList<CategoryModel>>>,
    IRequestHandler<GetCategoryQuery, Result<CategoryModel>>,
    IRequestHandler<CreateBrandCommand, Result<BrandModel>>,
    IRequestHandler<UpdateBrandCommand, Result<BrandModel>>,
    IRequestHandler<DeleteBrandCommand, Result>,
    IRequestHandler<GetBrandsQuery, Result<IReadOnlyList<BrandModel>>>,
    IRequestHandler<GetBrandQuery, Result<BrandModel>>,
    IRequestHandler<CreateTagCommand, Result<TagModel>>,
    IRequestHandler<UpdateTagCommand, Result<TagModel>>,
    IRequestHandler<DeleteTagCommand, Result>,
    IRequestHandler<GetTagsQuery, Result<IReadOnlyList<TagModel>>>,
    IRequestHandler<GetTagQuery, Result<TagModel>>,
    IRequestHandler<GetAttributesQuery, Result<IReadOnlyList<AttributeModel>>>,
    IRequestHandler<CreateProductVariantCommand, Result<ProductVariantModel>>,
    IRequestHandler<UpdateProductVariantCommand, Result<ProductVariantModel>>,
    IRequestHandler<DeleteProductVariantCommand, Result>,
    IRequestHandler<ProductVariantListQuery, Result<IReadOnlyList<ProductVariantModel>>>,
    IRequestHandler<GetProductVariantQuery, Result<ProductVariantModel>>,
    IRequestHandler<ProductMetadataQuery, Result<IReadOnlyList<ProductMetadataModel>>>,
    IRequestHandler<UpsertProductMetadataCommand, Result<ProductMetadataModel>>,
    IRequestHandler<DeleteProductMetadataCommand, Result>
{
    /// <inheritdoc />
    public async Task<Result<ProductModel>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var value = await catalog.CreateProductAsync(request, cancellationToken);
        return Result<ProductModel>.Success(value, StatusCodes.Status201Created);
    }

    /// <inheritdoc />
    public async Task<Result<ProductModel>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var value = await catalog.UpdateProductAsync(request, cancellationToken);
        return value is null
            ? Result<ProductModel>.Failure(StatusCodes.Status404NotFound, "product_not_found", "The product was not found.")
            : Result<ProductModel>.Success(value);
    }

    /// <inheritdoc />
    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        await catalog.DeleteProductAsync(request.Request.Id, cancellationToken);
        return Result.Success(StatusCodes.Status204NoContent);
    }

    /// <inheritdoc />
    public async Task<Result<ProductModel>> Handle(PublishProductCommand request, CancellationToken cancellationToken)
    {
        return await ChangeStatusAsync(request.Request.Id, ProductStatus.Published, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result<ProductModel>> Handle(ArchiveProductCommand request, CancellationToken cancellationToken)
    {
        return await ChangeStatusAsync(request.Request.Id, ProductStatus.Archived, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result<PagedResult<ProductModel>>> Handle(ProductListQuery request, CancellationToken cancellationToken)
    {
        var value = await catalog.ListProductsAsync(request, cancellationToken);
        return Result<PagedResult<ProductModel>>.Success(value);
    }

    /// <inheritdoc />
    public async Task<Result<ProductModel>> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        var value = await catalog.GetProductAsync(request.Request.Id, cancellationToken);
        return value is null
            ? Result<ProductModel>.Failure(StatusCodes.Status404NotFound, "product_not_found", "The product was not found.")
            : Result<ProductModel>.Success(value);
    }

    /// <inheritdoc />
    public async Task<Result<CategoryModel>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var value = await catalog.CreateCategoryAsync(request, cancellationToken);
        return Result<CategoryModel>.Success(value, StatusCodes.Status201Created);
    }

    /// <inheritdoc />
    public async Task<Result<CategoryModel>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var value = await catalog.UpdateCategoryAsync(request, cancellationToken);
        return value is null
            ? Result<CategoryModel>.Failure(StatusCodes.Status404NotFound, "category_not_found", "The category was not found.")
            : Result<CategoryModel>.Success(value);
    }

    /// <inheritdoc />
    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        await catalog.DeleteCategoryAsync(request.Request.Id, cancellationToken);
        return Result.Success(StatusCodes.Status204NoContent);
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<CategoryModel>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var value = await catalog.GetCategoriesAsync(cancellationToken);
        return Result<IReadOnlyList<CategoryModel>>.Success(value);
    }

    /// <inheritdoc />
    public async Task<Result<CategoryModel>> Handle(GetCategoryQuery request, CancellationToken cancellationToken)
    {
        var values = await catalog.GetCategoriesAsync(cancellationToken);
        var value = values.FirstOrDefault(x => x.Id == request.Request.Id);
        return value is null
            ? Result<CategoryModel>.Failure(StatusCodes.Status404NotFound, "category_not_found", "The category was not found.")
            : Result<CategoryModel>.Success(value);
    }

    /// <inheritdoc />
    public async Task<Result<BrandModel>> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
    {
        var value = await catalog.CreateBrandAsync(request, cancellationToken);
        return Result<BrandModel>.Success(value, StatusCodes.Status201Created);
    }

    /// <inheritdoc />
    public async Task<Result<BrandModel>> Handle(UpdateBrandCommand request, CancellationToken cancellationToken)
    {
        var value = await catalog.UpdateBrandAsync(request, cancellationToken);
        return value is null
            ? Result<BrandModel>.Failure(StatusCodes.Status404NotFound, "brand_not_found", "The brand was not found.")
            : Result<BrandModel>.Success(value);
    }

    /// <inheritdoc />
    public async Task<Result> Handle(DeleteBrandCommand request, CancellationToken cancellationToken)
    {
        await catalog.DeleteBrandAsync(request.Request.Id, cancellationToken);
        return Result.Success(StatusCodes.Status204NoContent);
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<BrandModel>>> Handle(GetBrandsQuery request, CancellationToken cancellationToken)
    {
        var value = await catalog.GetBrandsAsync(cancellationToken);
        return Result<IReadOnlyList<BrandModel>>.Success(value);
    }

    /// <inheritdoc />
    public async Task<Result<BrandModel>> Handle(GetBrandQuery request, CancellationToken cancellationToken)
    {
        var values = await catalog.GetBrandsAsync(cancellationToken);
        var value = values.FirstOrDefault(x => x.Id == request.Request.Id);
        return value is null
            ? Result<BrandModel>.Failure(StatusCodes.Status404NotFound, "brand_not_found", "The brand was not found.")
            : Result<BrandModel>.Success(value);
    }

    /// <inheritdoc />
    public async Task<Result<TagModel>> Handle(CreateTagCommand request, CancellationToken cancellationToken)
    {
        var value = await catalog.CreateTagAsync(request, cancellationToken);
        return Result<TagModel>.Success(value, StatusCodes.Status201Created);
    }

    /// <inheritdoc />
    public async Task<Result<TagModel>> Handle(UpdateTagCommand request, CancellationToken cancellationToken)
    {
        var value = await catalog.UpdateTagAsync(request, cancellationToken);
        return value is null
            ? Result<TagModel>.Failure(StatusCodes.Status404NotFound, "tag_not_found", "The tag was not found.")
            : Result<TagModel>.Success(value);
    }

    /// <inheritdoc />
    public async Task<Result> Handle(DeleteTagCommand request, CancellationToken cancellationToken)
    {
        await catalog.DeleteTagAsync(request.Request.Id, cancellationToken);
        return Result.Success(StatusCodes.Status204NoContent);
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<TagModel>>> Handle(GetTagsQuery request, CancellationToken cancellationToken)
    {
        var value = await catalog.GetTagsAsync(cancellationToken);
        return Result<IReadOnlyList<TagModel>>.Success(value);
    }

    /// <inheritdoc />
    public async Task<Result<TagModel>> Handle(GetTagQuery request, CancellationToken cancellationToken)
    {
        var values = await catalog.GetTagsAsync(cancellationToken);
        var value = values.FirstOrDefault(x => x.Id == request.Request.Id);
        return value is null
            ? Result<TagModel>.Failure(StatusCodes.Status404NotFound, "tag_not_found", "The tag was not found.")
            : Result<TagModel>.Success(value);
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<AttributeModel>>> Handle(GetAttributesQuery request, CancellationToken cancellationToken)
    {
        var value = await catalog.GetAttributesAsync(cancellationToken);
        return Result<IReadOnlyList<AttributeModel>>.Success(value);
    }

    /// <inheritdoc />
    public async Task<Result<ProductVariantModel>> Handle(CreateProductVariantCommand request, CancellationToken cancellationToken)
    {
        var value = await catalog.CreateVariantAsync(request, cancellationToken);
        return Result<ProductVariantModel>.Success(value, StatusCodes.Status201Created);
    }

    /// <inheritdoc />
    public async Task<Result<ProductVariantModel>> Handle(UpdateProductVariantCommand request, CancellationToken cancellationToken)
    {
        var value = await catalog.UpdateVariantAsync(request, cancellationToken);
        return value is null
            ? Result<ProductVariantModel>.Failure(StatusCodes.Status404NotFound, "variation_not_found", "The product variation was not found.")
            : Result<ProductVariantModel>.Success(value);
    }

    /// <inheritdoc />
    public async Task<Result> Handle(DeleteProductVariantCommand request, CancellationToken cancellationToken)
    {
        await catalog.DeleteVariantAsync(request.Request.ProductId, request.Request.VariationId, cancellationToken);
        return Result.Success(StatusCodes.Status204NoContent);
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<ProductVariantModel>>> Handle(ProductVariantListQuery request, CancellationToken cancellationToken)
    {
        var value = await catalog.GetVariantsAsync(request.Request.Id, cancellationToken);
        return Result<IReadOnlyList<ProductVariantModel>>.Success(value);
    }

    /// <inheritdoc />
    public async Task<Result<ProductVariantModel>> Handle(GetProductVariantQuery request, CancellationToken cancellationToken)
    {
        var values = await catalog.GetVariantsAsync(request.Request.ProductId, cancellationToken);
        var value = values.FirstOrDefault(x => x.Id == request.Request.VariationId);
        return value is null
            ? Result<ProductVariantModel>.Failure(StatusCodes.Status404NotFound, "variation_not_found", "The product variation was not found.")
            : Result<ProductVariantModel>.Success(value);
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<ProductMetadataModel>>> Handle(ProductMetadataQuery request, CancellationToken cancellationToken)
    {
        var value = await catalog.GetMetadataAsync(request.Request.Id, cancellationToken);
        return Result<IReadOnlyList<ProductMetadataModel>>.Success(value);
    }

    /// <inheritdoc />
    public async Task<Result<ProductMetadataModel>> Handle(UpsertProductMetadataCommand request, CancellationToken cancellationToken)
    {
        var value = await catalog.UpsertMetadataAsync(request, cancellationToken);
        return Result<ProductMetadataModel>.Success(value, StatusCodes.Status200OK);
    }

    /// <inheritdoc />
    public async Task<Result> Handle(DeleteProductMetadataCommand request, CancellationToken cancellationToken)
    {
        await catalog.DeleteMetadataAsync(request.Request.ProductId, request.Request.Key, cancellationToken);
        return Result.Success(StatusCodes.Status204NoContent);
    }

    private async Task<Result<ProductModel>> ChangeStatusAsync(
        Guid id,
        ProductStatus status,
        CancellationToken cancellationToken)
    {
        var current = await catalog.GetProductAsync(id, cancellationToken);
        if (current is null)
        {
            return Result<ProductModel>.Failure(
                StatusCodes.Status404NotFound,
                "product_not_found",
                "The product was not found.");
        }

        if (current.Status == ProductStatus.Archived && status == ProductStatus.Published)
        {
            throw new ValidationException("An archived product cannot be published directly.");
        }

        if (current.Status == status)
        {
            return Result<ProductModel>.Success(current);
        }

        var request = new UpdateProductRequest
        {
            Id = current.Id,
            Name = current.Name,
            Slug = current.Slug,
            Sku = current.Sku,
            ShortDescription = current.ShortDescription,
            Description = current.Description,
            Status = status,
            ProductType = current.ProductType,
            Price = current.Price,
            CompareAtPrice = current.CompareAtPrice,
            Currency = current.Currency,
            BrandId = current.BrandId
        };

        var value = await catalog.UpdateProductAsync(new UpdateProductCommand(request), cancellationToken);
        return value is null
            ? Result<ProductModel>.Failure(StatusCodes.Status404NotFound, "product_not_found", "The product was not found.")
            : Result<ProductModel>.Success(value);
    }
}

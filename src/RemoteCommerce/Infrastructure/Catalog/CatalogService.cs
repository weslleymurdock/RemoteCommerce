namespace RemoteCommerce.Infrastructure.Catalog;

/// <summary>Provides catalog persistence through provider-independent repository abstractions.</summary>
public sealed class CatalogService(
    IRepository<Product> products,
    IRepository<Category> categories,
    IRepository<Brand> brands,
    IRepository<RemoteTag> tags,
    IRepository<ProductAttribute> attributes,
    IRepository<ProductVariant> variants,
    IRepository<ProductMetadata> metadata) : ICatalogService
{
    /// <inheritdoc />
    public async Task<ProductModel> CreateProductAsync(
        CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        await EnsureUniqueProductAsync(
            command.Request.Slug,
            command.Request.Sku,
            null,
            cancellationToken);

        var product = new Product
        {
            Name = command.Request.Name.Trim(),
            Slug = command.Request.Slug,
            Sku = command.Request.Sku,
            ShortDescription = command.Request.ShortDescription,
            Description = command.Request.Description,
            Status = command.Request.Status,
            ProductType = command.Request.ProductType,
            Price = command.Request.Price,
            CompareAtPrice = command.Request.CompareAtPrice,
            Currency = command.Request.Currency.ToUpperInvariant(),
            BrandId = command.Request.BrandId
        };

        await products.AddAsync(product, cancellationToken);
        await products.SaveChangesAsync(cancellationToken);

        return Map(product);
    }

    /// <inheritdoc />
    public async Task<ProductModel?> UpdateProductAsync(
        UpdateProductCommand command,
        CancellationToken cancellationToken)
    {
        var product = await products.FirstOrDefaultAsync(
            x => x.Id == command.Request.Id,
            true,
            cancellationToken);

        if (product is null)
        {
            return null;
        }

        await EnsureUniqueProductAsync(
            command.Request.Slug,
            command.Request.Sku,
            command.Request.Id,
            cancellationToken);

        Apply(product, command);
        await products.SaveChangesAsync(cancellationToken);

        return Map(product);
    }

    /// <inheritdoc />
    public async Task DeleteProductAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var product = await products.FirstOrDefaultAsync(
            x => x.Id == id,
            true,
            cancellationToken);

        if (product is null)
        {
            return;
        }

        products.Remove(product);
        await products.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ProductModel?> GetProductAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var product = await products.FirstOrDefaultAsync(
            x => x.Id == id,
            false,
            cancellationToken);

        return product is null
            ? null
            : Map(product);
    }

    /// <inheritdoc />
    public async Task<PagedResult<ProductModel>> ListProductsAsync(
        ProductListQuery query,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(1, query.Request.Page);
        var pageSize = Math.Clamp(query.Request.PageSize, 1, 100);
        var predicate = BuildProductPredicate(query);
        var total = await products.CountAsync(predicate, cancellationToken);
        var entities = await products.ListAsync(
            predicate,
            x => x.CreatedAt,
            true,
            (page - 1) * pageSize,
            pageSize,
            false,
            cancellationToken);
        var items = entities.Select(Map).ToList();

        return new PagedResult<ProductModel>(
            items,
            page,
            pageSize,
            total);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CategoryModel>> GetCategoriesAsync(
        CancellationToken cancellationToken)
    {
        var entities = await categories.ListAsync(
            null,
            x => x.DisplayOrder,
            false,
            null,
            null,
            false,
            cancellationToken);

        return entities
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .Select(Map)
            .ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<BrandModel>> GetBrandsAsync(
        CancellationToken cancellationToken)
    {
        var entities = await brands.ListAsync(
            null,
            x => x.Name,
            false,
            null,
            null,
            false,
            cancellationToken);

        return entities.Select(Map).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TagModel>> GetTagsAsync(
        CancellationToken cancellationToken)
    {
        var entities = await tags.ListAsync(
            null,
            x => x.Name,
            false,
            null,
            null,
            false,
            cancellationToken);

        return entities.Select(Map).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AttributeModel>> GetAttributesAsync(
        CancellationToken cancellationToken)
    {
        var entities = await attributes.ListAsync(
            null,
            x => x.Name,
            false,
            null,
            null,
            false,
            cancellationToken,
            x => x.Values);

        return entities.Select(Map).ToList();
    }

    /// <inheritdoc />
    public async Task<CategoryModel> CreateCategoryAsync(
        CreateCategoryCommand command,
        CancellationToken cancellationToken)
    {
        await ValidateParentAsync(
            command.Request.ParentId,
            null,
            cancellationToken);

        var category = new Category
        {
            Name = command.Request.Name.Trim(),
            Slug = command.Request.Slug,
            Description = command.Request.Description,
            ParentId = command.Request.ParentId,
            DisplayOrder = command.Request.DisplayOrder
        };

        await categories.AddAsync(category, cancellationToken);
        await categories.SaveChangesAsync(cancellationToken);

        return Map(category);
    }

    /// <inheritdoc />
    public async Task<CategoryModel?> UpdateCategoryAsync(
        UpdateCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var category = await categories.FirstOrDefaultAsync(
            x => x.Id == command.Request.Id,
            true,
            cancellationToken);

        if (category is null)
        {
            return null;
        }

        await ValidateParentAsync(
            command.Request.ParentId,
            command.Request.Id,
            cancellationToken);

        category.Name = command.Request.Name.Trim();
        category.Slug = command.Request.Slug;
        category.Description = command.Request.Description;
        category.ParentId = command.Request.ParentId;
        category.DisplayOrder = command.Request.DisplayOrder;
        category.UpdatedAt = DateTime.UtcNow;

        await categories.SaveChangesAsync(cancellationToken);

        return Map(category);
    }

    /// <inheritdoc />
    public async Task DeleteCategoryAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var category = await categories.FirstOrDefaultAsync(
            x => x.Id == id,
            true,
            cancellationToken);

        if (category is null)
        {
            return;
        }

        categories.Remove(category);
        await categories.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<BrandModel> CreateBrandAsync(
        CreateBrandCommand command,
        CancellationToken cancellationToken)
    {
        var entity = new Brand
        {
            Name = command.Request.Name.Trim(),
            Slug = command.Request.Slug,
            Description = command.Request.Description,
            LogoMediaId = command.Request.LogoMediaId
        };

        await brands.AddAsync(entity, cancellationToken);
        await brands.SaveChangesAsync(cancellationToken);

        return Map(entity);
    }

    /// <inheritdoc />
    public async Task<BrandModel?> UpdateBrandAsync(
        UpdateBrandCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await brands.FirstOrDefaultAsync(
            x => x.Id == command.Request.Id,
            true,
            cancellationToken);

        if (entity is null)
        {
            return null;
        }

        entity.Name = command.Request.Name.Trim();
        entity.Slug = command.Request.Slug;
        entity.Description = command.Request.Description;
        entity.LogoMediaId = command.Request.LogoMediaId;
        entity.UpdatedAt = DateTime.UtcNow;

        await brands.SaveChangesAsync(cancellationToken);

        return Map(entity);
    }

    /// <inheritdoc />
    public async Task DeleteBrandAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var entity = await brands.FirstOrDefaultAsync(
            x => x.Id == id,
            true,
            cancellationToken);

        if (entity is null)
        {
            return;
        }

        brands.Remove(entity);
        await brands.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TagModel> CreateTagAsync(
        CreateTagCommand command,
        CancellationToken cancellationToken)
    {
        var entity = new RemoteTag
        {
            Name = command.Request.Name.Trim(),
            Slug = command.Request.Slug,
            Description = command.Request.Description
        };

        await tags.AddAsync(entity, cancellationToken);
        await tags.SaveChangesAsync(cancellationToken);

        return Map(entity);
    }

    /// <inheritdoc />
    public async Task<TagModel?> UpdateTagAsync(
        UpdateTagCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await tags.FirstOrDefaultAsync(
            x => x.Id == command.Request.Id,
            true,
            cancellationToken);

        if (entity is null)
        {
            return null;
        }

        entity.Name = command.Request.Name.Trim();
        entity.Slug = command.Request.Slug;
        entity.Description = command.Request.Description;
        entity.UpdatedAt = DateTime.UtcNow;

        await tags.SaveChangesAsync(cancellationToken);

        return Map(entity);
    }

    /// <inheritdoc />
    public async Task DeleteTagAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var entity = await tags.FirstOrDefaultAsync(
            x => x.Id == id,
            true,
            cancellationToken);

        if (entity is null)
        {
            return;
        }

        tags.Remove(entity);
        await tags.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ProductVariantModel> CreateVariantAsync(
        CreateProductVariantCommand command,
        CancellationToken cancellationToken)
    {
        var productExists = await products.CountAsync(
            x => x.Id == command.Request.ProductId,
            cancellationToken);

        if (productExists == 0)
        {
            throw new ValidationException("The product does not exist.");
        }

        var skuExists = await variants.CountAsync(
            x => x.Sku == command.Request.Sku,
            cancellationToken);

        if (skuExists > 0)
        {
            throw new ValidationException("The variant SKU is already in use.");
        }

        var variant = new ProductVariant
        {
            ProductId = command.Request.ProductId,
            Sku = command.Request.Sku,
            Price = command.Request.Price,
            CompareAtPrice = command.Request.CompareAtPrice,
            StockQuantity = command.Request.StockQuantity,
            ManageStock = command.Request.ManageStock,
            Status = command.Request.Status
        };

        await variants.AddAsync(variant, cancellationToken);
        await variants.SaveChangesAsync(cancellationToken);

        return Map(variant);
    }

    /// <inheritdoc />
    public async Task<ProductVariantModel?> UpdateVariantAsync(
        UpdateProductVariantCommand command,
        CancellationToken cancellationToken)
    {
        var variant = await variants.FirstOrDefaultAsync(
            x => x.Id == command.Request.Id && x.ProductId == command.Request.ProductId,
            true,
            cancellationToken);

        if (variant is null)
        {
            return null;
        }

        var duplicateSku = await variants.CountAsync(
            x => x.Sku == command.Request.Sku && x.Id != command.Request.Id,
            cancellationToken);

        if (duplicateSku > 0)
        {
            throw new ValidationException("The variant SKU is already in use.");
        }

        variant.Sku = command.Request.Sku;
        variant.Price = command.Request.Price;
        variant.CompareAtPrice = command.Request.CompareAtPrice;
        variant.StockQuantity = command.Request.StockQuantity;
        variant.ManageStock = command.Request.ManageStock;
        variant.Status = command.Request.Status;
        variant.UpdatedAt = DateTime.UtcNow;

        await variants.SaveChangesAsync(cancellationToken);

        return Map(variant);
    }

    /// <inheritdoc />
    public async Task DeleteVariantAsync(
        Guid productId,
        Guid variantId,
        CancellationToken cancellationToken)
    {
        var variant = await variants.FirstOrDefaultAsync(
            x => x.Id == variantId && x.ProductId == productId,
            true,
            cancellationToken);

        if (variant is null)
        {
            return;
        }

        variants.Remove(variant);
        await variants.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductVariantModel>> GetVariantsAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var entities = await variants.ListAsync(
            x => x.ProductId == productId,
            x => x.Sku,
            false,
            null,
            null,
            false,
            cancellationToken);

        return entities.Select(Map).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductMetadataModel>> GetMetadataAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var entities = await metadata.ListAsync(
            x => x.ProductId == productId,
            x => x.Key,
            false,
            null,
            null,
            false,
            cancellationToken);

        return entities.Select(Map).ToList();
    }

    /// <inheritdoc />
    public async Task<ProductMetadataModel> UpsertMetadataAsync(
        UpsertProductMetadataCommand command,
        CancellationToken cancellationToken)
    {
        var productExists = await products.CountAsync(
            x => x.Id == command.Request.ProductId,
            cancellationToken);

        if (productExists == 0)
        {
            throw new ValidationException("The product does not exist.");
        }

        var entity = await metadata.FirstOrDefaultAsync(
            x => x.ProductId == command.Request.ProductId && x.Key == command.Request.Key,
            true,
            cancellationToken);

        if (entity is null)
        {
            entity = new ProductMetadata
            {
                ProductId = command.Request.ProductId,
                Key = command.Request.Key,
                Type = command.Request.Type,
                Value = command.Request.Value
            };

            await metadata.AddAsync(entity, cancellationToken);
        }
        else
        {
            entity.Type = command.Request.Type;
            entity.Value = command.Request.Value;
            entity.UpdatedAt = DateTime.UtcNow;
        }

        await metadata.SaveChangesAsync(cancellationToken);

        return Map(entity);
    }

    /// <inheritdoc />
    public async Task DeleteMetadataAsync(
        Guid productId,
        string key,
        CancellationToken cancellationToken)
    {
        var entity = await metadata.FirstOrDefaultAsync(
            x => x.ProductId == productId && x.Key == key,
            true,
            cancellationToken);

        if (entity is null)
        {
            return;
        }

        metadata.Remove(entity);
        await metadata.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureUniqueProductAsync(
        string slug,
        string? sku,
        Guid? id,
        CancellationToken cancellationToken)
    {
        var duplicateSlug = await products.CountAsync(
            x => x.Slug == slug && x.Id != id,
            cancellationToken);

        if (duplicateSlug > 0)
        {
            throw new ValidationException("The product slug is already in use.");
        }

        if (sku is null)
        {
            return;
        }

        var duplicateProductSku = await products.CountAsync(
            x => x.Sku == sku && x.Id != id,
            cancellationToken);

        if (duplicateProductSku > 0)
        {
            throw new ValidationException("The product SKU is already in use.");
        }

        var duplicateVariantSku = await variants.CountAsync(
            x => x.Sku == sku,
            cancellationToken);

        if (duplicateVariantSku > 0)
        {
            throw new ValidationException("The SKU is already in use by a variant.");
        }
    }

    private async Task ValidateParentAsync(
        Guid? parentId,
        Guid? currentId,
        CancellationToken cancellationToken)
    {
        if (!parentId.HasValue)
        {
            return;
        }

        if (currentId.HasValue && parentId.Value == currentId.Value)
        {
            throw new ValidationException("A category cannot be its own parent.");
        }

        var visited = new HashSet<Guid>();
        var cursor = parentId;

        while (cursor.HasValue)
        {
            if (!visited.Add(cursor.Value))
            {
                throw new ValidationException("The category hierarchy contains a cycle.");
            }

            if (currentId.HasValue && cursor.Value == currentId.Value)
            {
                throw new ValidationException("A category cannot be moved below its own descendant.");
            }

            var parent = await categories.FirstOrDefaultAsync(
                x => x.Id == cursor.Value,
                false,
                cancellationToken);

            if (parent is null)
            {
                throw new ValidationException("The parent category does not exist.");
            }

            cursor = parent.ParentId;
        }
    }

    private static Expression<Func<Product, bool>> BuildProductPredicate(ProductListQuery query)
    {
        Expression<Func<Product, bool>> predicate = x => true;

        if (!string.IsNullOrWhiteSpace(query.Request.Search))
        {
            var search = query.Request.Search.Trim();
            predicate = Combine(
                predicate,
                x => x.Name.Contains(search) || x.Description.Contains(search));
        }

        if (query.Request.Status.HasValue)
        {
            var status = query.Request.Status.Value;
            predicate = Combine(predicate, x => x.Status == status);
        }

        if (query.Request.BrandId.HasValue)
        {
            var brandId = query.Request.BrandId.Value;
            predicate = Combine(predicate, x => x.BrandId == brandId);
        }

        if (!string.IsNullOrWhiteSpace(query.Request.Sku))
        {
            var sku = query.Request.Sku.Trim();
            predicate = Combine(predicate, x => x.Sku == sku);
        }

        if (query.Request.ProductType.HasValue)
        {
            var productType = query.Request.ProductType.Value;
            predicate = Combine(predicate, x => x.ProductType == productType);
        }

        if (query.Request.CategoryId.HasValue)
        {
            var categoryId = query.Request.CategoryId.Value;
            predicate = Combine(
                predicate,
                x => x.Categories.Any(category => category.CategoryId == categoryId));
        }

        if (!string.IsNullOrWhiteSpace(query.Request.Tag))
        {
            var tag = query.Request.Tag.Trim();
            predicate = Combine(
                predicate,
                x => x.Tags.Any(productTag =>
                    productTag.Tag!.Slug == tag ||
                    productTag.Tag!.Name == tag));
        }

        return predicate;
    }

    private static Expression<Func<Product, bool>> Combine(
        Expression<Func<Product, bool>> first,
        Expression<Func<Product, bool>> second)
    {
        var parameter = Expression.Parameter(typeof(Product), "product");
        var firstBody = ReplaceParameter(
            first.Body,
            first.Parameters[0],
            parameter);
        var secondBody = ReplaceParameter(
            second.Body,
            second.Parameters[0],
            parameter);
        var body = Expression.AndAlso(
            firstBody,
            secondBody);

        return Expression.Lambda<Func<Product, bool>>(
            body,
            parameter);
    }

    private static Expression ReplaceParameter(
        Expression expression,
        ParameterExpression source,
        ParameterExpression target)
    {
        return new ParameterReplaceVisitor(
            source,
            target).Visit(expression)!;
    }

    private static void Apply(
        Product product,
        UpdateProductCommand command)
    {
        product.Name = command.Request.Name.Trim();
        product.Slug = command.Request.Slug;
        product.Sku = command.Request.Sku;
        product.ShortDescription = command.Request.ShortDescription;
        product.Description = command.Request.Description;
        product.Status = command.Request.Status;
        product.ProductType = command.Request.ProductType;
        product.Price = command.Request.Price;
        product.CompareAtPrice = command.Request.CompareAtPrice;
        product.Currency = command.Request.Currency.ToUpperInvariant();
        product.BrandId = command.Request.BrandId;
        product.UpdatedAt = DateTime.UtcNow;
    }

    private static ProductModel Map(Product entity)
    {
        return new ProductModel(
            entity.Id,
            entity.Name,
            entity.Slug,
            entity.Sku,
            entity.ShortDescription,
            entity.Description,
            entity.Status,
            entity.ProductType,
            entity.Price,
            entity.CompareAtPrice,
            entity.Currency,
            entity.BrandId,
            entity.CreatedAt,
            entity.UpdatedAt);
    }

    private static CategoryModel Map(Category entity)
    {
        return new CategoryModel(
            entity.Id,
            entity.Name,
            entity.Slug,
            entity.Description,
            entity.ParentId,
            entity.DisplayOrder);
    }

    private static BrandModel Map(Brand entity)
    {
        return new BrandModel(
            entity.Id,
            entity.Name,
            entity.Slug,
            entity.Description,
            entity.LogoMediaId);
    }

    private static TagModel Map(RemoteTag entity)
    {
        return new TagModel(
            entity.Id,
            entity.Name,
            entity.Slug,
            entity.Description);
    }

    private static AttributeModel Map(ProductAttribute entity)
    {
        var values = entity.Values
            .OrderBy(value => value.Value)
            .Select(value => new AttributeValueModel(
                value.Id,
                value.Value,
                value.Slug))
            .ToList();

        return new AttributeModel(
            entity.Id,
            entity.Name,
            entity.Slug,
            values);
    }

    private static ProductVariantModel Map(ProductVariant entity)
    {
        return new ProductVariantModel(
            entity.Id,
            entity.ProductId,
            entity.Sku,
            entity.Price,
            entity.CompareAtPrice,
            entity.StockQuantity,
            entity.ManageStock,
            entity.Status);
    }

    private static ProductMetadataModel Map(ProductMetadata entity)
    {
        return new ProductMetadataModel(
            entity.Id,
            entity.ProductId,
            entity.Key,
            entity.Type,
            entity.Value);
    }

    private sealed class ParameterReplaceVisitor(
        ParameterExpression source,
        ParameterExpression target) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == source
                ? target
                : base.VisitParameter(node);
        }
    }
}

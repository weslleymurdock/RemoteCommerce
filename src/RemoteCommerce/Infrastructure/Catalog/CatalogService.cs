namespace RemoteCommerce.Infrastructure.Catalog;

/// <summary>Provides catalog persistence for application feature services.</summary>
public sealed class CatalogService(CommerceDbContext db) : ICatalogService
{
    /// <inheritdoc />
    public async Task<ProductModel> CreateProductAsync(CreateProductCommand command, CancellationToken cancellationToken)
    {
        await EnsureUniqueProductAsync(command.Request.Slug, command.Request.Sku, null, cancellationToken);
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
        db.Products.Add(product);
        await db.SaveChangesAsync(cancellationToken);
        return Map(product);
    }

    /// <inheritdoc />
    public async Task<ProductModel?> UpdateProductAsync(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        var product = await db.Products.SingleOrDefaultAsync(
            x => x.Id == command.Request.Id,
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
        Apply(product, command.Request);
        await db.SaveChangesAsync(cancellationToken);
        return Map(product);
    }

    /// <inheritdoc />
    public async Task DeleteProductAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await db.Products.SingleOrDefaultAsync(
            x => x.Id == id,
            cancellationToken);
        if (product is null)
        {
            return;
        }

        db.Products.Remove(product);
        await db.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ProductModel?> GetProductAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await db.Products.AsNoTracking().SingleOrDefaultAsync(
            x => x.Id == id,
            cancellationToken);
        return product is null ? null : Map(product);
    }

    /// <inheritdoc />
    public async Task<PagedResult<ProductModel>> ListProductsAsync(ProductListQuery query, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, query.Request.Page);
        var pageSize = Math.Clamp(query.Request.PageSize, 1, 100);
        IQueryable<Product> products = db.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Request.Search))
        {
            products = products.Where(x =>
                x.Name.Contains(query.Request.Search) ||
                x.Description.Contains(query.Request.Search));
        }

        if (query.Request.Status.HasValue)
        {
            products = products.Where(x => x.Status == query.Request.Status.Value);
        }

        if (query.Request.BrandId.HasValue)
        {
            products = products.Where(x => x.BrandId == query.Request.BrandId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Request.Sku))
        {
            products = products.Where(x => x.Sku == query.Request.Sku);
        }

        if (query.Request.ProductType.HasValue)
        {
            products = products.Where(x => x.ProductType == query.Request.ProductType.Value);
        }

        if (query.Request.CategoryId.HasValue)
        {
            products = products.Where(x =>
                x.Categories.Any(c => c.CategoryId == query.Request.CategoryId.Value));
        }

        if (!string.IsNullOrWhiteSpace(query.Request.Tag))
        {
            products = products.Where(x =>
                x.Tags.Any(t =>
                    t.Tag!.Slug == query.Request.Tag ||
                    t.Tag!.Name == query.Request.Tag));
        }

        var total = await products.CountAsync(cancellationToken);
        var items = await products
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ProductModel(
                x.Id,
                x.Name,
                x.Slug,
                x.Sku,
                x.ShortDescription,
                x.Description,
                x.Status,
                x.ProductType,
                x.Price,
                x.CompareAtPrice,
                x.Currency,
                x.BrandId,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductModel>(items, page, pageSize, total);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CategoryModel>> GetCategoriesAsync(CancellationToken cancellationToken)
    {
        return await db.Categories
            .AsNoTracking()
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .Select(x => new CategoryModel(
                x.Id,
                x.Name,
                x.Slug,
                x.Description,
                x.ParentId,
                x.DisplayOrder))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<BrandModel>> GetBrandsAsync(CancellationToken cancellationToken)
    {
        return await db.Brands
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new BrandModel(
                x.Id,
                x.Name,
                x.Slug,
                x.Description,
                x.LogoMediaId))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TagModel>> GetTagsAsync(CancellationToken cancellationToken)
    {
        return await db.Tags
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new TagModel(
                x.Id,
                x.Name,
                x.Slug,
                x.Description))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AttributeModel>> GetAttributesAsync(CancellationToken cancellationToken)
    {
        return await db.ProductAttributes
            .AsNoTracking()
            .Include(x => x.Values)
            .OrderBy(x => x.Name)
            .Select(x => new AttributeModel(
                x.Id,
                x.Name,
                x.Slug,
                x.Values
                    .OrderBy(v => v.Value)
                    .Select(v => new AttributeValueModel(v.Id, v.Value, v.Slug))
                    .ToList()))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CategoryModel> CreateCategoryAsync(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        await ValidateParentAsync(command.Request.ParentId, null, cancellationToken);
        var category = new Category
        {
            Name = command.Request.Name.Trim(),
            Slug = command.Request.Slug,
            Description = command.Request.Description,
            ParentId = command.Request.ParentId,
            DisplayOrder = command.Request.DisplayOrder
        };
        db.Categories.Add(category);
        await db.SaveChangesAsync(cancellationToken);
        return Map(category);
    }

    /// <inheritdoc />
    public async Task<CategoryModel?> UpdateCategoryAsync(UpdateCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await db.Categories.SingleOrDefaultAsync(
            x => x.Id == command.Request.Id,
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
        await db.SaveChangesAsync(cancellationToken);
        return Map(category);
    }

    /// <inheritdoc />
    public async Task DeleteCategoryAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await db.Categories.SingleOrDefaultAsync(
            x => x.Id == id,
            cancellationToken);
        if (entity is null)
        {
            return;
        }

        db.Categories.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<BrandModel> CreateBrandAsync(CreateBrandCommand command, CancellationToken cancellationToken)
    {
        var entity = new Brand
        {
            Name = command.Request.Name.Trim(),
            Slug = command.Request.Slug,
            Description = command.Request.Description,
            LogoMediaId = command.Request.LogoMediaId
        };
        db.Brands.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    /// <inheritdoc />
    public async Task<BrandModel?> UpdateBrandAsync(UpdateBrandCommand command, CancellationToken cancellationToken)
    {
        var entity = await db.Brands.SingleOrDefaultAsync(
            x => x.Id == command.Request.Id,
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
        await db.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    /// <inheritdoc />
    public async Task DeleteBrandAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await db.Brands.SingleOrDefaultAsync(
            x => x.Id == id,
            cancellationToken);
        if (entity is null)
        {
            return;
        }

        db.Brands.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TagModel> CreateTagAsync(CreateTagCommand command, CancellationToken cancellationToken)
    {
        var entity = new RemoteTag
        {
            Name = command.Request.Name.Trim(),
            Slug = command.Request.Slug,
            Description = command.Request.Description
        };
        db.Tags.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    /// <inheritdoc />
    public async Task<TagModel?> UpdateTagAsync(UpdateTagCommand command, CancellationToken cancellationToken)
    {
        var entity = await db.Tags.SingleOrDefaultAsync(
            x => x.Id == command.Request.Id,
            cancellationToken);
        if (entity is null)
        {
            return null;
        }

        entity.Name = command.Request.Name.Trim();
        entity.Slug = command.Request.Slug;
        entity.Description = command.Request.Description;
        entity.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return Map(entity);
    }

    /// <inheritdoc />
    public async Task DeleteTagAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await db.Tags.SingleOrDefaultAsync(
            x => x.Id == id,
            cancellationToken);
        if (entity is null)
        {
            return;
        }

        db.Tags.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ProductVariantModel> CreateVariantAsync(CreateProductVariantCommand command, CancellationToken cancellationToken)
    {
        if (!await db.Products.AnyAsync(
            x => x.Id == command.Request.ProductId,
            cancellationToken))
        {
            throw new ValidationException("The product does not exist.");
        }

        if (await db.ProductVariants.AnyAsync(
            x => x.Sku == command.Request.Sku,
            cancellationToken))
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
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync(cancellationToken);
        return Map(variant);
    }

    /// <inheritdoc />
    public async Task<ProductVariantModel?> UpdateVariantAsync(UpdateProductVariantCommand command, CancellationToken cancellationToken)
    {
        var variant = await db.ProductVariants.SingleOrDefaultAsync(
            x => x.Id == command.Request.Id && x.ProductId == command.Request.ProductId,
            cancellationToken);
        if (variant is null)
        {
            return null;
        }

        if (await db.ProductVariants.AnyAsync(
            x => x.Sku == command.Request.Sku && x.Id != command.Request.Id,
            cancellationToken))
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
        await db.SaveChangesAsync(cancellationToken);
        return Map(variant);
    }

    /// <inheritdoc />
    public async Task DeleteVariantAsync(Guid productId, Guid variantId, CancellationToken cancellationToken)
    {
        var variant = await db.ProductVariants.SingleOrDefaultAsync(
            x => x.Id == variantId && x.ProductId == productId,
            cancellationToken);
        if (variant is null)
        {
            return;
        }

        db.ProductVariants.Remove(variant);
        await db.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductVariantModel>> GetVariantsAsync(Guid productId, CancellationToken cancellationToken)
    {
        return await db.ProductVariants
            .AsNoTracking()
            .Where(x => x.ProductId == productId)
            .OrderBy(x => x.Sku)
            .Select(x => new ProductVariantModel(
                x.Id,
                x.ProductId,
                x.Sku,
                x.Price,
                x.CompareAtPrice,
                x.StockQuantity,
                x.ManageStock,
                x.Status))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductMetadataModel>> GetMetadataAsync(Guid productId, CancellationToken cancellationToken)
    {
        return await db.ProductMetadata
            .AsNoTracking()
            .Where(x => x.ProductId == productId)
            .OrderBy(x => x.Key)
            .Select(x => new ProductMetadataModel(
                x.Id,
                x.ProductId,
                x.Key,
                x.Type,
                x.Value))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ProductMetadataModel> UpsertMetadataAsync(UpsertProductMetadataCommand command, CancellationToken cancellationToken)
    {
        if (!await db.Products.AnyAsync(
            x => x.Id == command.Request.ProductId,
            cancellationToken))
        {
            throw new ValidationException("The product does not exist.");
        }

        var metadata = await db.ProductMetadata.SingleOrDefaultAsync(
            x => x.ProductId == command.Request.ProductId && x.Key == command.Request.Key,
            cancellationToken);
        if (metadata is null)
        {
            metadata = new ProductMetadata
            {
                ProductId = command.Request.ProductId,
                Key = command.Request.Key,
                Type = command.Request.Type,
                Value = command.Request.Value
            };
            db.ProductMetadata.Add(metadata);
        }
        else
        {
            metadata.Type = command.Request.Type;
            metadata.Value = command.Request.Value;
            metadata.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);
        return Map(metadata);
    }

    /// <inheritdoc />
    public async Task DeleteMetadataAsync(Guid productId, string key, CancellationToken cancellationToken)
    {
        var metadata = await db.ProductMetadata.SingleOrDefaultAsync(
            x => x.ProductId == productId && x.Key == key,
            cancellationToken);
        if (metadata is null)
        {
            return;
        }

        db.ProductMetadata.Remove(metadata);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureUniqueProductAsync(
        string slug,
        string? sku,
        Guid? id,
        CancellationToken cancellationToken)
    {
        if (await db.Products.AnyAsync(
            x => x.Slug == slug && x.Id != id,
            cancellationToken))
        {
            throw new ValidationException("The product slug is already in use.");
        }

        if (sku is null)
        {
            return;
        }

        if (await db.Products.AnyAsync(
            x => x.Sku == sku && x.Id != id,
            cancellationToken))
        {
            throw new ValidationException("The product SKU is already in use.");
        }

        if (await db.ProductVariants.AnyAsync(
            x => x.Sku == sku,
            cancellationToken))
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

        if (currentId == parentId)
        {
            throw new ValidationException("A category cannot be its own parent.");
        }

        var parent = await db.Categories.SingleOrDefaultAsync(
            x => x.Id == parentId.Value,
            cancellationToken);
        if (parent is null)
        {
            throw new ValidationException("The parent category does not exist.");
        }

        if (!currentId.HasValue)
        {
            return;
        }

        var seen = new HashSet<Guid> { currentId.Value };
        while (parent.ParentId.HasValue)
        {
            if (!seen.Add(parent.Id) || parent.ParentId == currentId)
            {
                throw new ValidationException("The category hierarchy cannot contain a cycle.");
            }

            parent = await db.Categories.SingleOrDefaultAsync(
                x => x.Id == parent.ParentId.Value,
                cancellationToken);
            if (parent is null)
            {
                throw new ValidationException("The category hierarchy is invalid.");
            }
        }
    }

    private static void Apply(Product product, UpdateProductRequest request)
    {
        product.Name = request.Name.Trim();
        product.Slug = request.Slug;
        product.Sku = request.Sku;
        product.ShortDescription = request.ShortDescription;
        product.Description = request.Description;
        product.Status = request.Status;
        product.ProductType = request.ProductType;
        product.Price = request.Price;
        product.CompareAtPrice = request.CompareAtPrice;
        product.Currency = request.Currency.ToUpperInvariant();
        product.BrandId = request.BrandId;
        product.UpdatedAt = DateTime.UtcNow;
    }

    private static ProductModel Map(Product product)
    {
        return new ProductModel(
            product.Id,
            product.Name,
            product.Slug,
            product.Sku,
            product.ShortDescription,
            product.Description,
            product.Status,
            product.ProductType,
            product.Price,
            product.CompareAtPrice,
            product.Currency,
            product.BrandId,
            product.CreatedAt,
            product.UpdatedAt);
    }

    private static CategoryModel Map(Category category)
    {
        return new CategoryModel(
            category.Id,
            category.Name,
            category.Slug,
            category.Description,
            category.ParentId,
            category.DisplayOrder);
    }

    private static BrandModel Map(Brand brand)
    {
        return new BrandModel(
            brand.Id,
            brand.Name,
            brand.Slug,
            brand.Description,
            brand.LogoMediaId);
    }

    private static TagModel Map(RemoteTag tag)
    {
        return new TagModel(
            tag.Id,
            tag.Name,
            tag.Slug,
            tag.Description);
    }

    private static ProductVariantModel Map(ProductVariant variant)
    {
        return new ProductVariantModel(
            variant.Id,
            variant.ProductId,
            variant.Sku,
            variant.Price,
            variant.CompareAtPrice,
            variant.StockQuantity,
            variant.ManageStock,
            variant.Status);
    }

    private static ProductMetadataModel Map(ProductMetadata metadata)
    {
        return new ProductMetadataModel(
            metadata.Id,
            metadata.ProductId,
            metadata.Key,
            metadata.Type,
            metadata.Value);
    }
}

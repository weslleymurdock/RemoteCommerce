namespace RemoteCommerce.Controllers.v1;

/// <summary>Exposes the RemoteCommerce catalog REST API.</summary>
[ApiController]
[Route("api/rc/v1")]
public sealed class CatalogController(IMediator mediator) : ControllerBase
{
    /// <summary>Lists catalog products with bounded pagination and filters.</summary><param name="query">Pagination and filtering parameters.</param><param name="cancellationToken">The cancellation token.</param><returns>A paged product collection.</returns>
    [HttpGet("products")][AllowAnonymous] public Task<PagedResult<ProductModel>> GetProducts([FromQuery] ProductListQuery query, CancellationToken cancellationToken) => mediator.Send(query, cancellationToken);
    /// <summary>Gets a product by identifier.</summary><param name="id">The product identifier.</param><param name="cancellationToken">The cancellation token.</param><returns>The product when found.</returns>
    [HttpGet("products/{id:guid}")][AllowAnonymous] public async Task<ActionResult<ProductModel>> GetProduct(Guid id, CancellationToken cancellationToken) { var result = await mediator.Send(new GetProductQuery(id), cancellationToken); return result is null ? NotFound() : Ok(result); }
    /// <summary>Creates a product.</summary><param name="command">The product payload.</param><param name="cancellationToken">The cancellation token.</param><returns>The created product.</returns>
    [HttpPost("products")][Authorize(Policy = AuthorizationPolicies.Administrator)] public async Task<ActionResult<ProductModel>> CreateProduct(CreateProductCommand command, CancellationToken cancellationToken) { var result = await mediator.Send(command, cancellationToken); return CreatedAtAction(nameof(GetProduct), new { id = result.Id }, result); }
    /// <summary>Updates a product.</summary><param name="id">The product identifier.</param><param name="command">The product payload.</param><param name="cancellationToken">The cancellation token.</param><returns>The updated product.</returns>
    [HttpPut("products/{id:guid}")][Authorize(Policy = AuthorizationPolicies.Administrator)] public async Task<ActionResult<ProductModel>> UpdateProduct(Guid id, UpdateProductCommand command, CancellationToken cancellationToken) { if (id != command.Id) return BadRequest(); var result = await mediator.Send(command, cancellationToken); return result is null ? NotFound() : Ok(result); }
    /// <summary>Soft-deletes a product.</summary><param name="id">The product identifier.</param><param name="cancellationToken">The cancellation token.</param>
    [HttpDelete("products/{id:guid}")][Authorize(Policy = AuthorizationPolicies.Administrator)][ProducesResponseType(StatusCodes.Status204NoContent)] public async Task<IActionResult> DeleteProduct(Guid id, CancellationToken cancellationToken) { await mediator.Send(new DeleteProductCommand(id), cancellationToken); return NoContent(); }

    /// <summary>Lists categories.</summary><param name="cancellationToken">The cancellation token.</param><returns>Categories.</returns>
    [HttpGet("categories")][AllowAnonymous] public Task<IReadOnlyList<CategoryModel>> GetCategories(CancellationToken cancellationToken) => mediator.Send(new GetCategoriesQuery(), cancellationToken);
    /// <summary>Gets a category.</summary><param name="id">The category identifier.</param><param name="cancellationToken">The cancellation token.</param><returns>The category.</returns>
    [HttpGet("categories/{id:guid}")][AllowAnonymous] public async Task<ActionResult<CategoryModel>> GetCategory(Guid id, CancellationToken cancellationToken) { var result = (await mediator.Send(new GetCategoriesQuery(), cancellationToken)).FirstOrDefault(x => x.Id == id); return result is null ? NotFound() : Ok(result); }
    /// <summary>Creates a category.</summary><param name="command">The category payload.</param><param name="cancellationToken">The cancellation token.</param><returns>The created category.</returns>
    [HttpPost("categories")][Authorize(Policy = AuthorizationPolicies.Administrator)] public Task<CategoryModel> CreateCategory(CreateCategoryCommand command, CancellationToken cancellationToken) => mediator.Send(command, cancellationToken);
    /// <summary>Updates a category.</summary><param name="id">The category identifier.</param><param name="command">The category payload.</param><param name="cancellationToken">The cancellation token.</param><returns>The updated category.</returns>
    [HttpPut("categories/{id:guid}")][Authorize(Policy = AuthorizationPolicies.Administrator)] public async Task<ActionResult<CategoryModel>> UpdateCategory(Guid id, UpdateCategoryCommand command, CancellationToken cancellationToken) { if (id != command.Id) return BadRequest(); var result = await mediator.Send(command, cancellationToken); return result is null ? NotFound() : Ok(result); }
    /// <summary>Soft-deletes a category.</summary><param name="id">The category identifier.</param><param name="cancellationToken">The cancellation token.</param>
    [HttpDelete("categories/{id:guid}")][Authorize(Policy = AuthorizationPolicies.Administrator)] public async Task<IActionResult> DeleteCategory(Guid id, CancellationToken cancellationToken) { await mediator.Send(new DeleteCategoryCommand(id), cancellationToken); return NoContent(); }

    /// <summary>Lists brands.</summary><param name="cancellationToken">The cancellation token.</param><returns>Brands.</returns>
    [HttpGet("brands")][AllowAnonymous] public Task<IReadOnlyList<BrandModel>> GetBrands(CancellationToken cancellationToken) => mediator.Send(new GetBrandsQuery(), cancellationToken);
    /// <summary>Gets a brand.</summary><param name="id">The brand identifier.</param><param name="cancellationToken">The cancellation token.</param><returns>The brand.</returns>
    [HttpGet("brands/{id:guid}")][AllowAnonymous] public async Task<ActionResult<BrandModel>> GetBrand(Guid id, CancellationToken cancellationToken) { var result = (await mediator.Send(new GetBrandsQuery(), cancellationToken)).FirstOrDefault(x => x.Id == id); return result is null ? NotFound() : Ok(result); }
    /// <summary>Creates a brand.</summary><param name="command">The brand payload.</param><param name="cancellationToken">The cancellation token.</param><returns>The created brand.</returns>
    [HttpPost("brands")][Authorize(Policy = AuthorizationPolicies.Administrator)] public Task<BrandModel> CreateBrand(CreateBrandCommand command, CancellationToken cancellationToken) => mediator.Send(command, cancellationToken);
    /// <summary>Updates a brand.</summary><param name="id">The brand identifier.</param><param name="command">The brand payload.</param><param name="cancellationToken">The cancellation token.</param><returns>The updated brand.</returns>
    [HttpPut("brands/{id:guid}")][Authorize(Policy = AuthorizationPolicies.Administrator)] public async Task<ActionResult<BrandModel>> UpdateBrand(Guid id, UpdateBrandCommand command, CancellationToken cancellationToken) { if (id != command.Id) return BadRequest(); var result = await mediator.Send(command, cancellationToken); return result is null ? NotFound() : Ok(result); }
    /// <summary>Soft-deletes a brand.</summary><param name="id">The brand identifier.</param><param name="cancellationToken">The cancellation token.</param>
    [HttpDelete("brands/{id:guid}")][Authorize(Policy = AuthorizationPolicies.Administrator)] public async Task<IActionResult> DeleteBrand(Guid id, CancellationToken cancellationToken) { await mediator.Send(new DeleteBrandCommand(id), cancellationToken); return NoContent(); }

    /// <summary>Lists tags.</summary><param name="cancellationToken">The cancellation token.</param><returns>Tags.</returns>
    [HttpGet("tags")][AllowAnonymous] public Task<IReadOnlyList<TagModel>> GetTags(CancellationToken cancellationToken) => mediator.Send(new GetTagsQuery(), cancellationToken);
    /// <summary>Gets a tag.</summary><param name="id">The tag identifier.</param><param name="cancellationToken">The cancellation token.</param><returns>The tag.</returns>
    [HttpGet("tags/{id:guid}")][AllowAnonymous] public async Task<ActionResult<TagModel>> GetTag(Guid id, CancellationToken cancellationToken) { var result = (await mediator.Send(new GetTagsQuery(), cancellationToken)).FirstOrDefault(x => x.Id == id); return result is null ? NotFound() : Ok(result); }
    /// <summary>Creates a tag.</summary><param name="command">The tag payload.</param><param name="cancellationToken">The cancellation token.</param><returns>The created tag.</returns>
    [HttpPost("tags")][Authorize(Policy = AuthorizationPolicies.Administrator)] public Task<TagModel> CreateTag(CreateTagCommand command, CancellationToken cancellationToken) => mediator.Send(command, cancellationToken);
    /// <summary>Updates a tag.</summary><param name="id">The tag identifier.</param><param name="command">The tag payload.</param><param name="cancellationToken">The cancellation token.</param><returns>The updated tag.</returns>
    [HttpPut("tags/{id:guid}")][Authorize(Policy = AuthorizationPolicies.Administrator)] public async Task<ActionResult<TagModel>> UpdateTag(Guid id, UpdateTagCommand command, CancellationToken cancellationToken) { if (id != command.Id) return BadRequest(); var result = await mediator.Send(command, cancellationToken); return result is null ? NotFound() : Ok(result); }
    /// <summary>Soft-deletes a tag.</summary><param name="id">The tag identifier.</param><param name="cancellationToken">The cancellation token.</param>
    [HttpDelete("tags/{id:guid}")][Authorize(Policy = AuthorizationPolicies.Administrator)] public async Task<IActionResult> DeleteTag(Guid id, CancellationToken cancellationToken) { await mediator.Send(new DeleteTagCommand(id), cancellationToken); return NoContent(); }

    /// <summary>Lists product attributes.</summary><param name="cancellationToken">The cancellation token.</param><returns>Attributes.</returns>
    [HttpGet("attributes")][AllowAnonymous] public Task<IReadOnlyList<AttributeModel>> GetAttributes(CancellationToken cancellationToken) => mediator.Send(new GetAttributesQuery(), cancellationToken);
    /// <summary>Gets an attribute.</summary><param name="id">The attribute identifier.</param><param name="cancellationToken">The cancellation token.</param><returns>The attribute.</returns>
    [HttpGet("attributes/{id:guid}")][AllowAnonymous] public async Task<ActionResult<AttributeModel>> GetAttribute(Guid id, CancellationToken cancellationToken) { var result = (await mediator.Send(new GetAttributesQuery(), cancellationToken)).FirstOrDefault(x => x.Id == id); return result is null ? NotFound() : Ok(result); }

    /// <summary>Lists variants for a product.</summary><param name="productId">The product identifier.</param><param name="cancellationToken">The cancellation token.</param><returns>Product variants.</returns>
    [HttpGet("products/{productId:guid}/variations")][AllowAnonymous] public Task<IReadOnlyList<ProductVariantModel>> GetVariations(Guid productId, CancellationToken cancellationToken) => mediator.Send(new ProductVariantListQuery(productId), cancellationToken);
    /// <summary>Creates a product variant.</summary><param name="productId">The product identifier.</param><param name="command">The variant payload.</param><param name="cancellationToken">The cancellation token.</param><returns>The created variant.</returns>
    [HttpPost("products/{productId:guid}/variations")][Authorize(Policy = AuthorizationPolicies.Administrator)] public Task<ProductVariantModel> CreateVariation(Guid productId, CreateProductVariantCommand command, CancellationToken cancellationToken) => mediator.Send(command with { ProductId = productId }, cancellationToken);
    /// <summary>Gets a product variant.</summary><param name="productId">The product identifier.</param><param name="variationId">The variant identifier.</param><param name="cancellationToken">The cancellation token.</param><returns>The variant.</returns>
    [HttpGet("products/{productId:guid}/variations/{variationId:guid}")][AllowAnonymous] public async Task<ActionResult<ProductVariantModel>> GetVariation(Guid productId, Guid variationId, CancellationToken cancellationToken) { var result = (await mediator.Send(new ProductVariantListQuery(productId), cancellationToken)).FirstOrDefault(x => x.Id == variationId); return result is null ? NotFound() : Ok(result); }
    /// <summary>Updates a product variant.</summary><param name="productId">The product identifier.</param><param name="variationId">The variant identifier.</param><param name="command">The variant payload.</param><param name="cancellationToken">The cancellation token.</param><returns>The updated variant.</returns>
    [HttpPut("products/{productId:guid}/variations/{variationId:guid}")][Authorize(Policy = AuthorizationPolicies.Administrator)] public async Task<ActionResult<ProductVariantModel>> UpdateVariation(Guid productId, Guid variationId, UpdateProductVariantCommand command, CancellationToken cancellationToken) { var result = await mediator.Send(command with { ProductId = productId, Id = variationId }, cancellationToken); return result is null ? NotFound() : Ok(result); }
    /// <summary>Deletes a product variant.</summary><param name="productId">The product identifier.</param><param name="variationId">The variant identifier.</param><param name="cancellationToken">The cancellation token.</param>
    [HttpDelete("products/{productId:guid}/variations/{variationId:guid}")][Authorize(Policy = AuthorizationPolicies.Administrator)] public async Task<IActionResult> DeleteVariation(Guid productId, Guid variationId, CancellationToken cancellationToken) { await mediator.Send(new DeleteProductVariantCommand(productId, variationId), cancellationToken); return NoContent(); }

    /// <summary>Lists product metadata.</summary><param name="productId">The product identifier.</param><param name="cancellationToken">The cancellation token.</param><returns>Metadata records.</returns>
    [HttpGet("products/{productId:guid}/metadata")][AllowAnonymous] public Task<IReadOnlyList<ProductMetadataModel>> GetMetadata(Guid productId, CancellationToken cancellationToken) => mediator.Send(new ProductMetadataQuery(productId), cancellationToken);
    /// <summary>Creates or replaces product metadata.</summary><param name="productId">The product identifier.</param><param name="command">The metadata payload.</param><param name="cancellationToken">The cancellation token.</param><returns>The metadata record.</returns>
    [HttpPost("products/{productId:guid}/metadata")][Authorize(Policy = AuthorizationPolicies.Administrator)] public Task<ProductMetadataModel> CreateMetadata(Guid productId, UpsertProductMetadataCommand command, CancellationToken cancellationToken) => mediator.Send(command with { ProductId = productId }, cancellationToken);
    /// <summary>Replaces product metadata by key.</summary><param name="productId">The product identifier.</param><param name="key">The metadata key.</param><param name="command">The metadata payload.</param><param name="cancellationToken">The cancellation token.</param><returns>The metadata record.</returns>
    [HttpPut("products/{productId:guid}/metadata/{key}")][Authorize(Policy = AuthorizationPolicies.Administrator)] public Task<ProductMetadataModel> UpdateMetadata(Guid productId, string key, UpsertProductMetadataCommand command, CancellationToken cancellationToken) => mediator.Send(command with { ProductId = productId, Key = key }, cancellationToken);
    /// <summary>Deletes product metadata.</summary><param name="productId">The product identifier.</param><param name="key">The metadata key.</param><param name="cancellationToken">The cancellation token.</param>
    [HttpDelete("products/{productId:guid}/metadata/{key}")][Authorize(Policy = AuthorizationPolicies.Administrator)] public async Task<IActionResult> DeleteMetadata(Guid productId, string key, CancellationToken cancellationToken) { await mediator.Send(new DeleteProductMetadataCommand(productId, key), cancellationToken); return NoContent(); }
}

/// <summary>Reads categories.</summary> public sealed record GetCategoriesQuery : IRequest<IReadOnlyList<CategoryModel>>;
/// <summary>Reads brands.</summary> public sealed record GetBrandsQuery : IRequest<IReadOnlyList<BrandModel>>;
/// <summary>Reads tags.</summary> public sealed record GetTagsQuery : IRequest<IReadOnlyList<TagModel>>;
/// <summary>Reads attributes.</summary> public sealed record GetAttributesQuery : IRequest<IReadOnlyList<AttributeModel>>;
/// <summary>Handles catalog read queries.</summary>
public sealed class CatalogQueryHandlers(ICatalogService catalog) : IRequestHandler<GetCategoriesQuery, IReadOnlyList<CategoryModel>>, IRequestHandler<GetBrandsQuery, IReadOnlyList<BrandModel>>, IRequestHandler<GetTagsQuery, IReadOnlyList<TagModel>>, IRequestHandler<GetAttributesQuery, IReadOnlyList<AttributeModel>>
{
    /// <inheritdoc /> public Task<IReadOnlyList<CategoryModel>> Handle(GetCategoriesQuery r, CancellationToken c) => catalog.GetCategoriesAsync(c);
    /// <inheritdoc /> public Task<IReadOnlyList<BrandModel>> Handle(GetBrandsQuery r, CancellationToken c) => catalog.GetBrandsAsync(c);
    /// <inheritdoc /> public Task<IReadOnlyList<TagModel>> Handle(GetTagsQuery r, CancellationToken c) => catalog.GetTagsAsync(c);
    /// <inheritdoc /> public Task<IReadOnlyList<AttributeModel>> Handle(GetAttributesQuery r, CancellationToken c) => catalog.GetAttributesAsync(c);
}

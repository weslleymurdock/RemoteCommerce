namespace RemoteCommerce.Controllers.v1;

/// <summary>Exposes the RemoteCommerce catalog REST API.</summary>
[ApiController]
[Route("api/rc/v1")]
public sealed class CatalogController(IMediator mediator) : ControllerBase
{
    /// <summary>Lists catalog products.</summary>
    /// <param name="query">Pagination and filtering parameters.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged product collection.</returns>
    [HttpGet("products")]
    [AllowAnonymous]
    public Task<PagedResult<ProductModel>> GetProducts([FromQuery] ProductListQuery query, CancellationToken cancellationToken) => mediator.Send(query, cancellationToken);

    /// <summary>Gets a product by identifier.</summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The product when it exists.</returns>
    [HttpGet("products/{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ProductModel>> GetProduct(Guid id, CancellationToken cancellationToken)
    {
        var product = await mediator.Send(new GetProductQuery(id), cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    /// <summary>Creates a product.</summary>
    /// <param name="command">The product payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created product.</returns>
    [HttpPost("products")]
    [Authorize(Policy = AuthorizationPolicies.Administrator)]
    public async Task<ActionResult<ProductModel>> CreateProduct(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var product = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    /// <summary>Updates a product.</summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="command">The product payload.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated product.</returns>
    [HttpPut("products/{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.Administrator)]
    public async Task<ActionResult<ProductModel>> UpdateProduct(Guid id, UpdateProductCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest();
        var product = await mediator.Send(command, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    /// <summary>Soft-deletes a product.</summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [HttpDelete("products/{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.Administrator)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteProduct(Guid id, CancellationToken cancellationToken) { await mediator.Send(new DeleteProductCommand(id), cancellationToken); return NoContent(); }

    /// <summary>Lists categories.</summary>
    [HttpGet("categories")]
    [AllowAnonymous]
    public Task<IReadOnlyList<CategoryModel>> GetCategories(CancellationToken cancellationToken) => mediator.Send(new GetCategoriesQuery(), cancellationToken);
    /// <summary>Lists brands.</summary>
    [HttpGet("brands")]
    [AllowAnonymous]
    public Task<IReadOnlyList<BrandModel>> GetBrands(CancellationToken cancellationToken) => mediator.Send(new GetBrandsQuery(), cancellationToken);
    /// <summary>Lists tags.</summary>
    [HttpGet("tags")]
    [AllowAnonymous]
    public Task<IReadOnlyList<TagModel>> GetTags(CancellationToken cancellationToken) => mediator.Send(new GetTagsQuery(), cancellationToken);
    /// <summary>Lists attributes and their values.</summary>
    [HttpGet("attributes")]
    [AllowAnonymous]
    public Task<IReadOnlyList<AttributeModel>> GetAttributes(CancellationToken cancellationToken) => mediator.Send(new GetAttributesQuery(), cancellationToken);
    /// <summary>Creates a category.</summary>
    [HttpPost("categories")]
    [Authorize(Policy = AuthorizationPolicies.Administrator)]
    public Task<CategoryModel> CreateCategory(CreateCategoryCommand command, CancellationToken cancellationToken) => mediator.Send(command, cancellationToken);
    /// <summary>Creates a brand.</summary>
    [HttpPost("brands")]
    [Authorize(Policy = AuthorizationPolicies.Administrator)]
    public Task<BrandModel> CreateBrand(CreateBrandCommand command, CancellationToken cancellationToken) => mediator.Send(command, cancellationToken);
    /// <summary>Creates a tag.</summary>
    [HttpPost("tags")]
    [Authorize(Policy = AuthorizationPolicies.Administrator)]
    public Task<TagModel> CreateTag(CreateTagCommand command, CancellationToken cancellationToken) => mediator.Send(command, cancellationToken);
}

/// <summary>Reads categories through MediatR.</summary>
public sealed record GetCategoriesQuery : IRequest<IReadOnlyList<CategoryModel>>;
/// <summary>Reads brands through MediatR.</summary>
public sealed record GetBrandsQuery : IRequest<IReadOnlyList<BrandModel>>;
/// <summary>Reads tags through MediatR.</summary>
public sealed record GetTagsQuery : IRequest<IReadOnlyList<TagModel>>;
/// <summary>Reads attributes through MediatR.</summary>
public sealed record GetAttributesQuery : IRequest<IReadOnlyList<AttributeModel>>;

/// <summary>Handles catalog read queries.</summary>
public sealed class CatalogQueryHandlers(ICatalogService catalog) : IRequestHandler<GetCategoriesQuery, IReadOnlyList<CategoryModel>>, IRequestHandler<GetBrandsQuery, IReadOnlyList<BrandModel>>, IRequestHandler<GetTagsQuery, IReadOnlyList<TagModel>>, IRequestHandler<GetAttributesQuery, IReadOnlyList<AttributeModel>>
{
    /// <inheritdoc />
    public Task<IReadOnlyList<CategoryModel>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken) => catalog.GetCategoriesAsync(cancellationToken);
    /// <inheritdoc />
    public Task<IReadOnlyList<BrandModel>> Handle(GetBrandsQuery request, CancellationToken cancellationToken) => catalog.GetBrandsAsync(cancellationToken);
    /// <inheritdoc />
    public Task<IReadOnlyList<TagModel>> Handle(GetTagsQuery request, CancellationToken cancellationToken) => catalog.GetTagsAsync(cancellationToken);
    /// <inheritdoc />
    public Task<IReadOnlyList<AttributeModel>> Handle(GetAttributesQuery request, CancellationToken cancellationToken) => catalog.GetAttributesAsync(cancellationToken);
}

namespace RemoteCommerce.Controllers.v1;

/// <summary>Exposes the RemoteCommerce catalog REST API.</summary>
[ApiController]
[Route("api/rc/v1")]
[Tags("Catalog")]
public sealed class CatalogController(IMediator mediator) : ControllerBase
{
    /// <summary>Lists catalog products with bounded pagination and filters.</summary>
    /// <param name="request">The product listing request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A standard result containing the paged product collection.</returns>
    [HttpGet("products")]
    [AllowAnonymous]
    [ProducesResponseType<Result<PagedResult<ProductModel>>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProducts(
        [FromQuery] ProductListRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new ProductListQuery(request),
            cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>Gets a product by identifier.</summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A standard result containing the product.</returns>
    [HttpGet("products/{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType<Result<ProductModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProduct(
        Guid id,
        CancellationToken cancellationToken)
    {
        var request = new ProductIdRequest
        {
            Id = id
        };
        var result = await mediator.Send(
            new GetProductQuery(request),
            cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>Creates a product.</summary>
    /// <param name="request">The product creation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A standard result containing the created product.</returns>
    [HttpPost("products")]
    [Authorize(Policy = AuthorizationPolicies.Administrator)]
    [ProducesResponseType<Result<ProductModel>>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateProduct(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateProductCommand(request),
            cancellationToken);
        if (!result.Succeeded)
        {
            return ToActionResult(result);
        }

        return CreatedAtAction(
            nameof(GetProduct),
            new { id = result.Value!.Id },
            result);
    }

    /// <summary>Updates a product.</summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="request">The product update request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A standard result containing the updated product.</returns>
    [HttpPut("products/{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.Administrator)]
    [ProducesResponseType<Result<ProductModel>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(
        Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        request.Id = id;
        var result = await mediator.Send(
            new UpdateProductCommand(request),
            cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>Soft-deletes a product.</summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A standard result without a response body.</returns>
    [HttpDelete("products/{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.Administrator)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteProduct(
        Guid id,
        CancellationToken cancellationToken)
    {
        var request = new ProductIdRequest
        {
            Id = id
        };
        var result = await mediator.Send(
            new DeleteProductCommand(request),
            cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>Lists categories.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A standard result containing the categories.</returns>
    [HttpGet("categories")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetCategoriesQuery(),
            cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>Gets a category.</summary>
    /// <param name="id">The category identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A standard result containing the category.</returns>
    [HttpGet("categories/{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategory(Guid id, CancellationToken cancellationToken)
    {
        var request = new ProductIdRequest
        {
            Id = id
        };
        var result = await mediator.Send(
            new GetCategoryQuery(request),
            cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>Creates a category.</summary>
    /// <param name="request">The category creation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A standard result containing the created category.</returns>
    [HttpPost("categories")]
    [Authorize(Policy = AuthorizationPolicies.Administrator)]
    public async Task<IActionResult> CreateCategory(
        [FromBody] CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateCategoryCommand(request),
            cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>Updates a category.</summary>
    /// <param name="id">The category identifier.</param>
    /// <param name="request">The category update request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A standard result containing the updated category.</returns>
    [HttpPut("categories/{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.Administrator)]
    public async Task<IActionResult> UpdateCategory(
        Guid id,
        [FromBody] UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        request.Id = id;
        var result = await mediator.Send(
            new UpdateCategoryCommand(request),
            cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>Soft-deletes a category.</summary>
    /// <param name="id">The category identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A standard result without a response body.</returns>
    [HttpDelete("categories/{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.Administrator)]
    public async Task<IActionResult> DeleteCategory(
        Guid id,
        CancellationToken cancellationToken)
    {
        var request = new ProductIdRequest
        {
            Id = id
        };
        var result = await mediator.Send(
            new DeleteCategoryCommand(request),
            cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>Lists brands.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A standard result containing the brands.</returns>
    [HttpGet("brands")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBrands(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetBrandsQuery(),
            cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>Gets a brand.</summary>
    /// <param name="id">The brand identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A standard result containing the brand.</returns>
    [HttpGet("brands/{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBrand(Guid id, CancellationToken cancellationToken)
    {
        var request = new ProductIdRequest
        {
            Id = id
        };
        var result = await mediator.Send(
            new GetBrandQuery(request),
            cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>Creates a brand.</summary>
    /// <param name="request">The brand creation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A standard result containing the created brand.</returns>
    [HttpPost("brands")]
    [Authorize(Policy = AuthorizationPolicies.Administrator)]
    public async Task<IActionResult> CreateBrand(
        [FromBody] CreateBrandRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateBrandCommand(request),
            cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>Updates a brand.</summary>
    /// <param name="id">The brand identifier.</param>
    /// <param name="request">The brand update request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A standard result containing the updated brand.</returns>
    [HttpPut("brands/{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.Administrator)]
    public async Task<IActionResult> UpdateBrand(
        Guid id,
        [FromBody] UpdateBrandRequest request,
        CancellationToken cancellationToken)
    {
        request.Id = id;
        var result = await mediator.Send(
            new UpdateBrandCommand(request),
            cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>Soft-deletes a brand.</summary>
    /// <param name="id">The brand identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A standard result without a response body.</returns>
    [HttpDelete("brands/{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.Administrator)]
    public async Task<IActionResult> DeleteBrand(
        Guid id,
        CancellationToken cancellationToken)
    {
        var request = new ProductIdRequest
        {
            Id = id
        };
        var result = await mediator.Send(
            new DeleteBrandCommand(request),
            cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>Lists tags.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A standard result containing the tags.</returns>
    [HttpGet("tags")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTags(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetTagsQuery(),
            cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>Gets a tag.</summary>
    /// <param name="id">The tag identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A standard result containing the tag.</returns>
    [HttpGet("tags/{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTag(Guid id, CancellationToken cancellationToken)
    {
        var request = new ProductIdRequest
        {
            Id = id
        };
        var result = await mediator.Send(
            new GetTagQuery(request),
            cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>Creates a tag.</summary>
    /// <param name="request">The tag creation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A standard result containing the created tag.</returns>
    [HttpPost("tags")]
    [Authorize(Policy = AuthorizationPolicies.Administrator)]
    public async Task<IActionResult> CreateTag(
        [FromBody] CreateTagRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateTagCommand(request),
            cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>Updates a tag.</summary>
    /// <param name="id">The tag identifier.</param>
    /// <param name="request">The tag update request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A standard result containing the updated tag.</returns>
    [HttpPut("tags/{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.Administrator)]
    public async Task<IActionResult> UpdateTag(
        Guid id,
        [FromBody] UpdateTagRequest request,
        CancellationToken cancellationToken)
    {
        request.Id = id;
        var result = await mediator.Send(
            new UpdateTagCommand(request),
            cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>Soft-deletes a tag.</summary>
    /// <param name="id">The tag identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A standard result without a response body.</returns>
    [HttpDelete("tags/{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.Administrator)]
    public async Task<IActionResult> DeleteTag(
        Guid id,
        CancellationToken cancellationToken)
    {
        var request = new ProductIdRequest
        {
            Id = id
        };
        var result = await mediator.Send(
            new DeleteTagCommand(request),
            cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>Lists product attributes.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A standard result containing the attributes.</returns>
    [HttpGet("attributes")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAttributes(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetAttributesQuery(),
            cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>Gets product variations.</summary>
    /// <param name="productId">The product identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A standard result containing the variations.</returns>
    [HttpGet("products/{productId:guid}/variations")]
    [AllowAnonymous]
    public async Task<IActionResult> GetVariations(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var request = new ProductIdRequest
        {
            Id = productId
        };
        var result = await mediator.Send(
            new ProductVariantListQuery(request),
            cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>Gets a product variation.</summary>
    /// <param name="productId">The product identifier.</param>
    /// <param name="variationId">The variation identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A standard result containing the variation.</returns>
    [HttpGet("products/{productId:guid}/variations/{variationId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetVariation(
        Guid productId,
        Guid variationId,
        CancellationToken cancellationToken)
    {
        var request = new ProductVariationIdRequest
        {
            ProductId = productId,
            VariationId = variationId
        };
        var result = await mediator.Send(
            new GetProductVariantQuery(request),
            cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>Creates a product variation.</summary>
    /// <param name="productId">The owning product identifier.</param>
    /// <param name="request">The variation creation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A standard result containing the created variation.</returns>
    [HttpPost("products/{productId:guid}/variations")]
    [Authorize(Policy = AuthorizationPolicies.Administrator)]
    public async Task<IActionResult> CreateVariation(
        Guid productId,
        [FromBody] CreateProductVariantRequest request,
        CancellationToken cancellationToken)
    {
        request.ProductId = productId;
        var result = await mediator.Send(
            new CreateProductVariantCommand(request),
            cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>Updates a product variation.</summary>
    /// <param name="productId">The owning product identifier.</param>
    /// <param name="variationId">The variation identifier.</param>
    /// <param name="request">The variation update request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A standard result containing the updated variation.</returns>
    [HttpPut("products/{productId:guid}/variations/{variationId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.Administrator)]
    public async Task<IActionResult> UpdateVariation(
        Guid productId,
        Guid variationId,
        [FromBody] UpdateProductVariantRequest request,
        CancellationToken cancellationToken)
    {
        request.ProductId = productId;
        request.Id = variationId;
        var result = await mediator.Send(
            new UpdateProductVariantCommand(request),
            cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>Deletes a product variation.</summary>
    /// <param name="productId">The owning product identifier.</param>
    /// <param name="variationId">The variation identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A standard result without a response body.</returns>
    [HttpDelete("products/{productId:guid}/variations/{variationId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.Administrator)]
    public async Task<IActionResult> DeleteVariation(
        Guid productId,
        Guid variationId,
        CancellationToken cancellationToken)
    {
        var request = new ProductVariationIdRequest
        {
            ProductId = productId,
            VariationId = variationId
        };
        var result = await mediator.Send(
            new DeleteProductVariantCommand(request),
            cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>Lists product metadata.</summary>
    /// <param name="productId">The product identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A standard result containing metadata.</returns>
    [HttpGet("products/{productId:guid}/metadata")]
    [AllowAnonymous]
    public async Task<IActionResult> GetMetadata(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var request = new ProductIdRequest
        {
            Id = productId
        };
        var result = await mediator.Send(
            new ProductMetadataQuery(request),
            cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>Creates or replaces product metadata.</summary>
    /// <param name="productId">The product identifier.</param>
    /// <param name="request">The metadata request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A standard result containing metadata.</returns>
    [HttpPost("products/{productId:guid}/metadata")]
    [Authorize(Policy = AuthorizationPolicies.Administrator)]
    public async Task<IActionResult> CreateMetadata(
        Guid productId,
        [FromBody] UpsertProductMetadataRequest request,
        CancellationToken cancellationToken)
    {
        request.ProductId = productId;
        var result = await mediator.Send(
            new UpsertProductMetadataCommand(request),
            cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>Replaces product metadata by key.</summary>
    /// <param name="productId">The product identifier.</param>
    /// <param name="key">The metadata key.</param>
    /// <param name="request">The metadata request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A standard result containing metadata.</returns>
    [HttpPut("products/{productId:guid}/metadata/{key}")]
    [Authorize(Policy = AuthorizationPolicies.Administrator)]
    public async Task<IActionResult> UpdateMetadata(
        Guid productId,
        string key,
        [FromBody] UpsertProductMetadataRequest request,
        CancellationToken cancellationToken)
    {
        request.ProductId = productId;
        request.Key = key;
        var result = await mediator.Send(
            new UpsertProductMetadataCommand(request),
            cancellationToken);
        return ToActionResult(result);
    }

    /// <summary>Deletes product metadata.</summary>
    /// <param name="productId">The product identifier.</param>
    /// <param name="key">The metadata key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A standard result without a response body.</returns>
    [HttpDelete("products/{productId:guid}/metadata/{key}")]
    [Authorize(Policy = AuthorizationPolicies.Administrator)]
    public async Task<IActionResult> DeleteMetadata(
        Guid productId,
        string key,
        CancellationToken cancellationToken)
    {
        var request = new ProductMetadataKeyRequest
        {
            ProductId = productId,
            Key = key
        };
        var result = await mediator.Send(
            new DeleteProductMetadataCommand(request),
            cancellationToken);
        return ToActionResult(result);
    }

    private IActionResult ToActionResult(Result result)
    {
        if (result.Succeeded)
        {
            return result.StatusCode == StatusCodes.Status204NoContent
                ? NoContent()
                : StatusCode(result.StatusCode);
        }

        return Problem(
            statusCode: result.StatusCode,
            title: result.ErrorCode,
            detail: result.ErrorMessage);
    }

    private IActionResult ToActionResult<T>(Result<T> result)
    {
        if (result.Succeeded)
        {
            return StatusCode(
                result.StatusCode,
                result);
        }

        return Problem(
            statusCode: result.StatusCode,
            title: result.ErrorCode,
            detail: result.ErrorMessage);
    }
}

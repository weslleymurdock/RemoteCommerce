namespace RemoteCommerce.Application.Catalog.Queries;

/// <summary>Gets a product by identifier.</summary>
public sealed record GetProductQuery(Guid Id) : IRequest<ProductModel?>;

/// <summary>Handles product detail queries.</summary>
public sealed class ProductQueryHandler(ICatalogService catalog) : IRequestHandler<GetProductQuery, ProductModel?>
{
    /// <inheritdoc />
    public Task<ProductModel?> Handle(GetProductQuery request, CancellationToken cancellationToken) => catalog.GetProductAsync(request.Id, cancellationToken);
}

namespace RemoteCommerce.Tests;

/// <summary>Verifies the Stage 08 catalog validation boundary.</summary>
public sealed class CatalogValidationTests
{
    /// <summary>Rejects non-normalized product slugs.</summary>
    [Fact]
    public async Task ProductValidator_RejectsInvalidSlug()
    {
        var validator = new CreateProductCommandValidator();
        var request = new CreateProductRequest
        {
            Name = "Product",
            Slug = "Not Valid",
            Price = 10m,
            Status = ProductStatus.Draft,
            ProductType = ProductType.Simple,
            Currency = "USD"
        };
        var result = await validator.ValidateAsync(
            new CreateProductCommand(request),
            TestContext.Current.CancellationToken);
        Assert.False(result.IsValid);
    }

    /// <summary>Rejects negative prices.</summary>
    [Fact]
    public async Task ProductValidator_RejectsNegativePrice()
    {
        var validator = new CreateProductCommandValidator();
        var request = new CreateProductRequest
        {
            Name = "Product",
            Slug = "product",
            Price = -1m,
            Status = ProductStatus.Draft,
            ProductType = ProductType.Simple,
            Currency = "USD"
        };
        var result = await validator.ValidateAsync(
            new CreateProductCommand(request),
            TestContext.Current.CancellationToken);
        Assert.False(result.IsValid);
    }

    /// <summary>Rejects secret-like metadata keys.</summary>
    [Fact]
    public async Task MetadataValidator_RejectsSecretKey()
    {
        var validator = new ProductMetadataValidator();
        var request = new UpsertProductMetadataRequest
        {
            ProductId = Guid.NewGuid(),
            Key = "api_token",
            Type = MetadataValueType.String,
            Value = "value"
        };
        var result = await validator.ValidateAsync(
            new UpsertProductMetadataCommand(request),
            TestContext.Current.CancellationToken);
        Assert.False(result.IsValid);
    }

    /// <summary>Rejects negative variant stock.</summary>
    [Fact]
    public async Task VariantValidator_RejectsNegativeStock()
    {
        var validator = new CreateProductVariantCommandValidator();
        var request = new CreateProductVariantRequest
        {
            ProductId = Guid.NewGuid(),
            Sku = "SKU-1",
            Price = 10m,
            StockQuantity = -1m,
            ManageStock = true,
            Status = ProductVariantStatus.Active
        };
        var result = await validator.ValidateAsync(
            new CreateProductVariantCommand(request),
            TestContext.Current.CancellationToken);
        Assert.False(result.IsValid);
    }
}

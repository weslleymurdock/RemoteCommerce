namespace RemoteCommerce.Tests;

/// <summary>Verifies the Stage 08 catalog validation boundary.</summary>
public sealed class CatalogValidationTests
{
    /// <summary>Rejects non-normalized product slugs.</summary>
    [Fact]
    public async Task ProductValidator_RejectsInvalidSlug()
    {
        var validator = new ProductCommandValidator();
        var result = await validator.ValidateAsync(new CreateProductCommand("Product", "Not Valid", null, string.Empty, string.Empty, ProductStatus.Draft, ProductType.Simple, 10m, null, "USD", null));
        Assert.False(result.IsValid);
    }

    /// <summary>Rejects negative prices.</summary>
    [Fact]
    public async Task ProductValidator_RejectsNegativePrice()
    {
        var validator = new ProductCommandValidator();
        var result = await validator.ValidateAsync(new CreateProductCommand("Product", "product", null, string.Empty, string.Empty, ProductStatus.Draft, ProductType.Simple, -1m, null, "USD", null));
        Assert.False(result.IsValid);
    }

    /// <summary>Rejects secret-like metadata keys.</summary>
    [Fact]
    public async Task MetadataValidator_RejectsSecretKey()
    {
        var validator = new ProductMetadataValidator();
        var result = await validator.ValidateAsync(new UpsertProductMetadataCommand(Guid.NewGuid(), "api_token", MetadataValueType.String, "value"));
        Assert.False(result.IsValid);
    }

    /// <summary>Rejects negative variant stock.</summary>
    [Fact]
    public async Task VariantValidator_RejectsNegativeStock()
    {
        var validator = new CreateProductVariantCommandValidator();
        var result = await validator.ValidateAsync(new CreateProductVariantCommand(Guid.NewGuid(), "SKU-1", 10m, null, -1m, true, ProductVariantStatus.Active));
        Assert.False(result.IsValid);
    }
}

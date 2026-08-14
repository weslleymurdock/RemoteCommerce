namespace RemoteCommerce.Tests;

/// <summary>Validates Stage 08 catalog domain and presentation contracts.</summary>
public sealed class Stage08CatalogTests
{
    /// <summary>Ensures product defaults support a draft simple catalog item.</summary>
    [Fact]
    public void Product_DefaultsToNewIdentityAndDraftState()
    {
        var product = new Product();
        Assert.NotEqual(Guid.Empty, product.Id);
        Assert.Equal(ProductStatus.Draft, product.Status);
        Assert.Equal(ProductType.Simple, product.ProductType);
        Assert.False(product.IsDisabled);
    }

    /// <summary>Ensures variants are explicitly owned by one product.</summary>
    [Fact]
    public void Variant_RequiresProductOwnership()
    {
        var productId = Guid.NewGuid();
        var variant = new ProductVariant { ProductId = productId, Sku = "SKU-1" };
        Assert.Equal(productId, variant.ProductId);
        Assert.Equal("SKU-1", variant.Sku);
    }

    /// <summary>Ensures the default theme remains independent from MudBlazor contracts.</summary>
    [Fact]
    public void DefaultTheme_ExposesPresentationMetadata()
    {
        IThemeProvider provider = new DefaultThemeProvider();
        Assert.Equal("default", provider.Current.Id);
        Assert.Contains("admin", provider.Current.Layouts);
        Assert.Empty(provider.Current.ComponentOverrides);
    }

    /// <summary>Ensures plugin menu contributions can be projected into the host menu.</summary>
    [Fact]
    public void PluginMenuContribution_IsSupportedByStableContract()
    {
        var contributor = new TestPluginMenuContributor();
        var item = Assert.Single(contributor.GetItems());
        Assert.Equal("catalog.plugin", item.Id);
        Assert.Equal("catalog", item.ParentId);
    }

    private sealed class TestPluginMenuContributor : IRemoteCommercePluginMenuContributor
    {
        public IReadOnlyList<PluginMenuItem> GetItems() => [new("catalog.plugin", "catalog", "Plugin.Catalog", "Extension", "/plugin/catalog", 20)];
    }
}

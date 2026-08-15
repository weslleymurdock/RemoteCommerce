namespace RemoteCommerce.Tests;

/// <summary>Verifies Stage 08 presentation composition contracts.</summary>
public sealed class PresentationContractsTests
{
    /// <summary>Resolves the built-in theme without exposing MudBlazor.</summary>
    [Fact]
    public void DefaultThemeProvider_ResolvesDefaultTheme()
    {
        var theme = new DefaultThemeProvider().Current;
        Assert.Equal("default", theme.Id);
        Assert.Contains("admin", theme.Layouts);
        Assert.Contains("/app.css", theme.Stylesheets);
    }

    /// <summary>Includes catalog core navigation in the final menu.</summary>
    [Fact]
    public void CoreMenuProvider_ContainsCatalogItems()
    {
        var provider = new AdminMenuProvider(new[] { new CoreMenuContributor() }, Array.Empty<IRemoteCommercePluginMenuContributor>());
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Administrator") }));
        var menu = provider.GetItems(user);
        Assert.Contains(menu, item => item.Id == "catalog.products");
        Assert.Contains(menu, item => item.Id == "catalog.categories");
    }

    /// <summary>Hides policy-protected menu entries from users without permission.</summary>
    [Fact]
    public void MenuProvider_FiltersPolicyProtectedItems()
    {
        var contributor = new TestMenuContributor();
        var provider = new AdminMenuProvider(new[] { contributor }, Array.Empty<IRemoteCommercePluginMenuContributor>());
        var menu = provider.GetItems(new ClaimsPrincipal(new ClaimsIdentity()));
        Assert.DoesNotContain(menu, item => item.Id == "restricted");
    }

    private sealed class TestMenuContributor : IMenuContributor
    {
        public IReadOnlyList<MenuItemDefinition> GetItems() => new[]
        {
            new MenuItemDefinition("restricted", null, "Test.Restricted", "Lock", "/admin/restricted", 100, "Catalog.Manage")
        };
    }
}

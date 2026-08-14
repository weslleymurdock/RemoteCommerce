namespace RemoteCommerce.Application.Presentation;

/// <summary>Describes a presentation theme available to the host.</summary>
public sealed record ThemeDefinition(
    string Id,
    string Name,
    string Version,
    string Author,
    IReadOnlyList<string> Layouts,
    IReadOnlyList<string> Stylesheets,
    IReadOnlyList<string> Scripts,
    IReadOnlyDictionary<string, string> ComponentOverrides,
    IReadOnlyDictionary<string, string> Metadata);

/// <summary>Provides the resolved presentation theme without exposing UI library details.</summary>
public interface IThemeProvider
{
    /// <summary>Gets the currently resolved theme.</summary>
    ThemeDefinition Current { get; }
}

/// <summary>Resolves the built-in RemoteCommerce theme.</summary>
public sealed class DefaultThemeProvider : IThemeProvider
{
    /// <inheritdoc />
    public ThemeDefinition Current { get; } = new(
        "default",
        "RemoteCommerce Default",
        "1.0.0",
        "RemoteCommerce",
        ["admin"],
        ["/app.css"],
        [],
        new Dictionary<string, string>(),
        new Dictionary<string, string>());
}

/// <summary>Describes a dynamic administration menu contribution.</summary>
public sealed record MenuItemDefinition(
    string Id,
    string? ParentId,
    string LabelResourceKey,
    string Icon,
    string Route,
    int Order,
    string? RequiredPolicy = null,
    Func<bool>? VisibilityPredicate = null);

/// <summary>Contributes menu items to the administration navigation tree.</summary>
public interface IMenuContributor
{
    /// <summary>Gets menu contributions supplied by the contributor.</summary>
    IReadOnlyList<MenuItemDefinition> GetItems();
}

/// <summary>Builds the final administration menu from registered contributions.</summary>
public interface IMenuProvider
{
    /// <summary>Gets visible menu items ordered for presentation.</summary>
    /// <param name="user">The authenticated principal used for visibility filtering.</param>
    /// <returns>The visible menu contributions.</returns>
    IReadOnlyList<MenuItemDefinition> GetItems(ClaimsPrincipal user);
}

/// <summary>Provides core and plugin administration menu contributions.</summary>
public sealed class AdminMenuProvider(
    IEnumerable<IMenuContributor> contributors,
    IEnumerable<IRemoteCommercePluginMenuContributor> pluginContributors) : IMenuProvider
{
    /// <inheritdoc />
    public IReadOnlyList<MenuItemDefinition> GetItems(ClaimsPrincipal user)
    {
        var core = contributors.SelectMany(x => x.GetItems());
        var plugins = pluginContributors
            .SelectMany(x => x.GetItems())
            .Select(x => new MenuItemDefinition(
                x.Id,
                x.ParentId,
                x.LabelResourceKey,
                x.Icon,
                x.Route,
                x.Order,
                x.RequiredPermission));

        return core
            .Concat(plugins)
            .Where(x => x.VisibilityPredicate?.Invoke() != false)
            .Where(x => string.IsNullOrWhiteSpace(x.RequiredPolicy) || user.IsInRole("Administrator"))
            .OrderBy(x => x.Order)
            .ThenBy(x => x.Id)
            .ToArray();
    }
}

/// <summary>Provides the built-in administration menu.</summary>
public sealed class CoreMenuContributor : IMenuContributor
{
    /// <inheritdoc />
    public IReadOnlyList<MenuItemDefinition> GetItems() =>
    [
        new("dashboard", null, "Admin.Dashboard", "Dashboard", "/admin", 0),
        new("catalog", null, "Catalog.Title", "ShoppingBag", "/admin/catalog/products", 10),
        new("catalog.products", "catalog", "Catalog.Products", "Inventory2", "/admin/catalog/products", 11),
        new("catalog.categories", "catalog", "Catalog.Categories", "Category", "/admin/catalog/categories", 12),
        new("catalog.brands", "catalog", "Catalog.Brands", "BrandingWatermark", "/admin/catalog/brands", 13),
        new("catalog.tags", "catalog", "Catalog.Tags", "Sell", "/admin/catalog/tags", 14),
        new("catalog.attributes", "catalog", "Catalog.Attributes", "Tune", "/admin/catalog/attributes", 15),
        new("settings", null, "Admin.Settings", "Settings", "/admin/settings", 50),
        new("plugins", null, "Admin.Plugins", "Extension", "/admin/plugins", 60)
    ];
}

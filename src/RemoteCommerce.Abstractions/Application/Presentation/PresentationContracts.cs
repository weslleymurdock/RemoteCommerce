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

/// <summary>Provides the resolved presentation theme without exposing a UI library.</summary>
public interface IThemeProvider
{
    /// <summary>Gets the currently resolved theme.</summary>
    ThemeDefinition Current { get; }
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
    IReadOnlyList<MenuItemDefinition> GetItems(
        ClaimsPrincipal user);
}

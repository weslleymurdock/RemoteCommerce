namespace RemoteCommerce.Application.Administration;

/// <summary>Describes a navigable administration menu item.</summary>
/// <param name="Title">The display title.</param>
/// <param name="Href">The local navigation URL.</param>
/// <param name="Icon">The MudBlazor icon name.</param>
/// <param name="Order">The display order.</param>
public sealed record AdminNavigationItem(string Title, string Href, string Icon, int Order = 0);

/// <summary>Provides an extensible registration boundary for administration navigation.</summary>
public interface IAdminNavigationRegistry
{
    /// <summary>Registers an administration navigation item.</summary>
    /// <param name="item">The navigation item to register.</param>
    void Register(AdminNavigationItem item);

    /// <summary>Gets registered administration navigation items in display order.</summary>
    /// <returns>The current navigation item collection.</returns>
    IReadOnlyList<AdminNavigationItem> GetItems();
}

/// <summary>Stores administration navigation contributions for the current host.</summary>
public sealed class AdminNavigationRegistry : IAdminNavigationRegistry
{
    private readonly List<AdminNavigationItem> items = [];

    /// <summary>Registers an administration navigation item.</summary>
    /// <param name="item">The navigation item to register.</param>
    public void Register(AdminNavigationItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        items.RemoveAll(x => string.Equals(x.Href, item.Href, StringComparison.OrdinalIgnoreCase));
        items.Add(item);
    }

    /// <summary>Gets registered administration navigation items in display order.</summary>
    /// <returns>The current navigation item collection.</returns>
    public IReadOnlyList<AdminNavigationItem> GetItems() => items.OrderBy(x => x.Order).ThenBy(x => x.Title).ToArray();
}

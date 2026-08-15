namespace RemoteCommerce.Plugins.Abstractions;

/// <summary>Describes an administration menu item contributed by a plugin.</summary>
public sealed record PluginMenuItem(string Id, string? ParentId, string LabelResourceKey, string Icon, string Route, int Order, string? RequiredPermission = null);

/// <summary>Allows an installed plugin to contribute administration navigation without referencing host UI components.</summary>
public interface IRemoteCommercePluginMenuContributor
{
    /// <summary>Gets the plugin's current administration menu contributions.</summary>
    IReadOnlyList<PluginMenuItem> GetItems();
}

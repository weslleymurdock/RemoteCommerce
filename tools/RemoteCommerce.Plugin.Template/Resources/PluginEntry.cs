using Microsoft.Extensions.DependencyInjection;
using RemoteCommerce.Plugins.Abstractions;

namespace {{Namespace}};

/// <summary>
/// Provides the generated entry point for the RemoteCommerce plugin.
/// </summary>
public sealed class PluginEntry : IRemoteCommercePlugin
{
    /// <summary>
    /// Registers the generated plugin services, controllers, and Razor components.
    /// </summary>
    /// <param name="services">The host service collection.</param>
    /// <param name="manifest">The installed plugin manifest.</param>
    public void ConfigureServices(IServiceCollection services, PluginManifest manifest)
    {
        services.AddControllers().AddApplicationPart(typeof(PluginEntry).Assembly);
        services.AddRazorComponents().AddAdditionalAssemblies(typeof(PluginEntry).Assembly);
    }
}

using Microsoft.Extensions.DependencyInjection;

namespace RemoteCommerce.Plugins.Abstractions;

/// <summary>
/// Defines the entry point used by a RemoteCommerce plugin to register its services.
/// </summary>
public interface IRemoteCommercePlugin
{
    /// <summary>
    /// Configures plugin services before the application host is built.
    /// </summary>
    /// <param name="services">The application service collection to extend.</param>
    /// <param name="manifest">The manifest describing the plugin being loaded.</param>
    void ConfigureServices(IServiceCollection services, PluginManifest manifest);
}

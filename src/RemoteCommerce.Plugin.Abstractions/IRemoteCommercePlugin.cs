using Microsoft.Extensions.DependencyInjection;

namespace RemoteCommerce.Plugins.Abstractions;

/// <summary>
/// Defines the entry point used by a RemoteCommerce plugin to register services with the host.
/// </summary>
public interface IRemoteCommercePlugin
{
    /// <summary>
    /// Registers the services and extension points supplied by the plugin.
    /// </summary>
    /// <param name="services">The application service collection that the plugin may extend.</param>
    /// <param name="manifest">The manifest describing the plugin package being loaded.</param>
    /// <remarks>
    /// This method is called during application startup before the final dependency injection container is built.
    /// Implementations should register only services owned by the plugin and should not build a service provider.
    /// </remarks>
    void ConfigureServices(IServiceCollection services, PluginManifest manifest);
}

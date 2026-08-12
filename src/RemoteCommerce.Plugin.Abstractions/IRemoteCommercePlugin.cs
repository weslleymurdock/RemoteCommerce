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
    /// <param name="configuration">The host application configuration, including the host's configured default values.</param>
    /// <remarks>
    /// This method is called during application startup before the final dependency injection container is built.
    /// Implementations should register only services owned by the plugin and should not build a service provider.
    /// The supplied configuration is the host configuration and can be used to read existing application settings.
    /// </remarks>
    void ConfigureServices(IServiceCollection services, PluginManifest manifest, IConfiguration configuration);
}

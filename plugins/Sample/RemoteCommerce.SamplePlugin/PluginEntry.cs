using Microsoft.Extensions.DependencyInjection;
using RemoteCommerce.Plugins.Abstractions;

namespace RemoteCommerce.SamplePlugin;

/// <summary>
/// Provides the reference plugin implementation used to verify startup discovery and dependency injection.
/// </summary>
public sealed class PluginEntry : IRemoteCommercePlugin
{
    /// <summary>
    /// Registers the sample plugin marker service.
    /// </summary>
    /// <param name="services">The application service collection.</param>
    /// <param name="manifest">The manifest describing this plugin.</param>
    public void ConfigureServices(IServiceCollection services, PluginManifest manifest)
    {
        services.AddSingleton(new SamplePluginRegistration(manifest.Id, manifest.Version));
    }
}

/// <summary>
/// Represents the registration created by the reference plugin.
/// </summary>
/// <param name="PluginId">The stable plugin identifier.</param>
/// <param name="Version">The loaded plugin version.</param>
public sealed record SamplePluginRegistration(string PluginId, string Version);

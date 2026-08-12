using Microsoft.Extensions.DependencyInjection;

namespace RemoteCommerce.Plugins.Abstractions;

public interface IRemoteCommercePlugin
{
    void ConfigureServices(IServiceCollection services, PluginManifest manifest);
}

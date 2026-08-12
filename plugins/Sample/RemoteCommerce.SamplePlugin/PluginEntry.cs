using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using RemoteCommerce.Plugins.Abstractions;

namespace RemoteCommerce.SamplePlugin;

/// <summary>
/// Provides the reference plugin implementation used to verify startup discovery and dependency injection.
/// </summary>
public sealed class PluginEntry : IRemoteCommercePlugin
{
    /// <summary>
    /// Registers the sample plugin marker service and MVC application part.
    /// </summary>
    /// <param name="services">The application service collection.</param>
    /// <param name="manifest">The manifest describing this plugin.</param>
    public void ConfigureServices(IServiceCollection services, PluginManifest manifest)
    {
        services.AddSingleton(new SamplePluginRegistration(manifest.Id, manifest.Version));
        services.AddControllers().AddApplicationPart(typeof(PluginEntry).Assembly);
    }
}

/// <summary>
/// Represents the registration created by the reference plugin.
/// </summary>
/// <param name="PluginId">The stable plugin identifier.</param>
/// <param name="Version">The loaded plugin version.</param>
public sealed record SamplePluginRegistration(string PluginId, string Version);

/// <summary>
/// Exposes a minimal endpoint proving that the sample plugin assembly entered the host during startup.
/// </summary>
[ApiController]
[Route("api/v1/sample-plugin")]
public sealed class SamplePluginController : ControllerBase
{
    private readonly SamplePluginRegistration _registration;

    /// <summary>
    /// Initializes the sample plugin controller.
    /// </summary>
    /// <param name="registration">The registration created by the plugin entry point.</param>
    public SamplePluginController(SamplePluginRegistration registration)
    {
        _registration = registration;
    }

    /// <summary>
    /// Gets the identity of the loaded sample plugin.
    /// </summary>
    /// <returns>The loaded plugin identifier and version.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(SamplePluginRegistration), StatusCodes.Status200OK)]
    public ActionResult<SamplePluginRegistration> Get() => Ok(_registration);
}

using Microsoft.AspNetCore.Mvc;

namespace RemoteCommerce_SamplePlugin.Controllers;

/// <summary>
/// Provides the generated plugin API endpoint.
/// </summary>
[ApiController]
[Route("api/rp/v1/sample")]
public sealed class PluginController : ControllerBase
{
    /// <summary>
    /// Returns basic plugin information.
    /// </summary>
    /// <returns>The plugin identifier and version.</returns>
    [HttpGet]
    public object Get() => new { PluginId = "remotecommerce_sampleplugin", Version = "1.0.0" };
}

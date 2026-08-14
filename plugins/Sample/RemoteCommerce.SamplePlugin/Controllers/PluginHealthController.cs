namespace RemoteCommerce_SamplePlugin.Controllers;

/// <summary>
/// Provides the standard health endpoint for the generated RemoteCommerce plugin.
/// </summary>
[ApiController]
[Route("api/rp/v1/sample")]
public sealed class PluginHealthController : ControllerBase
{
    /// <summary>
    /// Returns the health status of the plugin.
    /// </summary>
    /// <returns>A healthy status payload containing the plugin identifier and version.</returns>
    [HttpGet("health")]
    public object GetHealth() => new
    {
        Status = "Healthy",
        PluginId = "remotecommerce_sampleplugin",
        Version = "1.0.0"
    };
}

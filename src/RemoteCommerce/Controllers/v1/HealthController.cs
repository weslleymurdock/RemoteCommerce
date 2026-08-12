using Microsoft.AspNetCore.Mvc;

namespace RemoteCommerce.Controllers.v1;

/// <summary>
/// Exposes a health check endpoint for the RemoteCommerce application.
/// </summary>
[ApiController]
[Route("api/v1/health")]
[Tags("Healtcheck")]
public sealed class HealthController : ControllerBase
{
    /// <summary>
    /// Returns the health status of the RemoteCommerce application.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        status = "ok",
        application = "RemoteCommerce",
        version = typeof(Program).Assembly.GetName().Version?.ToString()
    });
}

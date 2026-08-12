using Microsoft.AspNetCore.Mvc;

namespace RemoteCommerce.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        status = "ok",
        application = "RemoteCommerce",
        version = typeof(Program).Assembly.GetName().Version?.ToString()
    });
}

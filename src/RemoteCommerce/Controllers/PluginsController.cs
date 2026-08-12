using Microsoft.AspNetCore.Mvc;
using RemoteCommerce.Plugins;

namespace RemoteCommerce.Controllers;

/// <summary>
/// Exposes administrative endpoints for RemoteCommerce plugin installation.
/// </summary>
[ApiController]
[Route("api/plugins")]
public sealed class PluginsController(PluginInstallationService installationService) : ControllerBase
{
    /// <summary>
    /// Installs a plugin package from a server-local directory.
    /// </summary>
    /// <param name="sourceDirectory">The directory containing the plugin package.</param>
    /// <param name="cancellationToken">The token used to cancel the installation.</param>
    /// <returns>The manifest of the installed plugin.</returns>
    [HttpPost("install")]
    [ProducesResponseType(typeof(InstallPluginResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<InstallPluginResponse>> Install(
        [FromQuery] string sourceDirectory,
        CancellationToken cancellationToken)
    {
        try
        {
            var manifest = await installationService.InstallAsync(sourceDirectory, cancellationToken);
            return Created(string.Empty, new InstallPluginResponse(manifest.Id, manifest.Version));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Plugin installation failed.", Detail = exception.Message });
        }
        catch (DirectoryNotFoundException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Plugin source directory was not found.", Detail = exception.Message });
        }
    }

    /// <summary>
    /// Represents the result of a successful plugin installation.
    /// </summary>
    /// <param name="PluginId">The stable plugin identifier.</param>
    /// <param name="Version">The installed plugin version.</param>
    public sealed record InstallPluginResponse(string PluginId, string Version);
}

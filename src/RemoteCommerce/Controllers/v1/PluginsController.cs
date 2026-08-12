using Microsoft.AspNetCore.Mvc;
using RemoteCommerce.Infrastructure.Persistence.Entities;
using RemoteCommerce.Plugins;

namespace RemoteCommerce.Controllers.v1;

/// <summary>
/// Exposes administrative endpoints for the RemoteCommerce plugin lifecycle.
/// </summary>
[ApiController]
[Route("api/v1/plugins")]
[Tags("Plugins")]
public sealed class PluginsController(
    PluginInstallationService installationService,
    PluginManagementService managementService) : ControllerBase
{
    /// <summary>
    /// Gets the plugins currently persisted by the host.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The persisted plugin installations.</returns>
    /// <response code="200">The plugin list was returned successfully.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PluginInstallation>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PluginInstallation>>> List(CancellationToken cancellationToken)
        => Ok(await managementService.ListAsync(cancellationToken));

    /// <summary>
    /// Installs a RemoteCommerce plugin from a NuGet package.
    /// </summary>
    /// <param name="package">The <c>.nupkg</c> plugin package.</param>
    /// <param name="cancellationToken">The token used to cancel installation.</param>
    /// <returns>The manifest of the installed plugin.</returns>
    /// <response code="201">The package was installed and will be active after restart.</response>
    /// <response code="400">The package could not be validated or installed.</response>
    [HttpPost("install")]
    [RequestSizeLimit(100_000_000)]
    [ProducesResponseType(typeof(InstallPluginResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<InstallPluginResponse>> Install(IFormFile package, CancellationToken cancellationToken)
    {
        if (package.Length == 0 || !package.FileName.EndsWith(".nupkg", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new ProblemDetails { Title = "Invalid plugin package.", Detail = "The uploaded file must be a non-empty .nupkg file." });

        var temporaryDirectory = Path.Combine(Path.GetTempPath(), "RemoteCommerce", "plugins");
        Directory.CreateDirectory(temporaryDirectory);
        var temporaryPath = Path.Combine(temporaryDirectory, $"{Guid.NewGuid():N}.nupkg");

        try
        {
            await using (var output = System.IO.File.Create(temporaryPath))
                await package.CopyToAsync(output, cancellationToken);

            var manifest = await installationService.InstallAsync(temporaryPath, cancellationToken);
            return CreatedAtAction(nameof(List), null, new InstallPluginResponse(manifest.Id, manifest.Version, true));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Plugin installation failed.", Detail = exception.Message });
        }
        finally
        {
            if (System.IO.File.Exists(temporaryPath))
                System.IO.File.Delete(temporaryPath);
        }
    }

    /// <summary>
    /// Disables a plugin for the next application startup.
    /// </summary>
    /// <param name="pluginId">The stable plugin identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>No content when the operation succeeds.</returns>
    /// <response code="204">The plugin was disabled.</response>
    /// <response code="404">The plugin was not installed.</response>
    [HttpPost("{pluginId}/disable")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Disable(string pluginId, CancellationToken cancellationToken)
        => await ExecuteLifecycleOperation(() => managementService.DisableAsync(pluginId, cancellationToken));

    /// <summary>
    /// Enables a plugin for the next application startup.
    /// </summary>
    /// <param name="pluginId">The stable plugin identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>No content when the operation succeeds.</returns>
    /// <response code="204">The plugin was enabled.</response>
    /// <response code="404">The plugin was not installed.</response>
    [HttpPost("{pluginId}/enable")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Enable(string pluginId, CancellationToken cancellationToken)
        => await ExecuteLifecycleOperation(() => managementService.EnableAsync(pluginId, cancellationToken));

    /// <summary>
    /// Uninstalls a plugin and schedules its files for deletion on the next application startup.
    /// </summary>
    /// <param name="pluginId">The stable plugin identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>No content when the operation succeeds.</returns>
    /// <response code="204">The plugin was uninstalled.</response>
    /// <response code="404">The plugin was not installed.</response>
    [HttpDelete("{pluginId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Uninstall(string pluginId, CancellationToken cancellationToken)
        => await ExecuteLifecycleOperation(() => managementService.UninstallAsync(pluginId, cancellationToken));

    private async Task<IActionResult> ExecuteLifecycleOperation(Func<Task> operation)
    {
        try
        {
            await operation();
            return NoContent();
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new ProblemDetails { Title = "Plugin was not found.", Detail = exception.Message });
        }
    }

    /// <summary>
    /// Represents the result of a successful plugin installation.
    /// </summary>
    /// <param name="PluginId">The stable plugin identifier.</param>
    /// <param name="Version">The installed plugin version.</param>
    /// <param name="RestartRequired">Indicates that the application must restart before the plugin enters the dependency injection container.</param>
    public sealed record InstallPluginResponse(string PluginId, string Version, bool RestartRequired);
}

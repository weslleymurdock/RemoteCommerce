namespace RemoteCommerce.Controllers.v1;

/// <summary>Exposes the plugin administration HTTP boundary.</summary>
[ApiController]
[Authorize(Policy = AuthorizationPolicies.ManagePlugins)]
[Route("api/rp/v1/plugins")]
[Tags("Plugins")]
public sealed class PluginsController(IMediator mediator) : ControllerBase
{
    /// <summary>Gets the plugins currently persisted by the host.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The persisted plugin administration records.</returns>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PluginInformation>>> List(CancellationToken cancellationToken) => Ok(await mediator.Send(new ListPluginsQuery(), cancellationToken));
    /// <summary>Gets whether a host restart is required for a pending plugin lifecycle change.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The current restart requirement.</returns>
    [HttpGet("restart-status")]
    public async Task<ActionResult<ApplicationRestartStatus>> RestartStatus(CancellationToken cancellationToken) => Ok(await mediator.Send(new GetPluginRestartStatusQuery(), cancellationToken));
    /// <summary>Validates a RemoteCommerce plugin NuGet package without installing it.</summary>
    /// <param name="package">The candidate package.</param><param name="cancellationToken">The cancellation token.</param>
    /// <returns>The package validation result.</returns>
    [HttpPost("validate"), RequestSizeLimit(100_000_000)]
    public async Task<ActionResult<PluginPackageValidationResult>> Validate(IFormFile package, CancellationToken cancellationToken)
    {
        if (!IsPackage(package)) return InvalidPackage<PluginPackageValidationResult>();
        return await WithTemporaryPackageAsync(package, async path => Ok(await mediator.Send(new ValidatePluginPackageQuery(path), cancellationToken)), cancellationToken);
    }
    /// <summary>Installs a RemoteCommerce plugin package and schedules activation after restart.</summary>
    /// <param name="package">The plugin package.</param><param name="cancellationToken">The cancellation token.</param>
    /// <returns>The installed plugin manifest.</returns>
    [HttpPost("install"), RequestSizeLimit(100_000_000)]
    public async Task<ActionResult<InstallPluginResponse>> Install(IFormFile package, CancellationToken cancellationToken)
    {
        if (!IsPackage(package)) return InvalidPackage<InstallPluginResponse>();
        var manifest = await WithTemporaryPackageAsync(package, path => mediator.Send(new InstallPluginCommand(path), cancellationToken), cancellationToken);
        return CreatedAtAction(nameof(List), null, new InstallPluginResponse(manifest.Id, manifest.Version, true));
    }
    /// <summary>Updates an installed plugin with a newer compatible package.</summary>
    /// <param name="pluginId">The plugin identifier.</param><param name="package">The newer package.</param><param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated plugin manifest.</returns>
    [HttpPost("{pluginId}/update"), RequestSizeLimit(100_000_000)]
    public async Task<ActionResult<UpdatePluginResponse>> Update(string pluginId, IFormFile package, CancellationToken cancellationToken)
    {
        if (!IsPackage(package)) return InvalidPackage<UpdatePluginResponse>();
        var manifest = await WithTemporaryPackageAsync(package, path => mediator.Send(new UpdatePluginCommand(pluginId, path), cancellationToken), cancellationToken);
        return Ok(new UpdatePluginResponse(manifest.Id, manifest.Version, true));
    }
    /// <summary>Disables a plugin for the next application startup.</summary>
    /// <param name="pluginId">The plugin identifier.</param><param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content when successful.</returns>
    [HttpPost("{pluginId}/disable")]
    public Task<IActionResult> Disable(string pluginId, CancellationToken cancellationToken) => ExecuteLifecycleOperation(() => mediator.Send(new DisablePluginCommand(pluginId), cancellationToken));
    /// <summary>Enables a plugin for the next application startup.</summary>
    /// <param name="pluginId">The plugin identifier.</param><param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content when successful.</returns>
    [HttpPost("{pluginId}/enable")]
    public Task<IActionResult> Enable(string pluginId, CancellationToken cancellationToken) => ExecuteLifecycleOperation(() => mediator.Send(new EnablePluginCommand(pluginId), cancellationToken));
    /// <summary>Uninstalls a plugin when no installed plugin requires it.</summary>
    /// <param name="pluginId">The plugin identifier.</param><param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content when successful.</returns>
    [HttpDelete("{pluginId}")]
    public Task<IActionResult> Uninstall(string pluginId, CancellationToken cancellationToken) => ExecuteLifecycleOperation(() => mediator.Send(new UninstallPluginCommand(pluginId), cancellationToken));
    private static bool IsPackage(IFormFile package) => package is not null && package.Length > 0 && package.FileName.EndsWith(".nupkg", StringComparison.OrdinalIgnoreCase);
    private static ActionResult<T> InvalidPackage<T>() => new(new BadRequestObjectResult(new ProblemDetails { Title = "Invalid plugin package.", Detail = "The uploaded file must be a non-empty .nupkg file." }));
    private static async Task<T> WithTemporaryPackageAsync<T>(IFormFile package, Func<string, Task<T>> operation, CancellationToken cancellationToken)
    {
        var directory = Path.Combine(Path.GetTempPath(), "RemoteCommerce", "plugins"); Directory.CreateDirectory(directory); var path = Path.Combine(directory, $"{Guid.NewGuid():N}.nupkg");
        try { await using (var output = System.IO.File.Create(path)) await package.CopyToAsync(output, cancellationToken); return await operation(path); }
        finally { if (System.IO.File.Exists(path)) System.IO.File.Delete(path); }
    }
    private static async Task<IActionResult> ExecuteLifecycleOperation(Func<Task<Unit>> operation)
    {
        try { await operation(); return new NoContentResult(); }
        catch (KeyNotFoundException exception) { return new NotFoundObjectResult(new ProblemDetails { Title = "Plugin was not found.", Detail = exception.Message }); }
        catch (InvalidOperationException exception) { return new BadRequestObjectResult(new ProblemDetails { Title = "Plugin lifecycle operation failed.", Detail = exception.Message }); }
    }
    /// <summary>Represents a successful plugin installation.</summary>
    /// <param name="PluginId">The plugin identifier.</param><param name="Version">The installed version.</param><param name="RestartRequired">Whether restart is required.</param>
    public sealed record InstallPluginResponse(string PluginId, string Version, bool RestartRequired);
    /// <summary>Represents a successful plugin update.</summary>
    /// <param name="PluginId">The plugin identifier.</param><param name="Version">The selected version.</param><param name="RestartRequired">Whether restart is required.</param>
    public sealed record UpdatePluginResponse(string PluginId, string Version, bool RestartRequired);
}

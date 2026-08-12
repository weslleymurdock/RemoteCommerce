namespace RemoteCommerce.Controllers.v1;

/// <summary>Exposes administrative endpoints for the RemoteCommerce plugin lifecycle.</summary>
[ApiController]
[Route("api/v1/plugins")]
[Tags("Plugins")]
public sealed class PluginsController(IMediator mediator, PluginManagementService managementService, IApplicationRestartService restartService) : ControllerBase
{
    /// <summary>Gets the plugins currently persisted by the host.</summary>
    /// <param name="cancellationToken">The token used to cancel the query.</param>
    /// <returns>The persisted plugin administration records.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PluginInformation>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PluginInformation>>> List(CancellationToken cancellationToken) => Ok(await managementService.ListAsync(cancellationToken));

    /// <summary>Gets whether a host restart is required for a pending plugin lifecycle change.</summary>
    /// <returns>The current restart requirement.</returns>
    [HttpGet("restart-status")]
    [ProducesResponseType(typeof(ApplicationRestartStatus), StatusCodes.Status200OK)]
    public ActionResult<ApplicationRestartStatus> RestartStatus() => Ok(restartService.Status);

    /// <summary>Validates a RemoteCommerce plugin NuGet package without installing it.</summary>
    /// <param name="package">The candidate <c>.nupkg</c> plugin package.</param>
    /// <param name="cancellationToken">The token used to cancel validation.</param>
    /// <returns>The package validation result.</returns>
    [HttpPost("validate")]
    [RequestSizeLimit(100_000_000)]
    [ProducesResponseType(typeof(PluginPackageValidationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PluginPackageValidationResult>> Validate(IFormFile package, CancellationToken cancellationToken)
    {
        if (!IsPackage(package)) return InvalidPackage<PluginPackageValidationResult>();
        return await WithTemporaryPackageAsync(package, async path => Ok(await mediator.Send(new ValidatePluginPackageQuery(path), cancellationToken)), cancellationToken);
    }

    /// <summary>Installs a RemoteCommerce plugin from a NuGet package and schedules activation after restart.</summary>
    /// <param name="package">The <c>.nupkg</c> plugin package.</param>
    /// <param name="cancellationToken">The token used to cancel installation.</param>
    /// <returns>The installed plugin manifest.</returns>
    [HttpPost("install")]
    [RequestSizeLimit(100_000_000)]
    [ProducesResponseType(typeof(InstallPluginResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<InstallPluginResponse>> Install(IFormFile package, CancellationToken cancellationToken)
    {
        if (!IsPackage(package)) return InvalidPackage<InstallPluginResponse>();
        try
        {
            return await WithTemporaryPackageAsync(package, async path =>
            {
                var manifest = await mediator.Send(new InstallPluginCommand(path), cancellationToken);
                return CreatedAtAction(nameof(List), null, new InstallPluginResponse(manifest.Id, manifest.Version, true));
            }, cancellationToken);
        }
        catch (InvalidOperationException exception) { return BadRequest(new ProblemDetails { Title = "Plugin installation failed.", Detail = exception.Message }); }
    }

    /// <summary>Updates an installed plugin with a newer compatible package while retaining the previous version.</summary>
    /// <param name="pluginId">The stable plugin identifier.</param>
    /// <param name="package">The newer <c>.nupkg</c> package.</param>
    /// <param name="cancellationToken">The token used to cancel the update.</param>
    /// <returns>The updated plugin manifest.</returns>
    [HttpPost("{pluginId}/update")]
    [RequestSizeLimit(100_000_000)]
    [ProducesResponseType(typeof(UpdatePluginResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<UpdatePluginResponse>> Update(string pluginId, IFormFile package, CancellationToken cancellationToken)
    {
        if (!IsPackage(package)) return InvalidPackage<UpdatePluginResponse>();
        try
        {
            return await WithTemporaryPackageAsync(package, async path =>
            {
                var manifest = await mediator.Send(new UpdatePluginCommand(pluginId, path), cancellationToken);
                return Ok(new UpdatePluginResponse(manifest.Id, manifest.Version, true));
            }, cancellationToken);
        }
        catch (KeyNotFoundException exception) { return NotFound(new ProblemDetails { Title = "Plugin was not found.", Detail = exception.Message }); }
        catch (InvalidOperationException exception) { return BadRequest(new ProblemDetails { Title = "Plugin update failed.", Detail = exception.Message }); }
    }

    /// <summary>Disables a plugin for the next application startup.</summary>
    /// <param name="pluginId">The stable plugin identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>No content when the operation succeeds.</returns>
    [HttpPost("{pluginId}/disable")]
    public Task<IActionResult> Disable(string pluginId, CancellationToken cancellationToken) => ExecuteLifecycleOperation(() => mediator.Send(new DisablePluginCommand(pluginId), cancellationToken));

    /// <summary>Enables a plugin for the next application startup.</summary>
    /// <param name="pluginId">The stable plugin identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>No content when the operation succeeds.</returns>
    [HttpPost("{pluginId}/enable")]
    public Task<IActionResult> Enable(string pluginId, CancellationToken cancellationToken) => ExecuteLifecycleOperation(() => mediator.Send(new EnablePluginCommand(pluginId), cancellationToken));

    /// <summary>Uninstalls a plugin when no installed plugin requires it.</summary>
    /// <param name="pluginId">The stable plugin identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>No content when the operation succeeds.</returns>
    [HttpDelete("{pluginId}")]
    public Task<IActionResult> Uninstall(string pluginId, CancellationToken cancellationToken) => ExecuteLifecycleOperation(() => mediator.Send(new UninstallPluginCommand(pluginId), cancellationToken));

    private static bool IsPackage(IFormFile package) => package is not null && package.Length > 0 && package.FileName.EndsWith(".nupkg", StringComparison.OrdinalIgnoreCase);

    private static ActionResult<T> InvalidPackage<T>() => new(new BadRequestObjectResult(new ProblemDetails { Title = "Invalid plugin package.", Detail = "The uploaded file must be a non-empty .nupkg file." }));

    private static async Task<T> WithTemporaryPackageAsync<T>(IFormFile package, Func<string, Task<T>> operation, CancellationToken cancellationToken)
    {
        var temporaryDirectory = Path.Combine(Path.GetTempPath(), "RemoteCommerce", "plugins");
        Directory.CreateDirectory(temporaryDirectory);
        var temporaryPath = Path.Combine(temporaryDirectory, $"{Guid.NewGuid():N}.nupkg");
        try
        {
            await using (var output = File.Create(temporaryPath)) await package.CopyToAsync(output, cancellationToken);
            return await operation(temporaryPath);
        }
        finally
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }
    }

    private static async Task<IActionResult> ExecuteLifecycleOperation(Func<Task<Unit>> operation)
    {
        try { await operation(); return new NoContentResult(); }
        catch (KeyNotFoundException exception) { return new NotFoundObjectResult(new ProblemDetails { Title = "Plugin was not found.", Detail = exception.Message }); }
        catch (InvalidOperationException exception) { return new BadRequestObjectResult(new ProblemDetails { Title = "Plugin lifecycle operation failed.", Detail = exception.Message }); }
    }

    /// <summary>Represents the result of a successful plugin installation.</summary>
    /// <param name="PluginId">The stable plugin identifier.</param>
    /// <param name="Version">The installed plugin version.</param>
    /// <param name="RestartRequired">Indicates that the application must restart before activation.</param>
    public sealed record InstallPluginResponse(string PluginId, string Version, bool RestartRequired);

    /// <summary>Represents the result of a successful plugin update.</summary>
    /// <param name="PluginId">The stable plugin identifier.</param>
    /// <param name="Version">The newly selected plugin version.</param>
    /// <param name="RestartRequired">Indicates that the application must restart before activation.</param>
    public sealed record UpdatePluginResponse(string PluginId, string Version, bool RestartRequired);
}

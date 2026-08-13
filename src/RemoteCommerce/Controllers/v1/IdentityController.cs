namespace RemoteCommerce.Controllers.v1;

/// <summary>Provides the RemoteCommerce browser authentication boundary without exposing ASP.NET Core Identity endpoints.</summary>
[ApiController]
[Tags("Identity")]
[Route("api/rc/v1/identity")]
public sealed class IdentityController(IMediator mediator) : ControllerBase
{
    /// <summary>Authenticates a user and stores the short-lived JWT in an HTTP-only browser cookie.</summary>
    /// <param name="request">The login credentials.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The JWT expiration metadata.</returns>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthenticationResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await mediator.Send(new LoginCommand(request.Email, request.Password), cancellationToken);
            Response.Cookies.Append(JwtOptions.CookieName, result.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = result.ExpiresAt,
                IsEssential = true,
                Path = "/"
            });
            return Ok(new AuthenticationResponse(result.ExpiresAt));
        }
        catch (UnauthorizedAccessException exception)
        {
            return Unauthorized(new ProblemDetails { Title = "Authentication failed.", Detail = exception.Message });
        }
    }

    /// <summary>Returns whether the initial administrator bootstrap is available.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The current bootstrap status.</returns>
    [AllowAnonymous]
    [HttpGet("setup-status")]
    public async Task<ActionResult<bool>> SetupStatus(CancellationToken cancellationToken) => Ok(await mediator.Send(new GetSetupStatusQuery(), cancellationToken));

    /// <summary>Creates the first administrator and signs the browser in with a JWT.</summary>
    /// <param name="request">The bootstrap request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created administrator identifier.</returns>
    [AllowAnonymous]
    [HttpPost("setup")]
    public async Task<ActionResult<Guid>> Setup(BootstrapRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new BootstrapAdministratorCommand(request.DisplayName, request.Email, request.Password), cancellationToken);
        var login = await mediator.Send(new LoginCommand(request.Email, request.Password), cancellationToken);
        Response.Cookies.Append(JwtOptions.CookieName, login.Token, new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict, Expires = login.ExpiresAt, IsEssential = true, Path = "/" });
        return Ok(result.UserId);
    }

    /// <summary>Invalidates the current JWT by rotating the Identity security stamp and deleting the browser cookie.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content when the session has been invalidated.</returns>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await mediator.Send(new LogoutCommand(), cancellationToken);
        Response.Cookies.Delete(JwtOptions.CookieName, new CookieOptions { Secure = true, SameSite = SameSiteMode.Strict, Path = "/" });
        return NoContent();
    }

    /// <summary>Contains credentials submitted to the login endpoint.</summary>
    /// <param name="Email">The user's email address.</param>
    /// <param name="Password">The user's password.</param>
    public sealed record LoginRequest(string Email, string Password);

    /// <summary>Contains the first-administrator bootstrap credentials.</summary>
    /// <param name="DisplayName">The administrator display name.</param>
    /// <param name="Email">The administrator email address.</param>
    /// <param name="Password">The administrator password.</param>
    public sealed record BootstrapRequest(string DisplayName, string Email, string Password);

    /// <summary>Contains non-sensitive browser session metadata.</summary>
    /// <param name="ExpiresAt">The UTC token expiration.</param>
    public sealed record AuthenticationResponse(DateTimeOffset ExpiresAt);
}

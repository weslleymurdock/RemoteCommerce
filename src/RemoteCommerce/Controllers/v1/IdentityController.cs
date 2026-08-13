namespace RemoteCommerce.Controllers.v1;

/// <summary>Provides the RemoteCommerce-owned Identity HTTP boundary.</summary>
[ApiController]
[Tags("Identity")]
[Route("api/rc/v1/identity")]
public sealed class IdentityController(IMediator mediator) : ControllerBase
{
    /// <summary>Authenticates a user.</summary><param name="request">Credentials.</param><param name="cancellationToken">Cancellation token.</param><returns>Session metadata.</returns>
    [AllowAnonymous, HttpPost("login")]
    public async Task<ActionResult<AuthenticationResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        try { return Ok(await SetSession(await mediator.Send(new LoginCommand(request.Email, request.Password), cancellationToken))); }
        catch (TwoFactorRequiredException) { return Conflict(new { requiresTwoFactor = true }); }
        catch (UnauthorizedAccessException ex) { return Unauthorized(new ProblemDetails { Title = "Authentication failed.", Detail = ex.Message }); }
    }

    /// <summary>Completes an authenticator challenge.</summary><param name="request">The challenge.</param><param name="cancellationToken">Cancellation token.</param><returns>Session metadata.</returns>
    [AllowAnonymous, HttpPost("login/2fa")]
    public async Task<ActionResult<AuthenticationResponse>> LoginTwoFactor(CompleteTwoFactorRequest request, CancellationToken cancellationToken) => Ok(await SetSession(await mediator.Send(new CompleteTwoFactorCommand(request.Email, request.Code, request.RememberMachine), cancellationToken)));

    /// <summary>Completes a recovery-code challenge.</summary><param name="request">The recovery code.</param><param name="cancellationToken">Cancellation token.</param><returns>Session metadata.</returns>
    [AllowAnonymous, HttpPost("login/recovery")]
    public async Task<ActionResult<AuthenticationResponse>> LoginRecovery(RecoveryLoginRequest request, CancellationToken cancellationToken) => Ok(await SetSession(await mediator.Send(new CompleteRecoveryCodeCommand(request.Email, request.RecoveryCode), cancellationToken)));

    /// <summary>Refreshes the authenticated session.</summary><param name="cancellationToken">Cancellation token.</param><returns>Session metadata.</returns>
    [Authorize, HttpPost("refresh")]
    public async Task<ActionResult<AuthenticationResponse>> Refresh(CancellationToken cancellationToken) => Ok(await SetSession(await mediator.Send(new RefreshTokenCommand(), cancellationToken)));

    /// <summary>Returns whether first administrator setup is available.</summary><param name="cancellationToken">Cancellation token.</param><returns>Setup state.</returns>
    [AllowAnonymous, HttpGet("setup-status")]
    public async Task<ActionResult<bool>> SetupStatus(CancellationToken cancellationToken) => Ok(await mediator.Send(new GetSetupStatusQuery(), cancellationToken));

    /// <summary>Creates the first administrator.</summary><param name="request">Bootstrap data.</param><param name="cancellationToken">Cancellation token.</param><returns>Created user identifier.</returns>
    [AllowAnonymous, HttpPost("setup")]
    public async Task<ActionResult<Guid>> Setup(BootstrapRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new BootstrapAdministratorCommand(request.DisplayName, request.Email, request.Password), cancellationToken);
        var login = await mediator.Send(new LoginCommand(request.Email, request.Password), cancellationToken);
        await SetSession(login);
        return Ok(result.UserId);
    }

    /// <summary>Registers a standard user.</summary><param name="request">Registration data.</param><param name="cancellationToken">Cancellation token.</param><returns>Created user identifier.</returns>
    [Authorize(Policy = AuthorizationPolicies.ManageUsers), HttpPost("register")]
    public async Task<ActionResult<Guid>> Register(RegisterRequest request, CancellationToken cancellationToken) => Ok(await mediator.Send(new RegisterUserCommand(request.Email, request.DisplayName, request.Password), cancellationToken));

    /// <summary>Invalidates the current session.</summary><param name="cancellationToken">Cancellation token.</param><returns>No content.</returns>
    [Authorize, HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await mediator.Send(new LogoutCommand(), cancellationToken);
        Response.Cookies.Delete(JwtOptions.CookieName, new CookieOptions { Secure = Request.IsHttps, SameSite = SameSiteMode.Strict, Path = "/" });
        return NoContent();
    }

    /// <summary>Requests password recovery.</summary><param name="request">The email address.</param><param name="cancellationToken">Cancellation token.</param><returns>No content.</returns>
    [AllowAnonymous, HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request, CancellationToken cancellationToken) { await mediator.Send(new ForgotPasswordCommand(request.Email), cancellationToken); return NoContent(); }

    /// <summary>Resets a password.</summary><param name="request">Reset data.</param><param name="cancellationToken">Cancellation token.</param><returns>No content.</returns>
    [AllowAnonymous, HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request, CancellationToken cancellationToken) { await mediator.Send(new ResetPasswordCommand(request.Email, request.ResetToken, request.NewPassword), cancellationToken); return NoContent(); }

    /// <summary>Confirms an email address.</summary><param name="userId">User identifier.</param><param name="token">Confirmation token.</param><param name="cancellationToken">Cancellation token.</param><returns>No content.</returns>
    [AllowAnonymous, HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(Guid userId, string token, CancellationToken cancellationToken) { await mediator.Send(new ConfirmEmailCommand(userId, token), cancellationToken); return NoContent(); }

    /// <summary>Resends email confirmation.</summary><param name="request">Email address.</param><param name="cancellationToken">Cancellation token.</param><returns>No content.</returns>
    [AllowAnonymous, HttpPost("resend-confirmation")]
    public async Task<IActionResult> ResendConfirmation(ResendConfirmationRequest request, CancellationToken cancellationToken) { await mediator.Send(new ResendConfirmationEmailCommand(request.Email), cancellationToken); return NoContent(); }

    /// <summary>Gets the authenticated profile.</summary><param name="cancellationToken">Cancellation token.</param><returns>Profile information.</returns>
    [Authorize, HttpGet("manage/info")]
    public async Task<ActionResult<UserProfileResult>> Profile(CancellationToken cancellationToken) => Ok(await mediator.Send(new GetCurrentProfileQuery(), cancellationToken));

    /// <summary>Updates the authenticated profile.</summary><param name="request">Profile data.</param><param name="cancellationToken">Cancellation token.</param><returns>No content.</returns>
    [Authorize, HttpPost("manage/info")]
    public async Task<IActionResult> UpdateProfile(ProfileRequest request, CancellationToken cancellationToken) { await mediator.Send(new UpdateProfileCommand(request.DisplayName, request.Email), cancellationToken); return NoContent(); }

    /// <summary>Gets two-factor configuration.</summary><param name="cancellationToken">Cancellation token.</param><returns>Two-factor information.</returns>
    [Authorize, HttpGet("manage/2fa")]
    public async Task<ActionResult<TwoFactorInfo>> GetTwoFactor(CancellationToken cancellationToken) => Ok(await mediator.Send(new GetTwoFactorQuery(), cancellationToken));

    /// <summary>Enables or disables two-factor authentication.</summary><param name="request">Two-factor state.</param><param name="cancellationToken">Cancellation token.</param><returns>Updated configuration.</returns>
    [Authorize, HttpPost("manage/2fa")]
    public async Task<ActionResult<TwoFactorInfo>> SetTwoFactor(TwoFactorRequest request, CancellationToken cancellationToken) => Ok(await mediator.Send(new SetTwoFactorCommand(request.Enable), cancellationToken));

    /// <summary>Disables two-factor authentication.</summary><param name="cancellationToken">Cancellation token.</param><returns>No content.</returns>
    [Authorize, HttpPost("manage/2fa/disable")]
    public async Task<IActionResult> DisableTwoFactor(CancellationToken cancellationToken) { await mediator.Send(new DisableTwoFactorCommand(), cancellationToken); return NoContent(); }

    /// <summary>Generates new recovery codes.</summary><param name="cancellationToken">Cancellation token.</param><returns>Recovery codes.</returns>
    [Authorize, HttpPost("manage/2fa/recovery-codes")]
    public async Task<ActionResult<IReadOnlyList<string>>> RecoveryCodes(CancellationToken cancellationToken) => Ok(await mediator.Send(new GenerateRecoveryCodesCommand(), cancellationToken));

    /// <summary>Resets the authenticator key.</summary><param name="cancellationToken">Cancellation token.</param><returns>Updated authenticator information.</returns>
    [Authorize, HttpPost("manage/2fa/reset-authenticator")]
    public async Task<ActionResult<TwoFactorInfo>> ResetAuthenticator(CancellationToken cancellationToken) => Ok(await mediator.Send(new ResetAuthenticatorKeyCommand(), cancellationToken));

    private Task<AuthenticationResponse> SetSession(JwtAuthenticationResult result)
    {
        Response.Cookies.Append(JwtOptions.CookieName, result.Token, new CookieOptions { HttpOnly = true, Secure = Request.IsHttps, SameSite = SameSiteMode.Strict, Expires = result.ExpiresAt, IsEssential = true, Path = "/" });
        return Task.FromResult(new AuthenticationResponse(result.ExpiresAt));
    }

    /// <summary>Login credentials.</summary><param name="Email">Email.</param><param name="Password">Password.</param>
    public sealed record LoginRequest(string Email, string Password);
    /// <summary>First administrator data.</summary><param name="DisplayName">Display name.</param><param name="Email">Email.</param><param name="Password">Password.</param>
    public sealed record BootstrapRequest(string DisplayName, string Email, string Password);
    /// <summary>User registration data.</summary><param name="Email">Email.</param><param name="DisplayName">Display name.</param><param name="Password">Password.</param>
    public sealed record RegisterRequest(string Email, string DisplayName, string Password);
    /// <summary>Two-factor login data.</summary><param name="Email">Email.</param><param name="Code">Authenticator code.</param><param name="RememberMachine">Whether to remember the machine.</param>
    public sealed record CompleteTwoFactorRequest(string Email, string Code, bool RememberMachine);
    /// <summary>Recovery login data.</summary><param name="Email">Email.</param><param name="RecoveryCode">Recovery code.</param>
    public sealed record RecoveryLoginRequest(string Email, string RecoveryCode);
    /// <summary>Password recovery request.</summary><param name="Email">Email.</param>
    public sealed record ForgotPasswordRequest(string Email);
    /// <summary>Password reset request.</summary><param name="Email">Email.</param><param name="ResetToken">Reset token.</param><param name="NewPassword">New password.</param>
    public sealed record ResetPasswordRequest(string Email, string ResetToken, string NewPassword);
    /// <summary>Confirmation resend request.</summary><param name="Email">Email.</param>
    public sealed record ResendConfirmationRequest(string Email);
    /// <summary>Profile data.</summary><param name="DisplayName">Display name.</param><param name="Email">Email.</param>
    public sealed record ProfileRequest(string DisplayName, string Email);
    /// <summary>Two-factor state.</summary><param name="Enable">Whether to enable 2FA.</param>
    public sealed record TwoFactorRequest(bool Enable);
    /// <summary>Authentication session metadata.</summary><param name="ExpiresAt">JWT expiration.</param>
    public sealed record AuthenticationResponse(DateTimeOffset ExpiresAt);
}

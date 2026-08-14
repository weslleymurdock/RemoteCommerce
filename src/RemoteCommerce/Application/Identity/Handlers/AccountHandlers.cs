namespace RemoteCommerce.Application.Identity.Handlers;

/// <summary>Handles account, profile, recovery, refresh-token, and two-factor operations.</summary>
public sealed class AccountHandlers(
    UserManager<ApplicationUser> users,
    RoleManager<ApplicationRole> roles,
    IJwtTokenService tokens,
    IApplicationContext context,
    IEmailService<IdentityEmailMessage> email,
    IConfiguration configuration,
    IAuditLogService audit)
{
    /// <summary>Registers a standard user.</summary>
    public async Task<Guid> Register(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var emailAddress = request.Email.Trim();
        var user = new ApplicationUser
        {
            UserName = emailAddress,
            Email = emailAddress,
            DisplayName = request.DisplayName.Trim()
        };

        Ensure(await users.CreateAsync(user, request.Password), "User registration failed.");

        if (!await roles.RoleExistsAsync("User"))
        {
            Ensure(
                await roles.CreateAsync(
                    new ApplicationRole
                    {
                        Name = "User",
                        Description = "Standard RemoteCommerce user."
                    }),
                "User role creation failed.");
        }

        Ensure(await users.AddToRoleAsync(user, "User"), "User role assignment failed.");
        await SendConfirmation(user, cancellationToken);
        await audit.WriteAsync(
            "identity.register",
            "User",
            user.Id,
            user.DisplayName,
            "Success",
            cancellationToken: cancellationToken);

        return user.Id;
    }

    /// <summary>Requests a password reset without disclosing account existence.</summary>
    public async Task ForgotPassword(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await users.FindByEmailAsync(request.Email.Trim());

        if (user is not null && !user.IsDisabled)
        {
            var token = await users.GeneratePasswordResetTokenAsync(user);
            var resetUrl = Url($"/reset-password?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}");

            await email.SendAsync(user.Email!, "RemoteCommerce password reset", $"Reset: {resetUrl}", cancellationToken);
        }
    }

    /// <summary>Authenticates a user and returns an access and refresh token pair.</summary>
    public async Task<JwtAuthenticationResult> Login(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await users.FindByEmailAsync(request.Email.Trim());
        if (user is null || user.IsDisabled || !await users.CheckPasswordAsync(user, request.Password))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        return await CreateTokenAsync(user);
    }

    /// <summary>Refreshes an access token using a valid refresh token.</summary>
    public async Task<JwtAuthenticationResult> Refresh(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var validation = tokens.ValidateRefreshToken(request.RefreshToken);
        var user = await users.FindByIdAsync(validation.UserId.ToString())
            ?? throw new UnauthorizedAccessException("The refresh token is invalid.");

        if (user.IsDisabled || !string.Equals(user.SecurityStamp, validation.SecurityStamp, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("The refresh token has been invalidated.");
        }

        return await CreateTokenAsync(user);
    }

    /// <summary>Completes an authenticator-based two-factor authentication flow.</summary>
    public async Task<JwtAuthenticationResult> VerifyTwoFactor(CompleteTwoFactorCommand request, CancellationToken cancellationToken)
    {
        var user = await users.FindByEmailAsync(request.Email.Trim())
            ?? throw new UnauthorizedAccessException("Invalid two-factor credentials.");

        Ensure(
            await users.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultAuthenticatorProvider, request.Code),
            "Invalid two-factor credentials.");

        return await CreateTokenAsync(user);
    }

    /// <summary>Completes two-factor authentication using a recovery code.</summary>
    public async Task<JwtAuthenticationResult> RedeemRecoveryCode(CompleteRecoveryCodeCommand request, CancellationToken cancellationToken)
    {
        var user = await users.FindByEmailAsync(request.Email.Trim())
            ?? throw new UnauthorizedAccessException("Invalid recovery credentials.");

        Ensure(
            await users.RedeemTwoFactorRecoveryCodeAsync(user, request.RecoveryCode.Trim()),
            "Invalid recovery credentials.");

        return await CreateTokenAsync(user);
    }

    private async Task<JwtAuthenticationResult> CreateTokenAsync(ApplicationUser user)
    {
        var roles = await users.GetRolesAsync(user);
        var claims = await users.GetClaimsAsync(user);

        return tokens.CreateToken(
            user.Id,
            user.Email ?? string.Empty,
            user.DisplayName,
            user.SecurityStamp ?? string.Empty,
            roles,
            claims);
    }

    private async Task SendConfirmation(ApplicationUser user, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(user.Email))
        {
            return;
        }

        var token = await users.GenerateEmailConfirmationTokenAsync(user);
        var confirmationUrl = Url($"/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}");

        await email.SendAsync(user.Email, "Confirm your RemoteCommerce account", $"Confirm: {confirmationUrl}", cancellationToken);
    }

    private string Url(string relativePath)
    {
        var baseUrl = configuration["Application:BaseUrl"]?.TrimEnd('/')
            ?? throw new InvalidOperationException("Application:BaseUrl is not configured.");

        return $"{baseUrl}{relativePath}";
    }

    private static void Ensure(IdentityResult result, string message)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"{message} {string.Join("; ", result.Errors.Select(error => error.Description))}");
        }
    }

    private static void Ensure(bool result, string message)
    {
        if (!result)
        {
            throw new UnauthorizedAccessException(message);
        }
    }
}

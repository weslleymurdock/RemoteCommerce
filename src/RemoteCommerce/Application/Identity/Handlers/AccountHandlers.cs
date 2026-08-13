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
    public async Task ForgotPassword(
        ForgotPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var user = await users.FindByEmailAsync(request.Email.Trim());

        if (user is not null && !user.IsDisabled)
        {
            var token = await users.GeneratePasswordResetTokenAsync(user);
            var resetUrl = Url(
                $"/reset-password?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}");

            await email.SendAsync(
                user.Email!,
                "RemoteCommerce password reset",
                $"Reset: {resetUrl}",
                cancellationToken);
        }
    }

    /// <summary>Resets a password.</summary>
    public async Task ResetPassword(
        ResetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var user = await users.FindByEmailAsync(request.Email.Trim())
            ?? throw new InvalidOperationException("The password reset request is invalid.");

        Ensure(
            await users.ResetPasswordAsync(user, request.ResetToken, request.NewPassword),
            "Password reset failed.");
        await users.UpdateSecurityStampAsync(user);
    }

    /// <summary>Confirms an email address.</summary>
    public async Task ConfirmEmail(
        ConfirmEmailCommand request,
        CancellationToken cancellationToken)
    {
        var user = await users.FindByIdAsync(request.UserId.ToString())
            ?? throw new InvalidOperationException("The confirmation request is invalid.");

        Ensure(
            await users.ConfirmEmailAsync(user, request.Token),
            "Email confirmation failed.");
    }

    /// <summary>Resends an email confirmation.</summary>
    public async Task ResendConfirmation(
        ResendConfirmationEmailCommand request,
        CancellationToken cancellationToken)
    {
        var user = await users.FindByEmailAsync(request.Email.Trim());

        if (user is not null && !user.EmailConfirmed && !user.IsDisabled)
        {
            await SendConfirmation(user, cancellationToken);
        }
    }

    /// <summary>Updates the authenticated profile.</summary>
    public async Task UpdateProfile(
        UpdateProfileCommand request,
        CancellationToken cancellationToken)
    {
        var user = await Current();
        var emailAddress = request.Email.Trim();
        var emailChanged = !string.Equals(
            user.Email,
            emailAddress,
            StringComparison.OrdinalIgnoreCase);

        user.DisplayName = request.DisplayName.Trim();
        Ensure(await users.UpdateAsync(user), "Profile update failed.");

        if (!emailChanged)
        {
            return;
        }

        Ensure(
            await users.SetEmailAsync(user, emailAddress),
            "Email update failed.");
        Ensure(
            await users.SetUserNameAsync(user, emailAddress),
            "Username update failed.");

        user.EmailConfirmed = false;
        Ensure(
            await users.UpdateAsync(user),
            "Email confirmation state update failed.");
        await SendConfirmation(user, cancellationToken);
    }

    /// <summary>Refreshes a JWT session using a valid refresh token.</summary>
    public async Task<JwtAuthenticationResult> Refresh(RefreshTokenCommand request)
    {
        var validation = tokens.ValidateRefreshToken(request.RefreshToken);
        var user = await users.FindByIdAsync(validation.UserId.ToString())
            ?? throw new UnauthorizedAccessException("The refresh token is invalid.");

        if (user.IsDisabled)
        {
            throw new UnauthorizedAccessException("The refresh token is invalid.");
        }

        if (!string.Equals(
                user.SecurityStamp,
                validation.SecurityStamp,
                StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("The refresh token has been invalidated.");
        }

        return tokens.CreateToken(
            user,
            await users.GetRolesAsync(user),
            await users.GetClaimsAsync(user));
    }

    /// <summary>Completes an authenticator challenge.</summary>
    public async Task<JwtAuthenticationResult> TwoFactor(CompleteTwoFactorCommand request)
    {
        var user = await users.FindByEmailAsync(request.Email.Trim())
            ?? throw new UnauthorizedAccessException("Invalid two-factor credentials.");

        if (!user.TwoFactorEnabled
            || !await users.VerifyTwoFactorTokenAsync(
                user,
                TokenOptions.DefaultAuthenticatorProvider,
                request.Code))
        {
            throw new UnauthorizedAccessException("Invalid two-factor credentials.");
        }

        return tokens.CreateToken(
            user,
            await users.GetRolesAsync(user),
            await users.GetClaimsAsync(user));
    }

    /// <summary>Completes a recovery-code challenge.</summary>
    public async Task<JwtAuthenticationResult> Recovery(CompleteRecoveryCodeCommand request)
    {
        var user = await users.FindByEmailAsync(request.Email.Trim())
            ?? throw new UnauthorizedAccessException("Invalid recovery credentials.");

        Ensure(
            await users.RedeemTwoFactorRecoveryCodeAsync(
                user,
                request.RecoveryCode.Trim()),
            "Invalid recovery credentials.");

        return tokens.CreateToken(
            user,
            await users.GetRolesAsync(user),
            await users.GetClaimsAsync(user));
    }

    /// <summary>Sets two-factor state.</summary>
    public async Task<TwoFactorInfo> SetTwoFactor(SetTwoFactorCommand request)
    {
        var user = await Current();

        if (request.Enable
            && string.IsNullOrWhiteSpace(await users.GetAuthenticatorKeyAsync(user)))
        {
            Ensure(
                await users.ResetAuthenticatorKeyAsync(user),
                "Authenticator setup failed.");
        }

        Ensure(
            await users.SetTwoFactorEnabledAsync(user, request.Enable),
            "Two-factor configuration failed.");

        return await TwoFactorInfo(user);
    }

    /// <summary>Disables two-factor authentication.</summary>
    public async Task DisableTwoFactor()
    {
        var user = await Current();
        Ensure(
            await users.SetTwoFactorEnabledAsync(user, false),
            "Two-factor disable failed.");
    }

    /// <summary>Generates recovery codes.</summary>
    public async Task<IReadOnlyList<string>> RecoveryCodes()
    {
        var user = await Current();

        if (!user.TwoFactorEnabled)
        {
            throw new InvalidOperationException(
                "Two-factor authentication must be enabled first.");
        }

        var codes = await users.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
        return [.. codes!];
    }

    /// <summary>Resets the authenticator key.</summary>
    public async Task<TwoFactorInfo> ResetAuthenticatorKey()
    {
        var user = await Current();
        Ensure(
            await users.ResetAuthenticatorKeyAsync(user),
            "Authenticator reset failed.");

        return await TwoFactorInfo(user);
    }

    /// <summary>Gets the authenticated profile.</summary>
    public async Task<UserProfileResult> Profile()
    {
        var user = await Current();

        return new UserProfileResult(
            user.Id,
            user.Email ?? string.Empty,
            user.DisplayName,
            user.EmailConfirmed,
            user.TwoFactorEnabled);
    }

    /// <summary>Gets two-factor information.</summary>
    public async Task<TwoFactorInfo> GetTwoFactor()
    {
        return await TwoFactorInfo(await Current());
    }

    private async Task<ApplicationUser> Current()
    {
        var userId = context.UserId
            ?? throw new UnauthorizedAccessException("Authentication is required.");

        return await users.FindByIdAsync(userId.ToString())
            ?? throw new UnauthorizedAccessException("Authentication is required.");
    }

    private async Task SendConfirmation(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        var token = await users.GenerateEmailConfirmationTokenAsync(user);
        var confirmationUrl = Url(
            $"/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}");

        await email.SendAsync(
            user.Email!,
            "Confirm your RemoteCommerce email",
            $"Confirm: {confirmationUrl}",
            cancellationToken);
    }

    private async Task<TwoFactorInfo> TwoFactorInfo(ApplicationUser user)
    {
        var key = await users.GetAuthenticatorKeyAsync(user);
        var uri = string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(user.Email)
            ? null
            : $"otpauth://totp/RemoteCommerce:{Uri.EscapeDataString(user.Email)}?secret={Uri.EscapeDataString(key)}&issuer=RemoteCommerce&digits=6&period=30";

        return new TwoFactorInfo(user.TwoFactorEnabled, key, uri);
    }

    private string Url(string path)
    {
        var publicUrl = configuration["Application:PublicUrl"]
            ?.TrimEnd('/');

        return $"{publicUrl ?? "https://localhost:5001"}{path}";
    }

    private static void Ensure(IdentityResult result, string message)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"{message} {string.Join(" ", result.Errors.Select(error => error.Description))}");
        }
    }
}

/// <summary>Handles registration.</summary>
public sealed class RegisterUserCommandHandler(AccountHandlers handler)
    : IRequestHandler<RegisterUserCommand, Guid>
{
    /// <summary>Handles the registration command.</summary>
    public Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        return handler.Register(request, cancellationToken);
    }
}

/// <summary>Handles password reset requests.</summary>
public sealed class ForgotPasswordCommandHandler(AccountHandlers handler)
    : IRequestHandler<ForgotPasswordCommand, Unit>
{
    /// <summary>Handles the password reset request command.</summary>
    public async Task<Unit> Handle(
        ForgotPasswordCommand request,
        CancellationToken cancellationToken)
    {
        await handler.ForgotPassword(request, cancellationToken);
        return Unit.Value;
    }
}

/// <summary>Handles password resets.</summary>
public sealed class ResetPasswordCommandHandler(AccountHandlers handler)
    : IRequestHandler<ResetPasswordCommand, Unit>
{
    /// <summary>Handles the password reset command.</summary>
    public async Task<Unit> Handle(
        ResetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        await handler.ResetPassword(request, cancellationToken);
        return Unit.Value;
    }
}

/// <summary>Handles email confirmation.</summary>
public sealed class ConfirmEmailCommandHandler(AccountHandlers handler)
    : IRequestHandler<ConfirmEmailCommand, Unit>
{
    /// <summary>Handles the email confirmation command.</summary>
    public async Task<Unit> Handle(
        ConfirmEmailCommand request,
        CancellationToken cancellationToken)
    {
        await handler.ConfirmEmail(request, cancellationToken);
        return Unit.Value;
    }
}

/// <summary>Handles confirmation resend.</summary>
public sealed class ResendConfirmationEmailCommandHandler(AccountHandlers handler)
    : IRequestHandler<ResendConfirmationEmailCommand, Unit>
{
    /// <summary>Handles the confirmation resend command.</summary>
    public async Task<Unit> Handle(
        ResendConfirmationEmailCommand request,
        CancellationToken cancellationToken)
    {
        await handler.ResendConfirmation(request, cancellationToken);
        return Unit.Value;
    }
}

/// <summary>Handles profile updates.</summary>
public sealed class UpdateProfileCommandHandler(AccountHandlers handler)
    : IRequestHandler<UpdateProfileCommand, Unit>
{
    /// <summary>Handles the profile update command.</summary>
    public async Task<Unit> Handle(
        UpdateProfileCommand request,
        CancellationToken cancellationToken)
    {
        await handler.UpdateProfile(request, cancellationToken);
        return Unit.Value;
    }
}

/// <summary>Handles JWT refresh.</summary>
public sealed class RefreshTokenCommandHandler(AccountHandlers handler)
    : IRequestHandler<RefreshTokenCommand, JwtAuthenticationResult>
{
    /// <summary>Handles the refresh-token command.</summary>
    public Task<JwtAuthenticationResult> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        return handler.Refresh(request);
    }
}

/// <summary>Handles authenticator login.</summary>
public sealed class CompleteTwoFactorCommandHandler(AccountHandlers handler)
    : IRequestHandler<CompleteTwoFactorCommand, JwtAuthenticationResult>
{
    /// <summary>Handles the authenticator challenge command.</summary>
    public Task<JwtAuthenticationResult> Handle(
        CompleteTwoFactorCommand request,
        CancellationToken cancellationToken)
    {
        return handler.TwoFactor(request);
    }
}

/// <summary>Handles recovery login.</summary>
public sealed class CompleteRecoveryCodeCommandHandler(AccountHandlers handler)
    : IRequestHandler<CompleteRecoveryCodeCommand, JwtAuthenticationResult>
{
    /// <summary>Handles the recovery-code challenge command.</summary>
    public Task<JwtAuthenticationResult> Handle(
        CompleteRecoveryCodeCommand request,
        CancellationToken cancellationToken)
    {
        return handler.Recovery(request);
    }
}

/// <summary>Handles two-factor enablement.</summary>
public sealed class SetTwoFactorCommandHandler(AccountHandlers handler)
    : IRequestHandler<SetTwoFactorCommand, TwoFactorInfo>
{
    /// <summary>Handles the two-factor enablement command.</summary>
    public Task<TwoFactorInfo> Handle(
        SetTwoFactorCommand request,
        CancellationToken cancellationToken)
    {
        return handler.SetTwoFactor(request);
    }
}

/// <summary>Handles two-factor disablement.</summary>
public sealed class DisableTwoFactorCommandHandler(AccountHandlers handler)
    : IRequestHandler<DisableTwoFactorCommand, Unit>
{
    /// <summary>Handles the two-factor disablement command.</summary>
    public async Task<Unit> Handle(
        DisableTwoFactorCommand request,
        CancellationToken cancellationToken)
    {
        await handler.DisableTwoFactor();
        return Unit.Value;
    }
}

/// <summary>Handles recovery-code generation.</summary>
public sealed class GenerateRecoveryCodesCommandHandler(AccountHandlers handler)
    : IRequestHandler<GenerateRecoveryCodesCommand, IReadOnlyList<string>>
{
    /// <summary>Handles the recovery-code generation command.</summary>
    public Task<IReadOnlyList<string>> Handle(
        GenerateRecoveryCodesCommand request,
        CancellationToken cancellationToken)
    {
        return handler.RecoveryCodes();
    }
}

/// <summary>Handles authenticator reset.</summary>
public sealed class ResetAuthenticatorKeyCommandHandler(AccountHandlers handler)
    : IRequestHandler<ResetAuthenticatorKeyCommand, TwoFactorInfo>
{
    /// <summary>Handles the authenticator reset command.</summary>
    public Task<TwoFactorInfo> Handle(
        ResetAuthenticatorKeyCommand request,
        CancellationToken cancellationToken)
    {
        return handler.ResetAuthenticatorKey();
    }
}

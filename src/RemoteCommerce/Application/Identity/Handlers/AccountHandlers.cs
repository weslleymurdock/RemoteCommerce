namespace RemoteCommerce.Application.Identity.Handlers;

/// <summary>Handles account, profile, recovery and two-factor operations.</summary>
public sealed class AccountHandlers(UserManager<ApplicationUser> users, RoleManager<ApplicationRole> roles, IJwtTokenService tokens, IApplicationContext context, IEmailService<IdentityEmailMessage> email, IConfiguration configuration, IAuditLogService audit)
{
    /// <summary>Registers a standard user.</summary>
    public async Task<Guid> Register(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser { UserName = request.Email.Trim(), Email = request.Email.Trim(), DisplayName = request.DisplayName.Trim() };
        Ensure(await users.CreateAsync(user, request.Password), "User registration failed.");
        if (!await roles.RoleExistsAsync("User")) Ensure(await roles.CreateAsync(new ApplicationRole { Name = "User", Description = "Standard RemoteCommerce user." }), "User role creation failed.");
        Ensure(await users.AddToRoleAsync(user, "User"), "User role assignment failed.");
        await SendConfirmation(user, cancellationToken);
        await audit.WriteAsync("identity.register", "User", user.Id, user.DisplayName, "Success", cancellationToken: cancellationToken);
        return user.Id;
    }

    /// <summary>Requests a password reset without disclosing account existence.</summary>
    public async Task ForgotPassword(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await users.FindByEmailAsync(request.Email.Trim());
        if (user is not null && !user.IsDisabled)
        {
            var token = await users.GeneratePasswordResetTokenAsync(user);
            await email.SendAsync(user.Email!, "RemoteCommerce password reset", $"Reset: {Url($"/reset-password?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}")}", cancellationToken);
        }
    }

    /// <summary>Resets a password.</summary>
    public async Task ResetPassword(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await users.FindByEmailAsync(request.Email.Trim()) ?? throw new InvalidOperationException("The password reset request is invalid.");
        Ensure(await users.ResetPasswordAsync(user, request.ResetToken, request.NewPassword), "Password reset failed.");
        await users.UpdateSecurityStampAsync(user);
    }

    /// <summary>Confirms an email address.</summary>
    public async Task ConfirmEmail(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await users.FindByIdAsync(request.UserId.ToString()) ?? throw new InvalidOperationException("The confirmation request is invalid.");
        Ensure(await users.ConfirmEmailAsync(user, request.Token), "Email confirmation failed.");
    }

    /// <summary>Resends an email confirmation.</summary>
    public async Task ResendConfirmation(ResendConfirmationEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await users.FindByEmailAsync(request.Email.Trim());
        if (user is not null && !user.EmailConfirmed && !user.IsDisabled) await SendConfirmation(user, cancellationToken);
    }

    /// <summary>Updates the authenticated profile.</summary>
    public async Task UpdateProfile(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await Current();
        var emailChanged = !string.Equals(user.Email, request.Email.Trim(), StringComparison.OrdinalIgnoreCase);
        user.DisplayName = request.DisplayName.Trim();
        Ensure(await users.UpdateAsync(user), "Profile update failed.");
        if (emailChanged)
        {
            Ensure(await users.SetEmailAsync(user, request.Email.Trim()), "Email update failed.");
            Ensure(await users.SetUserNameAsync(user, request.Email.Trim()), "Username update failed.");
            user.EmailConfirmed = false;
            Ensure(await users.UpdateAsync(user), "Email confirmation state update failed.");
            await SendConfirmation(user, cancellationToken);
        }
    }

    /// <summary>Refreshes the authenticated JWT.</summary>
    public async Task<JwtAuthenticationResult> Refresh() { var u = await Current(); return tokens.CreateToken(u, await users.GetRolesAsync(u), await users.GetClaimsAsync(u)); }

    /// <summary>Completes an authenticator challenge.</summary>
    public async Task<JwtAuthenticationResult> TwoFactor(CompleteTwoFactorCommand request)
    {
        var u = await users.FindByEmailAsync(request.Email.Trim()) ?? throw new UnauthorizedAccessException("Invalid two-factor credentials.");
        if (!u.TwoFactorEnabled || !await users.VerifyTwoFactorTokenAsync(u, TokenOptions.DefaultAuthenticatorProvider, request.Code)) throw new UnauthorizedAccessException("Invalid two-factor credentials.");
        return tokens.CreateToken(u, await users.GetRolesAsync(u), await users.GetClaimsAsync(u));
    }

    /// <summary>Completes a recovery-code challenge.</summary>
    public async Task<JwtAuthenticationResult> Recovery(CompleteRecoveryCodeCommand request)
    {
        var u = await users.FindByEmailAsync(request.Email.Trim()) ?? throw new UnauthorizedAccessException("Invalid recovery credentials.");
        Ensure(await users.RedeemTwoFactorRecoveryCodeAsync(u, request.RecoveryCode.Trim()), "Invalid recovery credentials.");
        return tokens.CreateToken(u, await users.GetRolesAsync(u), await users.GetClaimsAsync(u));
    }

    /// <summary>Sets two-factor state.</summary>
    public async Task<TwoFactorInfo> SetTwoFactor(SetTwoFactorCommand request)
    {
        var u = await Current();
        if (request.Enable && string.IsNullOrWhiteSpace(await users.GetAuthenticatorKeyAsync(u))) Ensure(await users.ResetAuthenticatorKeyAsync(u), "Authenticator setup failed.");
        Ensure(await users.SetTwoFactorEnabledAsync(u, request.Enable), "Two-factor configuration failed.");
        return await TwoFactorInfo(u);
    }

    /// <summary>Disables two-factor authentication.</summary>
    public async Task DisableTwoFactor() { Ensure(await users.SetTwoFactorEnabledAsync(await Current(), false), "Two-factor disable failed."); }

    /// <summary>Generates recovery codes.</summary>
    public async Task<IReadOnlyList<string>> RecoveryCodes()
    {
        var u = await Current();
        if (!u.TwoFactorEnabled) throw new InvalidOperationException("Two-factor authentication must be enabled first.");
        return (await users.GenerateNewTwoFactorRecoveryCodesAsync(u, 10)).ToArray();
    }

    /// <summary>Resets the authenticator key.</summary>
    public async Task<TwoFactorInfo> ResetAuthenticatorKey() { var u = await Current(); Ensure(await users.ResetAuthenticatorKeyAsync(u), "Authenticator reset failed."); return await TwoFactorInfo(u); }

    /// <summary>Gets the authenticated profile.</summary>
    public async Task<UserProfileResult> Profile() { var u = await Current(); return new(u.Id, u.Email ?? string.Empty, u.DisplayName, u.EmailConfirmed, u.TwoFactorEnabled); }

    /// <summary>Gets two-factor information.</summary>
    public async Task<TwoFactorInfo> GetTwoFactor() => await TwoFactorInfo(await Current());

    private async Task<ApplicationUser> Current() => await users.FindByIdAsync((context.UserId ?? throw new UnauthorizedAccessException("Authentication is required.")).ToString()) ?? throw new UnauthorizedAccessException("Authentication is required.");
    private async Task SendConfirmation(ApplicationUser u, CancellationToken c) { var t = await users.GenerateEmailConfirmationTokenAsync(u); await email.SendAsync(u.Email!, "Confirm your RemoteCommerce email", $"Confirm: {Url($"/confirm-email?userId={u.Id}&token={Uri.EscapeDataString(t)}")}", c); }
    private async Task<TwoFactorInfo> TwoFactorInfo(ApplicationUser u) { var key = await users.GetAuthenticatorKeyAsync(u); var uri = string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(u.Email) ? null : $"otpauth://totp/RemoteCommerce:{Uri.EscapeDataString(u.Email)}?secret={Uri.EscapeDataString(key)}&issuer=RemoteCommerce&digits=6&period=30"; return new(u.TwoFactorEnabled, key, uri); }
    private string Url(string path) => $"{configuration["Application:PublicUrl"]?.TrimEnd('/') ?? "https://localhost:5001"}{path}";
    private static void Ensure(IdentityResult result, string message) { if (!result.Succeeded) throw new InvalidOperationException($"{message} {string.Join(" ", result.Errors.Select(e => e.Description))}"); }
}

/// <summary>Handles registration.</summary>
public sealed class RegisterUserCommandHandler(AccountHandlers h) : IRequestHandler<RegisterUserCommand, Guid> { public Task<Guid> Handle(RegisterUserCommand r, CancellationToken c) => h.Register(r, c); }
/// <summary>Handles password reset requests.</summary>
public sealed class ForgotPasswordCommandHandler(AccountHandlers h) : IRequestHandler<ForgotPasswordCommand, Unit> { public async Task<Unit> Handle(ForgotPasswordCommand r, CancellationToken c) { await h.ForgotPassword(r, c); return Unit.Value; } }
/// <summary>Handles password resets.</summary>
public sealed class ResetPasswordCommandHandler(AccountHandlers h) : IRequestHandler<ResetPasswordCommand, Unit> { public async Task<Unit> Handle(ResetPasswordCommand r, CancellationToken c) { await h.ResetPassword(r, c); return Unit.Value; } }
/// <summary>Handles email confirmation.</summary>
public sealed class ConfirmEmailCommandHandler(AccountHandlers h) : IRequestHandler<ConfirmEmailCommand, Unit> { public async Task<Unit> Handle(ConfirmEmailCommand r, CancellationToken c) { await h.ConfirmEmail(r, c); return Unit.Value; } }
/// <summary>Handles confirmation resend.</summary>
public sealed class ResendConfirmationEmailCommandHandler(AccountHandlers h) : IRequestHandler<ResendConfirmationEmailCommand, Unit> { public async Task<Unit> Handle(ResendConfirmationEmailCommand r, CancellationToken c) { await h.ResendConfirmation(r, c); return Unit.Value; } }
/// <summary>Handles profile updates.</summary>
public sealed class UpdateProfileCommandHandler(AccountHandlers h) : IRequestHandler<UpdateProfileCommand, Unit> { public async Task<Unit> Handle(UpdateProfileCommand r, CancellationToken c) { await h.UpdateProfile(r, c); return Unit.Value; } }
/// <summary>Handles JWT refresh.</summary>
public sealed class RefreshTokenCommandHandler(AccountHandlers h) : IRequestHandler<RefreshTokenCommand, JwtAuthenticationResult> { public Task<JwtAuthenticationResult> Handle(RefreshTokenCommand r, CancellationToken c) => h.Refresh(); }
/// <summary>Handles authenticator login.</summary>
public sealed class CompleteTwoFactorCommandHandler(AccountHandlers h) : IRequestHandler<CompleteTwoFactorCommand, JwtAuthenticationResult> { public Task<JwtAuthenticationResult> Handle(CompleteTwoFactorCommand r, CancellationToken c) => h.TwoFactor(r); }
/// <summary>Handles recovery login.</summary>
public sealed class CompleteRecoveryCodeCommandHandler(AccountHandlers h) : IRequestHandler<CompleteRecoveryCodeCommand, JwtAuthenticationResult> { public Task<JwtAuthenticationResult> Handle(CompleteRecoveryCodeCommand r, CancellationToken c) => h.Recovery(r); }
/// <summary>Handles two-factor enablement.</summary>
public sealed class SetTwoFactorCommandHandler(AccountHandlers h) : IRequestHandler<SetTwoFactorCommand, TwoFactorInfo> { public Task<TwoFactorInfo> Handle(SetTwoFactorCommand r, CancellationToken c) => h.SetTwoFactor(r); }
/// <summary>Handles two-factor disablement.</summary>
public sealed class DisableTwoFactorCommandHandler(AccountHandlers h) : IRequestHandler<DisableTwoFactorCommand, Unit> { public async Task<Unit> Handle(DisableTwoFactorCommand r, CancellationToken c) { await h.DisableTwoFactor(); return Unit.Value; } }
/// <summary>Handles recovery-code generation.</summary>
public sealed class GenerateRecoveryCodesCommandHandler(AccountHandlers h) : IRequestHandler<GenerateRecoveryCodesCommand, IReadOnlyList<string>> { public Task<IReadOnlyList<string>> Handle(GenerateRecoveryCodesCommand r, CancellationToken c) => h.RecoveryCodes(); }
/// <summary>Handles authenticator reset.</summary>
public sealed class ResetAuthenticatorKeyCommandHandler(AccountHandlers h) : IRequestHandler<ResetAuthenticatorKeyCommand, TwoFactorInfo> { public Task<TwoFactorInfo> Handle(ResetAuthenticatorKeyCommand r, CancellationToken c) => h.ResetAuthenticatorKey(); }
/// <summary>Handles profile queries.</summary>
public sealed class GetCurrentProfileQueryHandler(AccountHandlers h) : IRequestHandler<GetCurrentProfileQuery, UserProfileResult> { public Task<UserProfileResult> Handle(GetCurrentProfileQuery r, CancellationToken c) => h.Profile(); }
/// <summary>Handles two-factor queries.</summary>
public sealed class GetTwoFactorQueryHandler(AccountHandlers h) : IRequestHandler<GetTwoFactorQuery, TwoFactorInfo> { public Task<TwoFactorInfo> Handle(GetTwoFactorQuery r, CancellationToken c) => h.GetTwoFactor(); }

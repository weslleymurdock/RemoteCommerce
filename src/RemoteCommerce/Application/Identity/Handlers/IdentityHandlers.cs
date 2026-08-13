namespace RemoteCommerce.Application.Identity.Handlers;

/// <summary>Handles JWT authentication through ASP.NET Core Identity's UserManager and RoleManager stores.</summary>
public sealed class LoginCommandHandler(UserManager<ApplicationUser> userManager, IJwtTokenService tokenService, IAuditLogService auditLog) : IRequestHandler<LoginCommand, JwtAuthenticationResult>
{
    /// <inheritdoc />
    public async Task<JwtAuthenticationResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null || user.IsDisabled || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            if (user is not null) await userManager.AccessFailedAsync(user);
            await auditLog.WriteAsync("identity.login", "User", user?.Id, user?.DisplayName, "Failed", "Reason=InvalidCredentials", cancellationToken);
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        await userManager.ResetAccessFailedCountAsync(user);
        var roles = await userManager.GetRolesAsync(user);
        var claims = await userManager.GetClaimsAsync(user);
        var result = tokenService.CreateToken(user, roles, claims);
        await auditLog.WriteAsync("identity.login", "User", user.Id, user.DisplayName, "Success", cancellationToken: cancellationToken);
        return result;
    }
}

/// <summary>Handles first-administrator bootstrap using ASP.NET Core Identity stores.</summary>
public sealed class BootstrapAdministratorCommandHandler(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, IAuditLogService auditLog) : IRequestHandler<BootstrapAdministratorCommand, BootstrapAdministratorResult>
{
    /// <inheritdoc />
    public async Task<BootstrapAdministratorResult> Handle(BootstrapAdministratorCommand request, CancellationToken cancellationToken)
    {
        if (await userManager.Users.AnyAsync(cancellationToken)) throw new InvalidOperationException("Initial administrator setup has already been completed.");
        const string roleName = "Administrator";
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            var roleResult = await roleManager.CreateAsync(new ApplicationRole { Name = roleName, Description = "Full access to RemoteCommerce administration." });
            EnsureSucceeded(roleResult, "Administrator role creation failed.");
        }
        var user = new ApplicationUser { UserName = request.Email.Trim(), Email = request.Email.Trim(), DisplayName = request.DisplayName.Trim(), EmailConfirmed = true };
        EnsureSucceeded(await userManager.CreateAsync(user, request.Password), "Administrator creation failed.");
        EnsureSucceeded(await userManager.AddToRoleAsync(user, roleName), "Administrator role assignment failed.");
        foreach (var permission in new[] { AuthorizationPolicies.ManageConfiguration, AuthorizationPolicies.ManageUsers, AuthorizationPolicies.ManageLocalization, AuthorizationPolicies.ManagePlugins })
            EnsureSucceeded(await userManager.AddClaimAsync(user, new Claim("permission", permission)), "Administrator permission assignment failed.");
        await auditLog.WriteAsync("identity.bootstrap", "User", user.Id, user.DisplayName, "Success", cancellationToken: cancellationToken);
        return new BootstrapAdministratorResult(user.Id);
    }

    private static void EnsureSucceeded(IdentityResult result, string message)
    {
        if (!result.Succeeded) throw new InvalidOperationException($"{message} {string.Join(" ", result.Errors.Select(x => x.Description))}");
    }
}

/// <summary>Invalidates the current JWT session by rotating the Identity security stamp.</summary>
public sealed class LogoutCommandHandler(UserManager<ApplicationUser> userManager, IApplicationContext applicationContext, IAuditLogService auditLog) : IRequestHandler<LogoutCommand, Unit>
{
    /// <inheritdoc />
    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (applicationContext.UserId is Guid userId)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user is not null) await userManager.UpdateSecurityStampAsync(user);
            await auditLog.WriteAsync("identity.logout", "User", userId, applicationContext.Actor, "Success", cancellationToken: cancellationToken);
        }
        return Unit.Value;
    }
}

/// <summary>Handles the setup availability query.</summary>
public sealed class GetSetupStatusQueryHandler(UserManager<ApplicationUser> userManager) : IRequestHandler<GetSetupStatusQuery, bool>
{
    /// <inheritdoc />
    public async Task<bool> Handle(GetSetupStatusQuery request, CancellationToken cancellationToken) => !await userManager.Users.AnyAsync(cancellationToken);
}

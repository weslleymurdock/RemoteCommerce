namespace RemoteCommerce.Application.Identity;

/// <summary>Gets whether first-administrator bootstrap is still available.</summary>
public sealed record GetSetupStatusQuery : IQuery<bool>;

/// <summary>Authenticates a RemoteCommerce user with the configured Identity cookie scheme.</summary>
/// <param name="Email">The user's email address.</param>
/// <param name="Password">The user's password.</param>
/// <param name="Persistent">Whether the authentication cookie should persist.</param>
public sealed record LoginCommand(string Email, string Password, bool Persistent = false) : ICommand<LoginResult>, ITransactionalCommand;

/// <summary>Creates the first RemoteCommerce administrator when the user store is empty.</summary>
/// <param name="DisplayName">The administrator display name.</param>
/// <param name="Email">The administrator email address.</param>
/// <param name="Password">The administrator password.</param>
public sealed record BootstrapAdministratorCommand(string DisplayName, string Email, string Password) : ICommand<BootstrapAdministratorResult>, ITransactionalCommand;

/// <summary>Signs out the current authenticated user and records the operation.</summary>
public sealed record LogoutCommand : ICommand<Unit>, ITransactionalCommand;

/// <summary>Creates an administrative user through ASP.NET Core Identity.</summary>
/// <param name="DisplayName">The display name of the user.</param>
/// <param name="Email">The user's email address.</param>
/// <param name="Password">The user's initial password.</param>
/// <param name="Role">The optional role to assign.</param>
public sealed record CreateUserCommand(string DisplayName, string Email, string Password, string? Role) : ICommand<Guid>, ITransactionalCommand;

/// <summary>Creates an administrative authorization role.</summary>
/// <param name="Name">The role name.</param>
/// <param name="Description">The role description.</param>
public sealed record CreateRoleCommand(string Name, string Description) : ICommand<Guid>, ITransactionalCommand;

/// <summary>Contains the result of a login attempt.</summary>
/// <param name="Succeeded">Whether authentication succeeded.</param>
/// <param name="LockedOut">Whether the account was locked after the attempt.</param>
public sealed record LoginResult(bool Succeeded, bool LockedOut);

/// <summary>Contains the result of the first administrator bootstrap.</summary>
/// <param name="UserId">The newly created administrator identifier.</param>
public sealed record BootstrapAdministratorResult(Guid UserId);

/// <summary>Validates login input before it reaches the Identity handler.</summary>
public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    /// <summary>Initializes login validation rules.</summary>
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(256);
    }
}

/// <summary>Validates first-administrator bootstrap input.</summary>
public sealed class BootstrapAdministratorCommandValidator : AbstractValidator<BootstrapAdministratorCommand>
{
    /// <summary>Initializes bootstrap validation rules.</summary>
    public BootstrapAdministratorCommandValidator()
    {
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(12).MaximumLength(256)
            .Matches("[A-Z]").WithMessage("Password must contain an uppercase character.")
            .Matches("[a-z]").WithMessage("Password must contain a lowercase character.")
            .Matches("[0-9]").WithMessage("Password must contain a digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain a non-alphanumeric character.");
    }
}

/// <summary>Validates user creation input.</summary>
public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    /// <summary>Initializes user creation validation rules.</summary>
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(12).MaximumLength(256)
            .Matches("[A-Z]").WithMessage("Password must contain an uppercase character.")
            .Matches("[a-z]").WithMessage("Password must contain a lowercase character.")
            .Matches("[0-9]").WithMessage("Password must contain a digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain a non-alphanumeric character.");
        RuleFor(x => x.Role).MaximumLength(256).When(x => x.Role is not null);
    }
}

/// <summary>Validates role creation input.</summary>
public sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    /// <summary>Initializes role creation validation rules.</summary>
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}

/// <summary>Handles the setup status query.</summary>
/// <param name="userManager">The Identity user manager.</param>
public sealed class GetSetupStatusQueryHandler(UserManager<ApplicationUser> userManager) : IRequestHandler<GetSetupStatusQuery, bool>
{
    /// <summary>Returns whether the Identity user store is empty.</summary>
    /// <param name="request">The setup status query.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true"/> when the first administrator can still be created.</returns>
    public async Task<bool> Handle(GetSetupStatusQuery request, CancellationToken cancellationToken)
        => !await userManager.Users.AnyAsync(cancellationToken);
}

/// <summary>Handles user authentication through ASP.NET Core Identity.</summary>
/// <param name="userManager">The Identity user manager.</param>
/// <param name="signInManager">The Identity sign-in manager.</param>
/// <param name="auditLog">The transactional audit service.</param>
public sealed class LoginCommandHandler(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IAuditLogService auditLog) : IRequestHandler<LoginCommand, LoginResult>
{
    /// <summary>Authenticates the supplied credentials and records the result without logging the password.</summary>
    /// <param name="request">The login request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The authentication result.</returns>
    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            await auditLog.WriteAsync("identity.login", "User", null, email, "Failed", "Reason=InvalidCredentials", cancellationToken);
            return new LoginResult(false, false);
        }
        var result = await signInManager.PasswordSignInAsync(user, request.Password, request.Persistent, lockoutOnFailure: true);
        await auditLog.WriteAsync("identity.login", "User", user.Id, user.DisplayName, result.Succeeded ? "Success" : "Failed", result.IsLockedOut ? "Reason=LockedOut" : "Reason=InvalidCredentials", cancellationToken);
        return new LoginResult(result.Succeeded, result.IsLockedOut);
    }
}

/// <summary>Handles the first administrator bootstrap.</summary>
/// <param name="userManager">The Identity user manager.</param>
/// <param name="roleManager">The Identity role manager.</param>
/// <param name="signInManager">The Identity sign-in manager.</param>
/// <param name="auditLog">The transactional audit service.</param>
public sealed class BootstrapAdministratorCommandHandler(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, SignInManager<ApplicationUser> signInManager, IAuditLogService auditLog) : IRequestHandler<BootstrapAdministratorCommand, BootstrapAdministratorResult>
{
    /// <summary>Creates the baseline administrator role, user, permission claims, and authentication session.</summary>
    /// <param name="request">The bootstrap request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created administrator identifier.</returns>
    /// <exception cref="InvalidOperationException">Thrown when bootstrap has already been completed or an Identity mutation fails.</exception>
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
        {
            EnsureSucceeded(await userManager.AddClaimAsync(user, new Claim("permission", permission)), "Administrator permission assignment failed.");
        }
        await signInManager.SignInAsync(user, isPersistent: false);
        await auditLog.WriteAsync("identity.bootstrap", "User", user.Id, user.DisplayName, "Success", cancellationToken: cancellationToken);
        return new BootstrapAdministratorResult(user.Id);
    }

    private static void EnsureSucceeded(IdentityResult result, string message)
    {
        if (!result.Succeeded) throw new InvalidOperationException($"{message} {string.Join(" ", result.Errors.Select(error => error.Description))}");
    }
}

/// <summary>Handles user sign-out and its audit record.</summary>
/// <param name="signInManager">The Identity sign-in manager.</param>
/// <param name="auditLog">The transactional audit service.</param>
/// <param name="applicationContext">The current actor context.</param>
public sealed class LogoutCommandHandler(SignInManager<ApplicationUser> signInManager, IAuditLogService auditLog, IApplicationContext applicationContext) : IRequestHandler<LogoutCommand, Unit>
{
    /// <summary>Signs out the current user and records the operation.</summary>
    /// <param name="request">The logout request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A completed mediator unit value.</returns>
    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        await signInManager.SignOutAsync();
        await auditLog.WriteAsync("identity.logout", "User", applicationContext.UserId, applicationContext.Actor, "Success", cancellationToken: cancellationToken);
        return Unit.Value;
    }
}

/// <summary>Handles administrative user creation.</summary>
/// <param name="userManager">The Identity user manager.</param>
/// <param name="auditLog">The transactional audit service.</param>
/// <param name="applicationContext">The current actor context.</param>
public sealed class CreateUserCommandHandler(UserManager<ApplicationUser> userManager, IAuditLogService auditLog, IApplicationContext applicationContext) : IRequestHandler<CreateUserCommand, Guid>
{
    /// <summary>Creates a user and optionally assigns an existing role.</summary>
    /// <param name="request">The user creation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created user identifier.</returns>
    /// <exception cref="InvalidOperationException">Thrown when Identity rejects the mutation.</exception>
    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser { UserName = request.Email.Trim(), Email = request.Email.Trim(), DisplayName = request.DisplayName.Trim(), EmailConfirmed = true };
        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded) throw new InvalidOperationException(string.Join(" ", result.Errors.Select(error => error.Description)));
        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            var roleResult = await userManager.AddToRoleAsync(user, request.Role);
            if (!roleResult.Succeeded) throw new InvalidOperationException(string.Join(" ", roleResult.Errors.Select(error => error.Description)));
        }
        await auditLog.WriteAsync("identity.user.create", "User", applicationContext.UserId, applicationContext.Actor, "Success", $"TargetUserId={user.Id}", cancellationToken);
        return user.Id;
    }
}

/// <summary>Handles administrative role creation.</summary>
/// <param name="roleManager">The Identity role manager.</param>
/// <param name="auditLog">The transactional audit service.</param>
/// <param name="applicationContext">The current actor context.</param>
public sealed class CreateRoleCommandHandler(RoleManager<ApplicationRole> roleManager, IAuditLogService auditLog, IApplicationContext applicationContext) : IRequestHandler<CreateRoleCommand, Guid>
{
    /// <summary>Creates a role and records the administrative operation.</summary>
    /// <param name="request">The role creation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created role identifier.</returns>
    /// <exception cref="InvalidOperationException">Thrown when Identity rejects the mutation.</exception>
    public async Task<Guid> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = new ApplicationRole { Name = request.Name.Trim(), Description = request.Description.Trim() };
        var result = await roleManager.CreateAsync(role);
        if (!result.Succeeded) throw new InvalidOperationException(string.Join(" ", result.Errors.Select(error => error.Description)));
        await auditLog.WriteAsync("identity.role.create", "Role", applicationContext.UserId, applicationContext.Actor, "Success", $"TargetRole={role.Name}", cancellationToken);
        return role.Id;
    }
}

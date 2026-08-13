namespace RemoteCommerce.Application.Identity.Handlers;

/// <summary>Creates administrative users through ASP.NET Core Identity.</summary>
public sealed class CreateUserCommandHandler(UserManager<ApplicationUser> userManager, IApplicationContext applicationContext, IAuditLogService auditLog) : IRequestHandler<CreateUserCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser { UserName = request.Email.Trim(), Email = request.Email.Trim(), DisplayName = request.DisplayName.Trim(), EmailConfirmed = true };
        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded) throw new InvalidOperationException(string.Join(" ", result.Errors.Select(x => x.Description)));
        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            var roleResult = await userManager.AddToRoleAsync(user, request.Role);
            if (!roleResult.Succeeded) throw new InvalidOperationException(string.Join(" ", roleResult.Errors.Select(x => x.Description)));
        }
        await auditLog.WriteAsync("identity.user.create", "User", applicationContext.UserId, applicationContext.Actor, "Success", $"TargetUserId={user.Id}", cancellationToken);
        return user.Id;
    }
}

/// <summary>Creates administrative roles through ASP.NET Core Identity.</summary>
public sealed class CreateRoleCommandHandler(RoleManager<ApplicationRole> roleManager, IApplicationContext applicationContext, IAuditLogService auditLog) : IRequestHandler<CreateRoleCommand, Guid>
{
    /// <inheritdoc />
    public async Task<Guid> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = new ApplicationRole { Name = request.Name.Trim(), Description = request.Description.Trim() };
        var result = await roleManager.CreateAsync(role);
        if (!result.Succeeded) throw new InvalidOperationException(string.Join(" ", result.Errors.Select(x => x.Description)));
        await auditLog.WriteAsync("identity.role.create", "Role", applicationContext.UserId, applicationContext.Actor, "Success", $"TargetRole={role.Name}", cancellationToken);
        return role.Id;
    }
}

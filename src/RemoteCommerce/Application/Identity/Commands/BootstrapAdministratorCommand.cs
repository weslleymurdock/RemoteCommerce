namespace RemoteCommerce.Application.Identity.Commands;

/// <summary>Creates the first RemoteCommerce administrator.</summary>
/// <param name="DisplayName">The administrator display name.</param>
/// <param name="Email">The administrator email address.</param>
/// <param name="Password">The administrator password.</param>
public sealed record BootstrapAdministratorCommand(string DisplayName, string Email, string Password) : ICommand<BootstrapAdministratorResult>, ITransactionalCommand;

/// <summary>Contains the created administrator identifier.</summary>
/// <param name="UserId">The new administrator identifier.</param>
public sealed record BootstrapAdministratorResult(Guid UserId);

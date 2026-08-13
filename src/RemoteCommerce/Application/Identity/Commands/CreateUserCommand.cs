namespace RemoteCommerce.Application.Identity.Commands;

/// <summary>Creates an administrative user.</summary>
/// <param name="DisplayName">The display name.</param>
/// <param name="Email">The email address.</param>
/// <param name="Password">The initial password.</param>
/// <param name="Role">The optional existing role.</param>
public sealed record CreateUserCommand(string DisplayName, string Email, string Password, string? Role) : ICommand<Guid>, ITransactionalCommand;

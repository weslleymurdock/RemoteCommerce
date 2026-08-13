namespace RemoteCommerce.Application.Identity.Commands;

/// <summary>Authenticates a RemoteCommerce user and issues a JWT session.</summary>
/// <param name="Email">The user's email address.</param>
/// <param name="Password">The user's password.</param>
public sealed record LoginCommand(string Email, string Password)
    : ICommand<JwtAuthenticationResult>, ITransactionalCommand;

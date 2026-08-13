namespace RemoteCommerce.Application.Identity.Commands;

/// <summary>Authenticates a RemoteCommerce user and issues a JWT session.</summary>
/// <param name="Email">The user's email address.</param>
/// <param name="Password">The user's password.</param>
public sealed record LoginCommand(string Email, string Password) : ICommand<JwtAuthenticationResult>, ITransactionalCommand;

/// <summary>Contains the issued JWT session metadata.</summary>
/// <param name="Token">The signed access token.</param>
/// <param name="ExpiresAt">The UTC expiration timestamp.</param>
public sealed record JwtAuthenticationResult(string Token, DateTimeOffset ExpiresAt);

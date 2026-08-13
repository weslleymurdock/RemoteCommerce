namespace RemoteCommerce.Application.Identity.Commands;

/// <summary>Registers a RemoteCommerce user.</summary>
/// <param name="Email">The email address.</param><param name="DisplayName">The display name.</param><param name="Password">The initial password.</param>
public sealed record RegisterUserCommand(string Email, string DisplayName, string Password) : ICommand<Guid>, ITransactionalCommand;
/// <summary>Requests a password reset message.</summary><param name="Email">The email address.</param>
public sealed record ForgotPasswordCommand(string Email) : ICommand<Unit>, ITransactionalCommand;
/// <summary>Resets a password.</summary><param name="Email">The email address.</param><param name="ResetToken">The reset token.</param><param name="NewPassword">The replacement password.</param>
public sealed record ResetPasswordCommand(string Email, string ResetToken, string NewPassword) : ICommand<Unit>, ITransactionalCommand;
/// <summary>Confirms an email address.</summary><param name="UserId">The user identifier.</param><param name="Token">The confirmation token.</param>
public sealed record ConfirmEmailCommand(Guid UserId, string Token) : ICommand<Unit>, ITransactionalCommand;
/// <summary>Requests another confirmation message.</summary><param name="Email">The email address.</param>
public sealed record ResendConfirmationEmailCommand(string Email) : ICommand<Unit>, ITransactionalCommand;
/// <summary>Updates the authenticated profile.</summary><param name="DisplayName">The display name.</param><param name="Email">The email address.</param>
public sealed record UpdateProfileCommand(string DisplayName, string Email) : ICommand<Unit>, ITransactionalCommand;
/// <summary>Refreshes the authenticated JWT.</summary>
public sealed record RefreshTokenCommand : ICommand<JwtAuthenticationResult>, ITransactionalCommand;
/// <summary>Completes an authenticator-code challenge.</summary><param name="Email">The challenged email.</param><param name="Code">The authenticator code.</param><param name="RememberMachine">Whether to remember the machine.</param>
public sealed record CompleteTwoFactorCommand(string Email, string Code, bool RememberMachine) : ICommand<JwtAuthenticationResult>, ITransactionalCommand;
/// <summary>Completes a recovery-code challenge.</summary><param name="Email">The challenged email.</param><param name="RecoveryCode">The recovery code.</param>
public sealed record CompleteRecoveryCodeCommand(string Email, string RecoveryCode) : ICommand<JwtAuthenticationResult>, ITransactionalCommand;
/// <summary>Sets two-factor authentication state.</summary><param name="Enable">Whether two-factor authentication should be enabled.</param>
public sealed record SetTwoFactorCommand(bool Enable) : ICommand<TwoFactorInfo>, ITransactionalCommand;
/// <summary>Disables two-factor authentication.</summary>
public sealed record DisableTwoFactorCommand : ICommand<Unit>, ITransactionalCommand;
/// <summary>Generates new recovery codes.</summary>
public sealed record GenerateRecoveryCodesCommand : ICommand<IReadOnlyList<string>>, ITransactionalCommand;
/// <summary>Resets the authenticator key.</summary>
public sealed record ResetAuthenticatorKeyCommand : ICommand<TwoFactorInfo>, ITransactionalCommand;
/// <summary>Contains two-factor configuration safe for the browser.</summary><param name="IsEnabled">Whether 2FA is enabled.</param><param name="SharedKey">The authenticator key.</param><param name="AuthenticatorUri">The provisioning URI.</param>
public sealed record TwoFactorInfo(bool IsEnabled, string? SharedKey, string? AuthenticatorUri);
/// <summary>Contains the authenticated profile.</summary><param name="Id">The user identifier.</param><param name="Email">The email address.</param><param name="DisplayName">The display name.</param><param name="EmailConfirmed">Whether the email is confirmed.</param><param name="TwoFactorEnabled">Whether 2FA is enabled.</param>
public sealed record UserProfileResult(Guid Id, string Email, string DisplayName, bool EmailConfirmed, bool TwoFactorEnabled);

namespace RemoteCommerce.Application.Identity.Validators;

/// <summary>Validates login requests.</summary>
public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    /// <summary>Initializes login validation rules.</summary>
    public LoginCommandValidator() { RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256); RuleFor(x => x.Password).NotEmpty().MaximumLength(256); }
}
/// <summary>Validates first-administrator bootstrap requests.</summary>
public sealed class BootstrapAdministratorCommandValidator : AbstractValidator<BootstrapAdministratorCommand>
{
    /// <summary>Initializes bootstrap validation rules.</summary>
    public BootstrapAdministratorCommandValidator() { RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(200); RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256); Password(RuleFor(x => x.Password)); }
    private static void Password(IRuleBuilderInitial<string, string> rule) => rule.NotEmpty().MinimumLength(12).MaximumLength(256).Matches("[A-Z]").WithMessage("Password must contain an uppercase character.").Matches("[a-z]").WithMessage("Password must contain a lowercase character.").Matches("[0-9]").WithMessage("Password must contain a digit.").Matches("[^a-zA-Z0-9]").WithMessage("Password must contain a non-alphanumeric character.");
}
/// <summary>Validates user registration.</summary>
public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    /// <summary>Initializes registration rules.</summary>
    public RegisterUserCommandValidator() { RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256); RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(200); RuleFor(x => x.Password).NotEmpty().MinimumLength(12).MaximumLength(256); }
}
/// <summary>Validates password recovery requests.</summary>
public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    /// <summary>Initializes recovery rules.</summary>
    public ForgotPasswordCommandValidator() { RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256); }
}
/// <summary>Validates password reset requests.</summary>
public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    /// <summary>Initializes reset rules.</summary>
    public ResetPasswordCommandValidator() { RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256); RuleFor(x => x.ResetToken).NotEmpty(); RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(12).MaximumLength(256); }
}
/// <summary>Validates profile changes.</summary>
public sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    /// <summary>Initializes profile rules.</summary>
    public UpdateProfileCommandValidator() { RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(200); RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256); }
}
/// <summary>Validates authenticator challenges.</summary>
public sealed class CompleteTwoFactorCommandValidator : AbstractValidator<CompleteTwoFactorCommand>
{
    /// <summary>Initializes authenticator rules.</summary>
    public CompleteTwoFactorCommandValidator() { RuleFor(x => x.Email).NotEmpty().EmailAddress(); RuleFor(x => x.Code).NotEmpty().Length(6); }
}
/// <summary>Validates recovery-code challenges.</summary>
public sealed class CompleteRecoveryCodeCommandValidator : AbstractValidator<CompleteRecoveryCodeCommand>
{
    /// <summary>Initializes recovery-code rules.</summary>
    public CompleteRecoveryCodeCommandValidator() { RuleFor(x => x.Email).NotEmpty().EmailAddress(); RuleFor(x => x.RecoveryCode).NotEmpty(); }
}

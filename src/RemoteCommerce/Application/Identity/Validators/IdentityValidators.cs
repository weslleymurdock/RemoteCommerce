namespace RemoteCommerce.Application.Identity.Validators;

/// <summary>Validates login requests.</summary>
public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    /// <summary>Initializes login validation rules.</summary>
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(256);
    }
}

/// <summary>Validates first-administrator bootstrap requests.</summary>
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

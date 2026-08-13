namespace RemoteCommerce.Application.Localization.Validators;

/// <summary>Validates localization resource import metadata.</summary>
public sealed class ImportLocalizationResourceCommandValidator : AbstractValidator<ImportLocalizationResourceCommand>
{
    /// <summary>Initializes localization import validation rules.</summary>
    public ImportLocalizationResourceCommandValidator()
    {
        RuleFor(x => x.Content).NotNull().Must(x => x.CanRead && x.CanSeek).WithMessage("The localization resource must be a readable seekable stream.");
        RuleFor(x => x.Culture).Must(x => x is "en-US" or "pt-BR").WithMessage("Only en-US and pt-BR resources are supported.");
        RuleFor(x => x.ResourceType).NotEmpty().MaximumLength(500);
    }
}

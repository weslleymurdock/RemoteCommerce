namespace RemoteCommerce.Application.Localization;

/// <summary>Imports and activates a validated XML localization resource.</summary>
/// <param name="Content">The seekable XML resource stream.</param>
/// <param name="Culture">The resource culture.</param>
/// <param name="ResourceType">The resource marker type name.</param>
public sealed record ImportLocalizationResourceCommand(Stream Content, string Culture, string ResourceType) : ICommand<LocalizationResourceImportResult>, ITransactionalCommand;

/// <summary>Validates localization resource import metadata before persistence.</summary>
public sealed class ImportLocalizationResourceCommandValidator : AbstractValidator<ImportLocalizationResourceCommand>
{
    /// <summary>Initializes localization import validation rules.</summary>
    public ImportLocalizationResourceCommandValidator()
    {
        RuleFor(x => x.Content).NotNull().Must(stream => stream.CanRead && stream.CanSeek).WithMessage("The localization resource must be a readable seekable stream.");
        RuleFor(x => x.Culture).Must(value => value is "en-US" or "pt-BR").WithMessage("Only en-US and pt-BR resources are supported.");
        RuleFor(x => x.ResourceType).NotEmpty().MaximumLength(500);
    }
}

/// <summary>Handles transactional localization resource imports.</summary>
/// <param name="resourceService">The localization resource persistence service.</param>
/// <param name="applicationContext">The current actor context.</param>
public sealed class ImportLocalizationResourceCommandHandler(ILocalizationResourceService resourceService, IApplicationContext applicationContext) : IRequestHandler<ImportLocalizationResourceCommand, LocalizationResourceImportResult>
{
    /// <summary>Validates, persists, and activates the requested localization resource.</summary>
    /// <param name="request">The localization import request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The activated resource metadata.</returns>
    public Task<LocalizationResourceImportResult> Handle(ImportLocalizationResourceCommand request, CancellationToken cancellationToken)
        => resourceService.ImportAsync(request.Content, request.Culture, request.ResourceType.Trim(), applicationContext.UserId, applicationContext.Actor, cancellationToken);
}

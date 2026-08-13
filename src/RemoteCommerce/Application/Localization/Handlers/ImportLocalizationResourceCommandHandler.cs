namespace RemoteCommerce.Application.Localization.Handlers;

/// <summary>Handles transactional localization resource imports.</summary>
public sealed class ImportLocalizationResourceCommandHandler(ILocalizationResourceService resourceService, IApplicationContext applicationContext) : IRequestHandler<ImportLocalizationResourceCommand, LocalizationResourceImportResult>
{
    /// <inheritdoc />
    public Task<LocalizationResourceImportResult> Handle(ImportLocalizationResourceCommand request, CancellationToken cancellationToken) => resourceService.ImportAsync(request.Content, request.Culture, request.ResourceType.Trim(), applicationContext.UserId, applicationContext.Actor, cancellationToken);
}

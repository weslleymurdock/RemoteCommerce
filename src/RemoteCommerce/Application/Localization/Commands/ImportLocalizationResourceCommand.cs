namespace RemoteCommerce.Application.Localization.Commands;

/// <summary>Imports and activates a validated XML localization resource.</summary>
/// <param name="Content">The readable seekable XML resource stream.</param>
/// <param name="Culture">The resource culture.</param>
/// <param name="ResourceType">The resource marker type name.</param>
public sealed record ImportLocalizationResourceCommand(Stream Content, string Culture, string ResourceType) : ICommand<LocalizationResourceImportResult>, ITransactionalCommand;

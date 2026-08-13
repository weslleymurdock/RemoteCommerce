namespace RemoteCommerce.Application.Site.Commands;

/// <summary>Updates the persistent site settings for the current store.</summary>
/// <param name="Settings">The validated application/site settings.</param>
public sealed record UpdateSiteSettingsCommand(SiteSettingsModel Settings) : ICommand<SiteSettingsModel>, ITransactionalCommand;

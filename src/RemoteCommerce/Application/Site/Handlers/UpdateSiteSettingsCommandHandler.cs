namespace RemoteCommerce.Application.Site.Handlers;

/// <summary>Handles site settings mutation requests through the application service boundary.</summary>
public sealed class UpdateSiteSettingsCommandHandler(ISiteSettingsService siteSettings, IApplicationContext applicationContext) : IRequestHandler<UpdateSiteSettingsCommand, SiteSettingsModel>
{
    /// <inheritdoc />
    public async Task<SiteSettingsModel> Handle(UpdateSiteSettingsCommand request, CancellationToken cancellationToken)
    {
        await siteSettings.UpdateAsync(request.Settings, applicationContext.UserId, applicationContext.Actor, cancellationToken);
        return await siteSettings.GetAsync(cancellationToken);
    }
}

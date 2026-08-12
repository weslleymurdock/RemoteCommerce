namespace RemoteCommerce.Application.Site;

/// <summary>Updates the persistent site settings for the current store.</summary>
/// <param name="Settings">The validated application/site settings.</param>
public sealed record UpdateSiteSettingsCommand(SiteSettingsModel Settings) : ICommand<SiteSettingsModel>, ITransactionalCommand;

/// <summary>Validates site settings before the mutation handler executes.</summary>
public sealed class UpdateSiteSettingsCommandValidator : AbstractValidator<UpdateSiteSettingsCommand>
{
    /// <summary>Initializes the site settings command validation rules.</summary>
    public UpdateSiteSettingsCommandValidator()
    {
        RuleFor(x => x.Settings.SiteName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Settings.SiteDescription).MaximumLength(2000);
        RuleFor(x => x.Settings.PublicUrl).Must(IsHttpUrl).WithMessage("Public URL must be an absolute HTTP or HTTPS URL.");
        RuleFor(x => x.Settings.Culture).Must(BeSupportedCulture).WithMessage("Culture must be en-US or pt-BR.");
        RuleFor(x => x.Settings.Locale).Must(BeSupportedCulture).WithMessage("Locale must be en-US or pt-BR.");
        RuleFor(x => x.Settings.TimeZone).Must(BeValidTimeZone).WithMessage("Time zone must be supported by the host.");
    }

    private static bool IsHttpUrl(string value)
        => Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

    private static bool BeSupportedCulture(string value) => value is "en-US" or "pt-BR";

    private static bool BeValidTimeZone(string value)
    {
        try
        {
            _ = TimeZoneInfo.FindSystemTimeZoneById(value);
            return true;
        }
        catch (TimeZoneNotFoundException)
        {
            return false;
        }
        catch (InvalidTimeZoneException)
        {
            return false;
        }
    }
}

/// <summary>Handles site settings mutation requests through the application service boundary.</summary>
/// <param name="siteSettings">The site settings service.</param>
/// <param name="applicationContext">The current actor context.</param>
public sealed class UpdateSiteSettingsCommandHandler(
    ISiteSettingsService siteSettings,
    IApplicationContext applicationContext) : IRequestHandler<UpdateSiteSettingsCommand, SiteSettingsModel>
{
    /// <summary>Persists the requested site settings.</summary>
    /// <param name="request">The site settings mutation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The normalized persisted settings.</returns>
    public async Task<SiteSettingsModel> Handle(UpdateSiteSettingsCommand request, CancellationToken cancellationToken)
    {
        await siteSettings.UpdateAsync(request.Settings, applicationContext.UserId, applicationContext.Actor, cancellationToken);
        return await siteSettings.GetAsync(cancellationToken);
    }
}

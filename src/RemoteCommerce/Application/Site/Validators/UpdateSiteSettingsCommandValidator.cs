namespace RemoteCommerce.Application.Site.Validators;

/// <summary>Validates persistent site settings.</summary>
public sealed class UpdateSiteSettingsCommandValidator : AbstractValidator<UpdateSiteSettingsCommand>
{
    /// <summary>Initializes site settings validation rules.</summary>
    public UpdateSiteSettingsCommandValidator()
    {
        RuleFor(x => x.Settings.SiteName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Settings.SiteDescription).MaximumLength(2000);
        RuleFor(x => x.Settings.PublicUrl).Must(IsHttpUrl).WithMessage("Public URL must be an absolute HTTP or HTTPS URL.");
        RuleFor(x => x.Settings.Culture).Must(IsSupportedCulture).WithMessage("Culture must be en-US or pt-BR.");
        RuleFor(x => x.Settings.Locale).Must(IsSupportedCulture).WithMessage("Locale must be en-US or pt-BR.");
        RuleFor(x => x.Settings.TimeZone).Must(IsValidTimeZone).WithMessage("Time zone must be supported by the host.");
    }
    private static bool IsHttpUrl(string value) => Uri.TryCreate(value, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    private static bool IsSupportedCulture(string value) => value is "en-US" or "pt-BR";
    private static bool IsValidTimeZone(string value)
    {
        try { _ = TimeZoneInfo.FindSystemTimeZoneById(value); return true; }
        catch (TimeZoneNotFoundException) { return false; }
        catch (InvalidTimeZoneException) { return false; }
    }
}

namespace RemoteCommerce.Application.Site;

/// <summary>Defines the editable application settings exposed by the administration layer.</summary>
public sealed record SiteSettingsModel
{
    /// <summary>Gets the public site name.</summary>
    public string SiteName { get; init; } = "RemoteCommerce";

    /// <summary>Gets the public site description.</summary>
    public string SiteDescription { get; init; } = string.Empty;

    /// <summary>Gets the canonical public/base URL.</summary>
    public string PublicUrl { get; init; } = "https://localhost";

    /// <summary>Gets the IANA or platform-supported time zone identifier.</summary>
    public string TimeZone { get; init; } = "UTC";

    /// <summary>Gets the default UI culture.</summary>
    public string Culture { get; init; } = "en-US";

    /// <summary>Gets the locale used for application formatting defaults.</summary>
    public string Locale { get; init; } = "en-US";
}

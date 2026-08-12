namespace RemoteCommerce.Application.Site;

/// <summary>Defines the editable application settings exposed by the administration layer.</summary>
public sealed record SiteSettingsModel
{
    /// <summary>Gets or sets the public site name.</summary>
    public string SiteName { get; set; } = "RemoteCommerce";

    /// <summary>Gets or sets the public site description.</summary>
    public string SiteDescription { get; set; } = string.Empty;

    /// <summary>Gets or sets the canonical public/base URL.</summary>
    public string PublicUrl { get; set; } = "https://localhost";

    /// <summary>Gets or sets the IANA or platform-supported time zone identifier.</summary>
    public string TimeZone { get; set; } = "UTC";

    /// <summary>Gets or sets the default UI culture.</summary>
    public string Culture { get; set; } = "en-US";

    /// <summary>Gets or sets the locale used for application formatting defaults.</summary>
    public string Locale { get; set; } = "en-US";
}

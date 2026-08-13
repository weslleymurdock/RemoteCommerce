namespace RemoteCommerce.Infrastructure.Persistence.Entities;

/// <summary>Stores editable application-level settings for the current store.</summary>
public sealed class SiteSettings : Persistence.ISoftDeletable
{
    /// <summary>Gets the singleton settings identifier.</summary>
    public int Id { get; set; } = 1;

    /// <summary>Gets or sets the public site name.</summary>
    public string SiteName { get; set; } = "RemoteCommerce";

    /// <summary>Gets or sets the public site description.</summary>
    public string SiteDescription { get; set; } = string.Empty;

    /// <summary>Gets or sets the canonical public/base URL.</summary>
    public string PublicUrl { get; set; } = "https://localhost";

    /// <summary>Gets or sets the configured IANA time zone identifier.</summary>
    public string TimeZone { get; set; } = "UTC";

    /// <summary>Gets or sets the default UI culture.</summary>
    public string Culture { get; set; } = "en-US";

    /// <summary>Gets or sets the locale used for application formatting defaults.</summary>
    public string Locale { get; set; } = "en-US";

    /// <summary>Gets or sets the UTC timestamp of the last administrative update.</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <inheritdoc />
    public bool IsDisabled { get; set; }
}

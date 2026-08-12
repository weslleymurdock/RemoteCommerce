using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using RemoteCommerce.Infrastructure.Persistence;

namespace RemoteCommerce.Application.Localization;

/// <summary>Uses persisted site culture as the lowest-priority application culture provider.</summary>
/// <param name="dbFactory">The factory used to read site configuration.</param>
public sealed class SiteSettingsRequestCultureProvider(IDbContextFactory<CommerceDbContext> dbFactory) : RequestCultureProvider
{
    /// <summary>Determines the current request culture from persisted site settings.</summary>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <returns>The configured culture when valid; otherwise no provider result.</returns>
    public override async Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        await using var db = await dbFactory.CreateDbContextAsync(httpContext.RequestAborted);
        var settings = await db.SiteSettings.AsNoTracking().SingleOrDefaultAsync(x => x.Id == 1, httpContext.RequestAborted);
        if (settings is null)
        {
            return null;
        }

        try
        {
            _ = CultureInfo.GetCultureInfo(settings.Culture);
            return new ProviderCultureResult(settings.Culture, settings.Culture);
        }
        catch (CultureNotFoundException)
        {
            return null;
        }
    }
}

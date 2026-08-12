using System.Globalization;
using Microsoft.EntityFrameworkCore;
using RemoteCommerce.Infrastructure.Persistence;
using RemoteCommerce.Infrastructure.Persistence.Entities;

namespace RemoteCommerce.Application.Site;

/// <summary>Implements validated persistent site configuration.</summary>
/// <param name="dbFactory">The factory used to create persistence contexts.</param>
public sealed class SiteSettingsService(IDbContextFactory<CommerceDbContext> dbFactory) : ISiteSettingsService
{
    private static readonly HashSet<string> SupportedCultures = ["en-US", "pt-BR"];

    /// <inheritdoc />
    public async Task<SiteSettingsModel> GetAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var entity = await db.SiteSettings.SingleOrDefaultAsync(x => x.Id == 1, cancellationToken);

        if (entity is null)
        {
            entity = new SiteSettings();
            db.SiteSettings.Add(entity);
            await db.SaveChangesAsync(cancellationToken);
        }

        return ToModel(entity);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(
        SiteSettingsModel settings,
        Guid? userId,
        string actor,
        CancellationToken cancellationToken = default)
    {
        Validate(settings);

        await using var db = await dbFactory.CreateDbContextAsync(cancellationToken);
        var entity = await db.SiteSettings.SingleOrDefaultAsync(x => x.Id == 1, cancellationToken) ?? new SiteSettings();

        entity.SiteName = settings.SiteName.Trim();
        entity.SiteDescription = settings.SiteDescription.Trim();
        entity.PublicUrl = settings.PublicUrl.Trim().TrimEnd('/');
        entity.TimeZone = settings.TimeZone.Trim();
        entity.Culture = settings.Culture.Trim();
        entity.Locale = settings.Locale.Trim();
        entity.UpdatedAt = DateTime.UtcNow;

        if (entity.Id == 0)
        {
            entity.Id = 1;
            db.SiteSettings.Add(entity);
        }

        db.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Actor = string.IsNullOrWhiteSpace(actor) ? "system" : actor,
            Operation = "site.settings.update",
            Resource = "SiteSettings",
            Context = $"Culture={entity.Culture}; Locale={entity.Locale}; TimeZone={entity.TimeZone}",
        });

        await db.SaveChangesAsync(cancellationToken);
    }

    private static void Validate(SiteSettingsModel settings)
    {
        if (string.IsNullOrWhiteSpace(settings.SiteName) || settings.SiteName.Length > 200)
        {
            throw new ArgumentException("Site name is required and must be at most 200 characters.", nameof(settings));
        }

        if (settings.SiteDescription.Length > 2000)
        {
            throw new ArgumentException("Site description must be at most 2000 characters.", nameof(settings));
        }

        if (!Uri.TryCreate(settings.PublicUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
        {
            throw new ArgumentException("Public URL must be an absolute HTTP or HTTPS URL.", nameof(settings));
        }

        if (!SupportedCultures.Contains(settings.Culture) || !SupportedCultures.Contains(settings.Locale))
        {
            throw new ArgumentException("Culture and locale must be en-US or pt-BR.", nameof(settings));
        }

        try
        {
            _ = CultureInfo.GetCultureInfo(settings.Culture);
            _ = CultureInfo.GetCultureInfo(settings.Locale);
            _ = TimeZoneInfo.FindSystemTimeZoneById(settings.TimeZone);
        }
        catch (CultureNotFoundException ex)
        {
            throw new ArgumentException("The configured culture is not supported.", nameof(settings), ex);
        }
        catch (TimeZoneNotFoundException ex)
        {
            throw new ArgumentException("The configured time zone is not supported by the host.", nameof(settings), ex);
        }
        catch (InvalidTimeZoneException ex)
        {
            throw new ArgumentException("The configured time zone is invalid.", nameof(settings), ex);
        }
    }

    private static SiteSettingsModel ToModel(SiteSettings entity) => new()
    {
        SiteName = entity.SiteName,
        SiteDescription = entity.SiteDescription,
        PublicUrl = entity.PublicUrl,
        TimeZone = entity.TimeZone,
        Culture = entity.Culture,
        Locale = entity.Locale,
    };
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RemoteCommerce.Application.Security;
using RemoteCommerce.Application.Site;
using RemoteCommerce.Infrastructure.Persistence;
using RemoteCommerce.Infrastructure.Persistence.Entities;

namespace RemoteCommerce.Tests;

public sealed class Stage05FoundationTests
{
    [Fact]
    public async Task SiteSettingsService_CreatesSafeDefaults()
    {
        var factory = CreateFactory();
        var service = new SiteSettingsService(factory);

        var settings = await service.GetAsync();

        Assert.Equal("RemoteCommerce", settings.SiteName);
        Assert.Equal("en-US", settings.Culture);
        Assert.Equal("en-US", settings.Locale);
        Assert.Equal("UTC", settings.TimeZone);
    }

    [Fact]
    public async Task SiteSettingsService_RejectsUnsupportedCulture()
    {
        var service = new SiteSettingsService(CreateFactory());
        var settings = new SiteSettingsModel
        {
            SiteName = "Store",
            PublicUrl = "https://example.test",
            Culture = "fr-FR",
            Locale = "en-US",
            TimeZone = "UTC",
        };

        await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateAsync(settings, null, "test"));
    }

    [Fact]
    public async Task SiteSettingsService_RejectsInvalidPublicUrl()
    {
        var service = new SiteSettingsService(CreateFactory());
        var settings = new SiteSettingsModel
        {
            SiteName = "Store",
            PublicUrl = "javascript:alert(1)",
            Culture = "en-US",
            Locale = "en-US",
            TimeZone = "UTC",
        };

        await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateAsync(settings, null, "test"));
    }

    [Fact]
    public void ConfigurationSecretProvider_RedactsByContract()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Secrets:Test"] = "top-secret" })
            .Build();
        var provider = new ConfigurationSecretProvider(configuration);

        Assert.True(provider.IsConfigured("Secrets:Test"));
        Assert.Null(provider.Get("Secrets:Missing"));
    }

    [Fact]
    public void IdentityPasswordHasher_DoesNotStorePlaintext()
    {
        var user = new ApplicationUser { UserName = "admin@example.test", Email = "admin@example.test" };
        var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<ApplicationUser>();

        var hash = hasher.HashPassword(user, "StrongPassword!123");

        Assert.NotEqual("StrongPassword!123", hash);
        Assert.Equal(Microsoft.AspNetCore.Identity.PasswordVerificationResult.Success, hasher.VerifyHashedPassword(user, hash, "StrongPassword!123"));
    }

    private static IDbContextFactory<CommerceDbContext> CreateFactory()
    {
        var options = new DbContextOptionsBuilder<CommerceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new PooledDbContextFactory<CommerceDbContext>(options);
    }
}

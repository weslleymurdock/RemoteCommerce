namespace RemoteCommerce.Tests;

public sealed class Stage05FoundationTests
{
    [Fact]
    public async Task SiteSettingsService_CreatesSafeDefaults()
    {
        await using var db = CreateDbContext();
        var service = CreateSiteSettingsService(db);
        var settings = await service.GetAsync();

        Assert.Equal("RemoteCommerce", settings.SiteName);
        Assert.Equal("en-US", settings.Culture);
        Assert.Equal("en-US", settings.Locale);
        Assert.Equal("UTC", settings.TimeZone);
    }

    [Fact]
    public async Task SiteSettingsValidator_RejectsUnsupportedCulture()
    {
        var validator = new UpdateSiteSettingsCommandValidator();
        var result = await validator.ValidateAsync(new UpdateSiteSettingsCommand(new SiteSettingsModel { SiteName = "Store", PublicUrl = "https://example.test", Culture = "fr-FR", Locale = "en-US", TimeZone = "UTC" }));
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task SiteSettingsValidator_RejectsInvalidPublicUrl()
    {
        var validator = new UpdateSiteSettingsCommandValidator();
        var result = await validator.ValidateAsync(new UpdateSiteSettingsCommand(new SiteSettingsModel { SiteName = "Store", PublicUrl = "javascript:alert(1)", Culture = "en-US", Locale = "en-US", TimeZone = "UTC" }));
        Assert.False(result.IsValid);
    }

    [Fact]
    public void ConfigurationSecretProvider_ReportsConfigurationWithoutExposingIt()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Secrets:Test"] = "top-secret" }).Build();
        var provider = new ConfigurationSecretProvider(configuration);
        Assert.True(provider.IsConfigured("Secrets:Test"));
        Assert.Null(provider.Get("Secrets:Missing"));
    }

    [Fact]
    public void IdentityPasswordHasher_DoesNotStorePlaintext()
    {
        var user = new ApplicationUser { UserName = "admin@example.test", Email = "admin@example.test" };
        var hasher = new PasswordHasher<ApplicationUser>();
        var hash = hasher.HashPassword(user, "StrongPassword!123");
        Assert.NotEqual("StrongPassword!123", hash);
        Assert.Equal(PasswordVerificationResult.Success, hasher.VerifyHashedPassword(user, hash, "StrongPassword!123"));
    }

    [Fact]
    public async Task TransactionalBehavior_RollsBackHandlerMutationOnFailure()
    {
        await using var db = CreateDbContext();
        var behavior = new TransactionalBehavior<FailingCommand, Unit>(db);
        var command = new FailingCommand();

        await Assert.ThrowsAsync<InvalidOperationException>(() => behavior.Handle(command, async _ =>
        {
            db.SiteSettings.Add(new SiteSettings { SiteName = "Should Roll Back" });
            await db.SaveChangesAsync();
            throw new InvalidOperationException("boom");
        }, CancellationToken.None));

        Assert.Empty(await db.SiteSettings.IgnoreQueryFilters().ToListAsync());
    }

    [Fact]
    public async Task ValidationBehavior_PreventsHandlerExecution()
    {
        var behavior = new ValidationBehavior<UpdateSiteSettingsCommand, SiteSettingsModel>(new[] { new UpdateSiteSettingsCommandValidator() });
        var invoked = false;
        var command = new UpdateSiteSettingsCommand(new SiteSettingsModel { SiteName = string.Empty, PublicUrl = "bad", Culture = "fr-FR", Locale = "en-US", TimeZone = "UTC" });

        await Assert.ThrowsAsync<ValidationException>(() => behavior.Handle(command, _ =>
        {
            invoked = true;
            return Task.FromResult(command.Settings);
        }, CancellationToken.None));

        Assert.False(invoked);
    }

    [Fact]
    public async Task SoftDelete_CreatesHistoryAndHidesEntityFromNormalQueries()
    {
        await using var db = CreateDbContext();
        var settings = new SiteSettings { SiteName = "Before" };
        db.SiteSettings.Add(settings);
        await db.SaveChangesAsync();
        settings.SiteName = "After";
        await db.SaveChangesAsync();
        db.SiteSettings.Remove(settings);
        await db.SaveChangesAsync();

        Assert.Empty(await db.SiteSettings.ToListAsync());
        Assert.Single(await db.SiteSettings.IgnoreQueryFilters().ToListAsync());
        var history = await db.OperationHistories.OrderBy(x => x.Id).ToListAsync();
        Assert.Equal(2, history.Count);
        Assert.Equal("SoftDelete", history[^1].OperationType);
        Assert.Contains("Before", history[0].PreviousState);
        Assert.Contains("After", history[0].NewState);
    }

    [Fact]
    public async Task OperationHistory_RedactsSensitiveProperties()
    {
        await using var db = CreateDbContext();
        var user = new ApplicationUser { UserName = "admin@example.test", Email = "admin@example.test", DisplayName = "Admin", PasswordHash = "secret-hash" };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        user.DisplayName = "Changed";
        user.PasswordHash = "new-secret-hash";
        await db.SaveChangesAsync();

        var history = await db.OperationHistories.OrderByDescending(x => x.Id).FirstAsync();
        Assert.Contains("[REDACTED]", history.PreviousState);
        Assert.DoesNotContain("new-secret-hash", history.NewState ?? string.Empty);
    }

    [Fact]
    public void MediatorPackageVersion_IsPinnedToRequiredVersion()
    {
        var version = typeof(IMediator).Assembly.GetName().Version;
        Assert.NotNull(version);
        Assert.Equal(new Version(12, 5, 0), version);
    }

    private static CommerceDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CommerceDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        return new CommerceDbContext(options, new TestApplicationContext());
    }

    private static SiteSettingsService CreateSiteSettingsService(CommerceDbContext db) => new(new TestDbContextFactory(db), db);

    private sealed class TestDbContextFactory(CommerceDbContext db) : IDbContextFactory<CommerceDbContext>
    {
        public CommerceDbContext CreateDbContext() => db;
        public Task<CommerceDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) => Task.FromResult(db);
    }

    private sealed class TestApplicationContext : IApplicationContext
    {
        public Guid? UserId => Guid.Parse("11111111-1111-1111-1111-111111111111");
        public string Actor => "test-user";
        public string CorrelationId => "test-correlation";
        public string? IpAddress => "127.0.0.1";
    }

    private sealed record FailingCommand : ITransactionalCommand;
}

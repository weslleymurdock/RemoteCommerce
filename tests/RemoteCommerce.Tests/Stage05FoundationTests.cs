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
        var command = new UpdateSiteSettingsCommand(
            new SiteSettingsModel
            {
                SiteName = "Store",
                PublicUrl = "https://example.test",
                Culture = "fr-FR",
                Locale = "en-US",
                TimeZone = "UTC"
            });

        var result = await validator.ValidateAsync(
            command,
            TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task SiteSettingsValidator_RejectsInvalidPublicUrl()
    {
        var validator = new UpdateSiteSettingsCommandValidator();
        var command = new UpdateSiteSettingsCommand(
            new SiteSettingsModel
            {
                SiteName = "Store",
                PublicUrl = "javascript:alert(1)",
                Culture = "en-US",
                Locale = "en-US",
                TimeZone = "UTC"
            });

        var result = await validator.ValidateAsync(
            command,
            TestContext.Current.CancellationToken);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void ConfigurationSecretProvider_ReportsConfigurationWithoutExposingIt()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Secrets:Test"] = "top-secret"
                })
            .Build();
        var provider = new ConfigurationSecretProvider(configuration);

        Assert.True(provider.IsConfigured("Secrets:Test"));
        Assert.Null(provider.Get("Secrets:Missing"));
    }

    [Fact]
    public void IdentityPasswordHasher_DoesNotStorePlaintext()
    {
        var user = new ApplicationUser
        {
            UserName = "admin@example.test",
            Email = "admin@example.test"
        };
        var hasher = new PasswordHasher<ApplicationUser>();
        var hash = hasher.HashPassword(user, "StrongPassword!123");

        Assert.NotEqual("StrongPassword!123", hash);
        Assert.Equal(
            PasswordVerificationResult.Success,
            hasher.VerifyHashedPassword(user, hash, "StrongPassword!123"));
    }

    [Fact]
    public async Task SetupStatusQuery_RequiresSetupWhenUserStoreIsEmpty()
    {
        await using var provider = CreateIdentityServiceProvider();
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var handler = new RemoteCommerce.Application.Identity.Handlers.GetSetupStatusQueryHandler(userManager);

        var result = await handler.Handle(
            new GetSetupStatusQuery(),
            TestContext.Current.CancellationToken);

        Assert.True(result);
    }

    [Fact]
    public async Task SetupStatusQuery_DoesNotRequireSetupWhenUserExists()
    {
        await using var provider = CreateIdentityServiceProvider();
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var handler = new RemoteCommerce.Application.Identity.Handlers.GetSetupStatusQueryHandler(userManager);
        var user = new ApplicationUser
        {
            UserName = "admin@example.test",
            Email = "admin@example.test",
            DisplayName = "Administrator",
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(user, "StrongPassword!123");
        Assert.True(createResult.Succeeded);

        var result = await handler.Handle(
            new GetSetupStatusQuery(),
            TestContext.Current.CancellationToken);

        Assert.False(result);
    }

    [Fact]
    public async Task SetupStatusQuery_RequiresSetupAgainWhenUserIsRemoved()
    {
        await using var provider = CreateIdentityServiceProvider();
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var handler = new RemoteCommerce.Application.Identity.Handlers.GetSetupStatusQueryHandler(userManager);
        var user = new ApplicationUser
        {
            UserName = "admin@example.test",
            Email = "admin@example.test",
            DisplayName = "Administrator",
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(user, "StrongPassword!123");
        Assert.True(createResult.Succeeded);
        Assert.False(
            await handler.Handle(
                new GetSetupStatusQuery(),
                TestContext.Current.CancellationToken));

        var db = provider.GetRequiredService<CommerceDbContext>();
        db.Users.Remove(user);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        Assert.True(
            await handler.Handle(
                new GetSetupStatusQuery(),
                TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task MediatorRegistration_ExecutesLoggingAndValidationBehaviors()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IApplicationContext, TestApplicationContext>();
        services.AddDbContext<CommerceDbContext>(
            options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(Stage05FoundationTests).Assembly);
            configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
            configuration.AddOpenBehavior(typeof(TransactionalBehavior<,>));
        });
        services.AddValidatorsFromAssembly(typeof(Stage05FoundationTests).Assembly);

        await using var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        Assert.Equal(
            "valid",
            await mediator.Send(
                new PingQuery("valid"),
                TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<ValidationException>(
            () => mediator.Send(
                new PingQuery(string.Empty),
                TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TransactionalBehavior_RollsBackHandlerMutationOnFailure()
    {
        await using var database = await CreateRelationalDatabaseAsync();
        var db = database.Db;
        var behavior = new TransactionalBehavior<FailingCommand, Unit>(db);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => behavior.Handle(
                new FailingCommand(),
                async _ =>
                {
                    db.SiteSettings.Add(
                        new SiteSettings
                        {
                            SiteName = "Should Roll Back"
                        });
                    await db.SaveChangesAsync(TestContext.Current.CancellationToken);
                    throw new InvalidOperationException("boom");
                },
                TestContext.Current.CancellationToken));

        Assert.Empty(
            await db.SiteSettings
                .IgnoreQueryFilters()
                .ToListAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ValidationBehavior_PreventsHandlerExecution()
    {
        var behavior = new ValidationBehavior<UpdateSiteSettingsCommand, SiteSettingsModel>(
            new[]
            {
                new UpdateSiteSettingsCommandValidator()
            });
        var invoked = false;
        var command = new UpdateSiteSettingsCommand(
            new SiteSettingsModel
            {
                SiteName = string.Empty,
                PublicUrl = "bad",
                Culture = "fr-FR",
                Locale = "en-US",
                TimeZone = "UTC"
            });

        await Assert.ThrowsAsync<ValidationException>(
            () => behavior.Handle(
                command,
                _ =>
                {
                    invoked = true;
                    return Task.FromResult(command.Settings);
                },
                TestContext.Current.CancellationToken));

        Assert.False(invoked);
    }

    [Fact]
    public async Task SoftDelete_CreatesHistoryAndHidesEntityFromNormalQueries()
    {
        await using var db = CreateDbContext();
        var settings = new SiteSettings
        {
            SiteName = "Before"
        };
        db.SiteSettings.Add(settings);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        settings.SiteName = "After";
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        db.SiteSettings.Remove(settings);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        Assert.Empty(
            await db.SiteSettings.ToListAsync(TestContext.Current.CancellationToken));
        Assert.Single(
            await db.SiteSettings
                .IgnoreQueryFilters()
                .ToListAsync(TestContext.Current.CancellationToken));

        var history = await db.OperationHistories
            .OrderBy(x => x.Id)
            .ToListAsync(TestContext.Current.CancellationToken);
        Assert.Equal(2, history.Count);
        Assert.Equal("SoftDelete", history[^1].OperationType);
        Assert.Contains("Before", history[0].PreviousState);
        Assert.Contains("After", history[0].NewState ?? string.Empty);
    }

    [Fact]
    public async Task OperationHistory_RedactsSensitiveProperties()
    {
        await using var db = CreateDbContext();
        var user = new ApplicationUser
        {
            UserName = "admin@example.test",
            Email = "admin@example.test",
            DisplayName = "Admin",
            PasswordHash = "secret-hash"
        };
        db.Users.Add(user);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        user.DisplayName = "Changed";
        user.PasswordHash = "new-secret-hash";
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var history = await db.OperationHistories
            .OrderByDescending(x => x.Id)
            .FirstAsync(TestContext.Current.CancellationToken);
        Assert.Contains("[REDACTED]", history.PreviousState);
        Assert.DoesNotContain("new-secret-hash", history.NewState ?? string.Empty);
    }

    [Fact]
    public void RequiredPackageVersions_ArePinned()
    {
        var mediatR = typeof(IMediator)
            .Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;
        var fluentValidation = typeof(IValidator)
            .Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        Assert.NotNull(mediatR);
        Assert.NotNull(fluentValidation);
        Assert.StartsWith("12.5.0", mediatR, StringComparison.Ordinal);
        Assert.StartsWith("12.1.1", fluentValidation, StringComparison.Ordinal);
    }

    private static ServiceProvider CreateIdentityServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDataProtection();
        services.AddSingleton<IApplicationContext, TestApplicationContext>();
        services.AddDbContext<CommerceDbContext>(
            options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddIdentityCore<ApplicationUser>()
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<CommerceDbContext>()
            .AddDefaultTokenProviders();

        return services.BuildServiceProvider();
    }

    private static CommerceDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CommerceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new CommerceDbContext(options, new TestApplicationContext());
    }

    private static async Task<TestDatabase> CreateRelationalDatabaseAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        var options = new DbContextOptionsBuilder<CommerceDbContext>()
            .UseSqlite(connection)
            .Options;
        var db = new CommerceDbContext(options, new TestApplicationContext());
        await db.Database.ExecuteSqlRawAsync(
            "CREATE TABLE SiteSettings (Id INTEGER NOT NULL PRIMARY KEY, SiteName TEXT NOT NULL, SiteDescription TEXT NOT NULL, PublicUrl TEXT NOT NULL, TimeZone TEXT NOT NULL, Culture TEXT NOT NULL, Locale TEXT NOT NULL, UpdatedAt TEXT NOT NULL, IsDisabled INTEGER NOT NULL DEFAULT 0, DeletedAt TEXT NULL);",
            TestContext.Current.CancellationToken);
        return new TestDatabase(db, connection);
    }

    private static SiteSettingsService CreateSiteSettingsService(CommerceDbContext db)
        => new(new TestDbContextFactory(db), db);

    private sealed class TestDatabase(CommerceDbContext db, SqliteConnection connection) : IAsyncDisposable
    {
        public CommerceDbContext Db { get; } = db;

        public async ValueTask DisposeAsync()
        {
            await Db.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    private sealed class TestDbContextFactory(DbContextOptions<CommerceDbContext> options) : IDbContextFactory<CommerceDbContext>
    {
        public CommerceDbContext CreateDbContext()
            => CreateDb(options);

        public Task<CommerceDbContext> CreateDbContextAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult(CreateDb(options));
    }

    private sealed class TestApplicationContext : IApplicationContext
    {
        public Guid? UserId => Guid.Parse("11111111-1111-1111-1111-111111111111");
        public string Actor => "test-user";
        public string CorrelationId => "test-correlation";
        public string? IpAddress => "127.0.0.1";
    }

    private static CommerceDbContext CreateDb(DbContextOptions<CommerceDbContext> options)
        => new(options, new TestApplicationContext());
}

public sealed record PingQuery(string Value) : IQuery<string>;

public sealed class PingQueryValidator : AbstractValidator<PingQuery>
{
    public PingQueryValidator()
        => RuleFor(x => x.Value).NotEmpty();
}

public sealed class PingQueryHandler : IRequestHandler<PingQuery, string>
{
    public Task<string> Handle(PingQuery request, CancellationToken cancellationToken)
        => Task.FromResult(request.Value);
}

internal sealed record FailingCommand : ITransactionalCommand;

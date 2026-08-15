namespace RemoteCommerce.Tests;

public sealed class SoftDeletePersistenceTests
{
    [Fact]
    public async Task DeletedEntity_IsPersistedAsDisabledAndHiddenByDefault()
    {
        var options = new DbContextOptionsBuilder<CommerceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var db = new CommerceDbContext(options, new TestApplicationContext());

        var settings = new SiteSettings { SiteName = "Before" };
        db.SiteSettings.Add(settings);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        db.SiteSettings.Remove(settings);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        Assert.Empty(await db.SiteSettings.ToListAsync(TestContext.Current.CancellationToken));
        var persisted = await db.SiteSettings.IgnoreQueryFilters().SingleAsync(TestContext.Current.CancellationToken);
        Assert.True(persisted.IsDisabled);
        Assert.Single(await db.OperationHistories.ToListAsync(TestContext.Current.CancellationToken));

        Assert.Equal("SoftDelete", (await db.OperationHistories.SingleAsync(TestContext.Current.CancellationToken)).OperationType);
    }

    private sealed class TestApplicationContext : IApplicationContext
    {
        public Guid? UserId => null;
        public string Actor => "test";
        public string CorrelationId => "test-correlation";
        public string? IpAddress => "127.0.0.1";
    }
}

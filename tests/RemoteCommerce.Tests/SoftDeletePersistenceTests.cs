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
        await db.SaveChangesAsync();

        db.SiteSettings.Remove(settings);
        await db.SaveChangesAsync();

        Assert.Empty(await db.SiteSettings.ToListAsync());
        var persisted = await db.SiteSettings.IgnoreQueryFilters().SingleAsync();
        Assert.True(persisted.IsDisabled);
        Assert.Single(await db.OperationHistories.ToListAsync());

        Assert.Equal("SoftDelete", (await db.OperationHistories.SingleAsync()).OperationType);
    }

    private sealed class TestApplicationContext : IApplicationContext
    {
        public Guid? UserId => null;
        public string Actor => "test";
        public string CorrelationId => "test-correlation";
        public string? IpAddress => "127.0.0.1";
    }
}

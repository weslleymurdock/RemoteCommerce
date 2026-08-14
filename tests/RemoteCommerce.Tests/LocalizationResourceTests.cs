namespace RemoteCommerce.Tests;

public sealed class LocalizationResourceTests
{
    [Fact]
    public async Task ImportAsync_StoresValidResourceAndVersion()
    {
        var root = Path.Combine(Path.GetTempPath(), "RemoteCommerceTests", Guid.NewGuid().ToString("N")); Directory.CreateDirectory(root);
        try
        {
            await using var database = CreateDatabase();
            var service = new LocalizationResourceService(database.Factory, database.Db, new TestEnvironment(root));
            await using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("<root><data name=\"Hello\"><value>Olá</value></data></root>"));
            var result = await service.ImportAsync(stream, "pt-BR", "RemoteCommerce.Tests.SharedResource", null, "test", TestContext.Current.CancellationToken);
            Assert.Equal(1, result.Version); Assert.Equal(1, result.EntryCount); Assert.True(System.IO.File.Exists(Path.Combine(root, "App_Data", "localization", "RemoteCommerce.Tests.SharedResource", "pt-BR", "v1.xml")));
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public async Task ImportAsync_RejectsDuplicateKeys()
    {
        var root = Path.Combine(Path.GetTempPath(), "RemoteCommerceTests", Guid.NewGuid().ToString("N")); Directory.CreateDirectory(root);
        try
        {
            await using var database = CreateDatabase(); var service = new LocalizationResourceService(database.Factory, database.Db, new TestEnvironment(root));
            await using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("<root><data name=\"Hello\"><value>A</value></data><data name=\"Hello\"><value>B</value></data></root>"));
            await Assert.ThrowsAsync<InvalidDataException>(() => service.ImportAsync(stream, "en-US", "SharedResource", null, "test", TestContext.Current.CancellationToken));
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public async Task ImportAsync_RejectsExternalEntities()
    {
        var root = Path.Combine(Path.GetTempPath(), "RemoteCommerceTests", Guid.NewGuid().ToString("N")); Directory.CreateDirectory(root);
        try
        {
            await using var database = CreateDatabase(); var service = new LocalizationResourceService(database.Factory, database.Db, new TestEnvironment(root));
            var xml = "<!DOCTYPE root [<!ENTITY xxe SYSTEM 'file:///etc/passwd'>]><root><data name=\"Hello\"><value>&xxe;</value></data></root>";
            await using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xml));
            await Assert.ThrowsAnyAsync<Exception>(() => service.ImportAsync(stream, "en-US", "SharedResource", null, "test", TestContext.Current.CancellationToken));
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public async Task ImportAsync_RejectsUnsupportedCulture()
    {
        await using var database = CreateDatabase(); var service = new LocalizationResourceService(database.Factory, database.Db, new TestEnvironment(Path.GetTempPath()));
        await using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("<root />"));
        await Assert.ThrowsAsync<ArgumentException>(() => service.ImportAsync(stream, "fr-FR", "SharedResource", null, "test", TestContext.Current.CancellationToken));
    }

    private static TestDatabase CreateDatabase()
    {
        var options = new DbContextOptionsBuilder<CommerceDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var db = new CommerceDbContext(options, new TestApplicationContext());
        return new TestDatabase(db, new TestDbContextFactory(db));
    }

    private sealed class TestDatabase(CommerceDbContext db, IDbContextFactory<CommerceDbContext> factory) : IAsyncDisposable
    {
        public CommerceDbContext Db { get; } = db;
        public IDbContextFactory<CommerceDbContext> Factory { get; } = factory;
        public ValueTask DisposeAsync() => Db.DisposeAsync();
    }

    private sealed class TestDbContextFactory(CommerceDbContext db) : IDbContextFactory<CommerceDbContext>
    {
        public CommerceDbContext CreateDbContext() => db;
        public Task<CommerceDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) => Task.FromResult(db);
    }

    private sealed class TestApplicationContext : IApplicationContext
    {
        public Guid? UserId => null; public string Actor => "test"; public string CorrelationId => "test"; public string? IpAddress => null;
    }

    private sealed class TestEnvironment(string contentRootPath) : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Development"; 
        public string ApplicationName { get; set; } = "RemoteCommerce.Tests"; 
        public string WebRootPath { get; set; } = contentRootPath; 
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider(); 
        public string ContentRootPath { get; set; } = contentRootPath; 
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}

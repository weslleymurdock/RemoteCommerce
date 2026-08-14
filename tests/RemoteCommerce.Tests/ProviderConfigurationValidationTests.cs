namespace RemoteCommerce.Tests;

public sealed class ProviderConfigurationValidationTests
{
    [Fact]
    public async Task StartupValidation_RejectsMongoWithoutConnectionString()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Media:Provider"] = "MongoGridFS",
                    ["Media:MongoGridFs:DatabaseName"] = "RemoteCommerceMedia",
                    ["Media:MongoGridFs:BucketName"] = "media"
                })
            .Build();
        var secretProvider = new ConfigurationSecretProvider(configuration);
        var databaseProvider = new SqlServerDatabaseProvider(configuration, secretProvider);
        var mediaResolver = new MediaStorageProviderResolver(
            configuration,
            secretProvider,
            CreateEnvironment());
        var validator = new ProviderConfigurationValidationService(
            databaseProvider,
            mediaResolver,
            configuration,
            secretProvider);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => validator.StartAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task StartupValidation_RejectsMultipleConnectionsWithoutTopology()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Primary"] = "Server=primary;",
                    ["ConnectionStrings:Reporting"] = "Server=reporting;"
                })
            .Build();
        var secretProvider = new ConfigurationSecretProvider(configuration);
        var databaseProvider = new SqlServerDatabaseProvider(configuration, secretProvider);
        var mediaResolver = new MediaStorageProviderResolver(
            configuration,
            secretProvider,
            CreateEnvironment());
        var validator = new ProviderConfigurationValidationService(
            databaseProvider,
            mediaResolver,
            configuration,
            secretProvider);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => validator.StartAsync(TestContext.Current.CancellationToken));
    }

    private static IWebHostEnvironment CreateEnvironment()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        return new TestWebHostEnvironment(root);
    }

    private sealed class TestWebHostEnvironment(string root) : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "RemoteCommerce.Tests";
        public string WebRootPath { get; set; } = root;
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = root;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}

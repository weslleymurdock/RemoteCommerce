namespace RemoteCommerce.Tests;

public sealed class Stage06ProviderStrategyTests
{
    [Fact]
    public void DatabaseProviderResolver_DefaultsToSqlServer()
    {
        var configuration = BuildConfiguration();
        var provider = new DatabaseProviderResolver(
            configuration,
            new ConfigurationSecretProvider(configuration)).Resolve();

        Assert.Equal("SqlServer", provider.Name);
    }

    [Fact]
    public void DatabaseProviderResolver_RejectsUnknownProvider()
    {
        var configuration = BuildConfiguration(
            new Dictionary<string, string?>
            {
                ["Persistence:Database:Provider"] = "Unknown"
            });

        var resolver = new DatabaseProviderResolver(
            configuration,
            new ConfigurationSecretProvider(configuration));

        Assert.Throws<InvalidOperationException>(() => resolver.Resolve());
    }

    [Fact]
    public void SqlServerProvider_UsesLocalDbWhenNoConnectionStringsExist()
    {
        var configuration = BuildConfiguration();
        var provider = new SqlServerDatabaseProvider(
            configuration,
            new ConfigurationSecretProvider(configuration));

        Assert.Contains("(localdb)\\MSSQLLocalDB", provider.GetConnectionString(DatabaseEndpoint.Primary));
        Assert.Equal(DatabaseTopology.Single, provider.Topology);
    }

    [Fact]
    public void SqlServerProvider_UsesTheOnlyConnectionStringAsPrimary()
    {
        var configuration = BuildConfiguration(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:Store"] = "Server=test;Database=Store;"
            });
        var provider = new SqlServerDatabaseProvider(
            configuration,
            new ConfigurationSecretProvider(configuration));

        Assert.Equal(
            "Server=test;Database=Store;",
            provider.GetConnectionString(DatabaseEndpoint.Primary));
    }

    [Fact]
    public void SqlServerProvider_RejectsMultipleConnectionsWithoutExplicitTopology()
    {
        var configuration = BuildConfiguration(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:Primary"] = "Server=primary;",
                ["ConnectionStrings:Reporting"] = "Server=reporting;"
            });
        var provider = new SqlServerDatabaseProvider(
            configuration,
            new ConfigurationSecretProvider(configuration));

        Assert.Throws<InvalidOperationException>(() => _ = provider.Topology);
    }

    [Fact]
    public void SqlServerProvider_ResolvesExplicitPrimaryReplicaTopology()
    {
        var configuration = BuildConfiguration(
            new Dictionary<string, string?>
            {
                ["Persistence:Database:Topology"] = "PrimaryReplica",
                ["ConnectionStrings:Primary"] = "Server=primary;",
                ["ConnectionStrings:Replica"] = "Server=replica;"
            });
        var provider = new SqlServerDatabaseProvider(
            configuration,
            new ConfigurationSecretProvider(configuration));

        Assert.Equal(DatabaseTopology.PrimaryReplica, provider.Topology);
        Assert.True(provider.RequiresSetup);
        Assert.Equal("Server=primary;", provider.GetConnectionString(DatabaseEndpoint.Primary));
        Assert.Equal("Server=replica;", provider.GetConnectionString(DatabaseEndpoint.Replica));
    }

    [Fact]
    public void SqlServerProvider_RejectsMissingReplicaEndpoint()
    {
        var configuration = BuildConfiguration(
            new Dictionary<string, string?>
            {
                ["Persistence:Database:Topology"] = "PrimaryReplica",
                ["ConnectionStrings:Primary"] = "Server=primary;"
            });
        var provider = new SqlServerDatabaseProvider(
            configuration,
            new ConfigurationSecretProvider(configuration));

        Assert.Throws<InvalidOperationException>(
            () => provider.GetConnectionString(DatabaseEndpoint.Replica));
    }

    [Fact]
    public void SqlServerProvider_RejectsInvalidTopology()
    {
        var configuration = BuildConfiguration(
            new Dictionary<string, string?>
            {
                ["Persistence:Database:Topology"] = "MultiMaster"
            });
        var provider = new SqlServerDatabaseProvider(
            configuration,
            new ConfigurationSecretProvider(configuration));

        Assert.Throws<InvalidOperationException>(() => _ = provider.Topology);
    }

    [Fact]
    public void SqlServerProvider_ResolvesConnectionThroughSecretProvider()
    {
        var configuration = BuildConfiguration(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:Commerce"] = "secret-connection"
            });
        var provider = new SqlServerDatabaseProvider(
            configuration,
            new ConfigurationSecretProvider(configuration));

        Assert.Equal(
            "secret-connection",
            provider.GetConnectionString(DatabaseEndpoint.Primary));
    }

    [Fact]
    public async Task FileSystemMediaStorageProvider_StoresRetrievesAndDeletesThroughAbstraction()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var configuration = BuildConfiguration(
            new Dictionary<string, string?>
            {
                ["Media:FileSystem:RootDirectory"] = root
            });
        var provider = new FileSystemMediaStorageProvider(
            configuration,
            CreateEnvironment(root));

        await using var input = new MemoryStream("media"u8.ToArray());
        var id = await provider.StoreAsync(
            new MediaUpload("image.png", "image/png", input),
            TestContext.Current.CancellationToken);

        await using var media = await provider.RetrieveAsync(
            id,
            TestContext.Current.CancellationToken);
        Assert.NotNull(media);
        Assert.Equal("image.png", media.Descriptor.FileName);
        Assert.Equal("image/png", media.Descriptor.ContentType);

        await provider.DeleteAsync(id, TestContext.Current.CancellationToken);
        Assert.Null(await provider.RetrieveAsync(id, TestContext.Current.CancellationToken));

        Directory.Delete(root, true);
    }

    [Fact]
    public async Task FileSystemMediaStorageProvider_RejectsPathTraversal()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var provider = new FileSystemMediaStorageProvider(
            BuildConfiguration(),
            CreateEnvironment(root));
        await using var input = new MemoryStream([1, 2, 3]);

        await Assert.ThrowsAsync<ArgumentException>(
            () => provider.StoreAsync(
                new MediaUpload("../secret.txt", "text/plain", input),
                TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task FileSystemMediaStorageProvider_RejectsArbitraryIdentifier()
    {
        var provider = new FileSystemMediaStorageProvider(
            BuildConfiguration(),
            CreateEnvironment(Path.GetTempPath()));

        await Assert.ThrowsAsync<ArgumentException>(
            () => provider.RetrieveAsync("../../secret", TestContext.Current.CancellationToken));
    }

    [Fact]
    public void MediaStorageProviderResolver_SelectsMongoGridFsWithoutConnecting()
    {
        var configuration = BuildConfiguration(
            new Dictionary<string, string?>
            {
                ["Media:Provider"] = "MongoGridFS",
                ["Media:MongoGridFs:DatabaseName"] = "RemoteCommerceMedia",
                ["Media:MongoGridFs:BucketName"] = "media"
            });
        var resolver = new MediaStorageProviderResolver(
            configuration,
            new ConfigurationSecretProvider(configuration),
            CreateEnvironment(Path.GetTempPath()));

        var provider = resolver.Resolve();

        Assert.Equal("MongoGridFS", provider.Name);
        Assert.IsType<MongoGridFsMediaStorageProvider>(provider);
    }

    [Fact]
    public async Task MongoGridFsProvider_ReportsMissingConnectionWithoutExternalDependency()
    {
        var configuration = BuildConfiguration(
            new Dictionary<string, string?>
            {
                ["Media:MongoGridFs:DatabaseName"] = "RemoteCommerceMedia",
                ["Media:MongoGridFs:BucketName"] = "media"
            });
        var provider = new MongoGridFsMediaStorageProvider(
            configuration,
            new ConfigurationSecretProvider(configuration));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => provider.ValidateAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task DatabaseSetupService_BlocksUntilReplicationSetupSucceeds()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var configuration = BuildConfiguration(
            new Dictionary<string, string?>
            {
                ["DatabaseSetup:StateFile"] = Path.Combine(root, "state.json")
            });
        var provider = new FakeDatabaseProvider(DatabaseTopology.PrimaryReplica);
        var replication = new FakeReplicationProvider();
        var stateStore = new DatabaseSetupStateStore(configuration, CreateEnvironment(root));
        var service = new DatabaseSetupService(
            provider,
            replication,
            stateStore,
            NullLogger<DatabaseSetupService>.Instance);

        Assert.Equal(
            DatabaseSetupState.Required,
            await service.GetStateAsync(TestContext.Current.CancellationToken));

        await service.ConfigureAsync(TestContext.Current.CancellationToken);

        Assert.Equal(
            DatabaseSetupState.Configured,
            await service.GetStateAsync(TestContext.Current.CancellationToken));
        Assert.True(replication.Initialized);

        Directory.Delete(root, true);
    }

    [Fact]
    public async Task DatabaseSetupService_PersistsFailureAndAllowsRetry()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var configuration = BuildConfiguration(
            new Dictionary<string, string?>
            {
                ["DatabaseSetup:StateFile"] = Path.Combine(root, "state.json")
            });
        var provider = new FakeDatabaseProvider(DatabaseTopology.PrimaryReplica);
        var replication = new FakeReplicationProvider { FailValidation = true };
        var stateStore = new DatabaseSetupStateStore(configuration, CreateEnvironment(root));
        var service = new DatabaseSetupService(
            provider,
            replication,
            stateStore,
            NullLogger<DatabaseSetupService>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ConfigureAsync(TestContext.Current.CancellationToken));
        Assert.Equal(
            DatabaseSetupState.Required,
            await service.GetStateAsync(TestContext.Current.CancellationToken));

        replication.FailValidation = false;
        await service.ConfigureAsync(TestContext.Current.CancellationToken);

        Assert.Equal(
            DatabaseSetupState.Configured,
            await service.GetStateAsync(TestContext.Current.CancellationToken));

        Directory.Delete(root, true);
    }

    private static IConfiguration BuildConfiguration(
        IReadOnlyDictionary<string, string?>? values = null)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
        return configuration;
    }

    private static IWebHostEnvironment CreateEnvironment(string root)
    {
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

    private sealed class FakeDatabaseProvider(DatabaseTopology topology) : IDatabaseProvider
    {
        public string Name => "Fake";
        public DatabaseTopology Topology => topology;
        public bool RequiresSetup => topology == DatabaseTopology.PrimaryReplica;

        public string GetConnectionString(DatabaseEndpoint endpoint) => $"fake-{endpoint}";

        public Task ValidateAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeReplicationProvider : IDatabaseReplicationProvider
    {
        public string ProviderName => "Fake";
        public bool FailValidation { get; set; }
        public bool Initialized { get; private set; }

        public Task ValidateAsync(CancellationToken cancellationToken = default)
        {
            if (FailValidation)
            {
                throw new InvalidOperationException("validation failed");
            }

            return Task.CompletedTask;
        }

        public Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            Initialized = true;
            return Task.CompletedTask;
        }
    }
}

namespace RemoteCommerce.Tests;

public sealed class Stage07PluginPersistenceTests
{
    [Fact]
    public void ManifestWithoutEfCompatibilityRemainsValid()
    {
        var validator = new PluginManifestValidator();
        var issues = validator.Validate(CreateManifest());

        Assert.DoesNotContain(issues, issue => issue.Code.StartsWith("EF_", StringComparison.Ordinal));
    }

    [Fact]
    public void ManifestWithCompatibleEfVersionPassesCompatibilityValidation()
    {
        var hostVersion = typeof(DbContext).Assembly.GetName().Version!;
        var manifest = CreateManifest(hostVersion.Major + "." + hostVersion.Minor + ".0");
        var issues = new PluginCompatibilityValidator().Validate(manifest);

        Assert.DoesNotContain(issues, issue => issue.Code == "EF_VERSION_INCOMPATIBLE");
    }

    [Fact]
    public void ManifestWithIncompatibleEfVersionIsRejected()
    {
        var hostVersion = typeof(DbContext).Assembly.GetName().Version!;
        var incompatible = hostVersion.Major + "." + (hostVersion.Minor + 1) + ".0";
        var manifest = CreateManifest(incompatible);
        var issues = new PluginCompatibilityValidator().Validate(manifest);

        Assert.Contains(issues, issue => issue.Code == "EF_VERSION_INCOMPATIBLE");
    }

    [Fact]
    public void ManifestWithInvalidEfMetadataIsRejected()
    {
        var manifest = CreateManifest("not-a-version");
        var issues = new PluginManifestValidator().Validate(manifest);

        Assert.Contains(issues, issue => issue.Code == "EF_VERSION_INVALID");
    }

    [Fact]
    public void PluginPersistenceSchemaUsesStablePluginIdentifier()
    {
        Assert.Equal(
            "rc_plugin_remotecommerce_sample",
            PluginPersistenceBuilder.GetDefaultSchema("RemoteCommerce.Sample"));
    }

    [Fact]
    public void PluginPersistenceBuilderRejectsForeignSchema()
    {
        var services = new ServiceCollection();
        var builder = new PluginPersistenceBuilder(services, "remotecommerce-sample");

        Assert.Throws<ArgumentException>(() => builder.AddDbContext(
            typeof(TestPluginDbContext),
            schema: "commerce"));
    }

    [Fact]
    public void PluginPersistenceBuilderRegistersContextAndDescriptor()
    {
        var services = new ServiceCollection();
        var builder = new PluginPersistenceBuilder(services, "remotecommerce-sample");

        builder.AddDbContext(typeof(TestPluginDbContext));

        var descriptor = Assert.Single(builder.GetDescriptors());
        Assert.Equal("remotecommerce-sample", descriptor.PluginId);
        Assert.Equal(typeof(TestPluginDbContext), descriptor.DbContextType);
        Assert.Equal("rc_plugin_remotecommerce_sample", descriptor.Schema);
        Assert.Contains(services, service => service.ServiceType == typeof(PluginPersistenceDescriptor));
    }

    [Fact]
    public void PluginPersistenceBuilderRejectsDuplicateContextRegistration()
    {
        var services = new ServiceCollection();
        var builder = new PluginPersistenceBuilder(services, "remotecommerce-sample");
        builder.AddDbContext(typeof(TestPluginDbContext));

        Assert.Throws<InvalidOperationException>(() => builder.AddDbContext(typeof(TestPluginDbContext)));
    }

    private static PluginManifest CreateManifest(string? efCoreVersion = null)
        => new(
            "remotecommerce-sample",
            "Sample",
            "LICENSE.md",
            "README.md",
            "1.0.0",
            "lib/net10.0/RemoteCommerce_Sample.dll",
            "RemoteCommerce.Sample.PluginEntry",
            "1.0.0",
            "Sample plugin",
            "RemoteCommerce_Sample",
            "sample",
            "Sample",
            "Tests",
            "RemoteCommerce",
            "https://example.test",
            "git",
            false,
            "https://example.test",
            EfCoreVersion: efCoreVersion);

    private sealed class TestPluginDbContext(DbContextOptions<TestPluginDbContext> options) : DbContext(options);
}

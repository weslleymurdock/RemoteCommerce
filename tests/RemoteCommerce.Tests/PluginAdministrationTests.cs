namespace RemoteCommerce.Tests;

public sealed class PluginAdministrationTests
{
    [Fact]
    public async Task PackageValidation_RejectsMissingReadme()
    {
        var path = await CreatePackageAsync(includeReadme: false, includeLicense: true);
        try { var result = await CreatePackageValidator().ValidateAsync(path, CancellationToken.None); Assert.Contains(result.Issues, x => x.Code == "README_MISSING"); Assert.False(result.IsValid); } finally { System.IO.File.Delete(path); }
    }
    [Fact]
    public async Task PackageValidation_RejectsMissingLicense()
    {
        var path = await CreatePackageAsync(includeReadme: true, includeLicense: false);
        try { var result = await CreatePackageValidator().ValidateAsync(path, CancellationToken.None); Assert.Contains(result.Issues, x => x.Code == "LICENSE_MISSING"); Assert.False(result.IsValid); } finally { System.IO.File.Delete(path); }
    }
    [Fact]
    public async Task PackageValidation_RejectsInvalidEntrypoint()
    {
        var path = await CreatePackageAsync(entryType: "InvalidType");
        try { var result = await CreatePackageValidator().ValidateAsync(path, CancellationToken.None); Assert.Contains(result.Issues, x => x.Code == "ENTRY_TYPE_INVALID"); Assert.False(result.IsValid); } finally { System.IO.File.Delete(path); }
    }
    [Fact]
    public async Task PackageValidation_RejectsIncompatibleFramework()
    {
        var path = await CreatePackageAsync(entryAssembly: "lib/net9.0/TestPlugin.dll");
        try { var result = await CreatePackageValidator().ValidateAsync(path, CancellationToken.None); Assert.Contains(result.Issues, x => x.Code == "ENTRY_ASSEMBLY_TARGET_INVALID"); Assert.False(result.IsValid); } finally { System.IO.File.Delete(path); }
    }
    [Fact]
    public async Task PackageValidation_RejectsIncompatibleRemoteCommerceVersion()
    {
        var path = await CreatePackageAsync(minHostVersion: "999.0.0");
        try { var result = await CreatePackageValidator().ValidateAsync(path, CancellationToken.None); Assert.Contains(result.Issues, x => x.Code == "HOST_VERSION_INCOMPATIBLE"); Assert.False(result.IsValid); } finally { System.IO.File.Delete(path); }
    }
    [Fact]
    public async Task PackageValidation_ProducesIntegrityHash()
    {
        var path = await CreatePackageAsync();
        try { var result = await CreatePackageValidator().ValidateAsync(path, CancellationToken.None); Assert.True(result.IsValid); Assert.Equal(64, result.PackageHash.Length); Assert.NotNull(result.Manifest); } finally { System.IO.File.Delete(path); }
    }
    [Fact]
    public async Task DependencyValidation_RejectsMissingDependency()
    {
        var options = CreateOptions(); await using var db = CreateDb(options); var validator = new PluginDependencyValidator(new TestDbContextFactory(options));
        var issues = await validator.ValidateAsync(CreateManifest(dependencies: [new PluginDependencyDeclaration("missing", "1.0.0")]), CancellationToken.None); Assert.Contains(issues, x => x.Code == "DEPENDENCY_MISSING");
    }
    [Fact]
    public async Task DependencyValidation_RejectsIncompatibleVersion()
    {
        var options = CreateOptions(); await using var db = CreateDb(options); db.PluginInstallations.Add(CreateInstallation("dependency", "1.0.0", PluginInstallationState.Loaded)); await db.SaveChangesAsync(CancellationToken.None);
        var validator = new PluginDependencyValidator(new TestDbContextFactory(options)); var issues = await validator.ValidateAsync(CreateManifest(dependencies: [new PluginDependencyDeclaration("dependency", "2.0.0")]), CancellationToken.None); Assert.Contains(issues, x => x.Code == "DEPENDENCY_INCOMPATIBLE");
    }
    [Fact]
    public async Task DependencyValidation_RejectsDisabledDependency()
    {
        var options = CreateOptions(); await using var db = CreateDb(options); db.PluginInstallations.Add(CreateInstallation("dependency", "1.0.0", PluginInstallationState.Disabled, PluginDesiredState.Disabled)); await db.SaveChangesAsync(CancellationToken.None);
        var validator = new PluginDependencyValidator(new TestDbContextFactory(options)); var issues = await validator.ValidateAsync(CreateManifest(dependencies: [new PluginDependencyDeclaration("dependency", "1.0.0")]), CancellationToken.None); Assert.Contains(issues, x => x.Code == "DEPENDENCY_DISABLED");
    }
    [Fact]
    public async Task DependencyValidation_RejectsCircularDependency()
    {
        var options = CreateOptions(); await using var db = CreateDb(options); db.PluginInstallations.AddRange(CreateInstallation("plugin-a", "1.0.0", PluginInstallationState.Loaded), CreateInstallation("plugin-b", "1.0.0", PluginInstallationState.Loaded)); db.PluginDependencies.AddRange(new PluginDependency { Id = Guid.NewGuid(), PluginId = "plugin-a", DependencyPluginId = "plugin-b", MinimumVersion = "1.0.0" }, new PluginDependency { Id = Guid.NewGuid(), PluginId = "plugin-b", DependencyPluginId = "plugin-a", MinimumVersion = "1.0.0" }); await db.SaveChangesAsync(CancellationToken.None);
        var validator = new PluginDependencyValidator(new TestDbContextFactory(options)); var issues = await validator.ValidateAsync(CreateManifest(id: "plugin-a", dependencies: [new PluginDependencyDeclaration("plugin-b", "1.0.0")]), CancellationToken.None); Assert.Contains(issues, x => x.Code == "DEPENDENCY_CYCLE");
    }
    [Fact]
    public async Task Management_DisableAndEnablePersistPendingState()
    {
        var options = CreateOptions(); 
        await using var db = CreateDb(options); 
        db.PluginInstallations.Add(CreateInstallation("plugin", "1.0.0", PluginInstallationState.Loaded)); 
        await db.SaveChangesAsync(CancellationToken.None); 
        var restart = new ApplicationRestartService(); 
        var management = new PluginManagementService(new TestDbContextFactory(options), CreateDb(options), restart);
        await management.DisableAsync("plugin", CancellationToken.None); await using (var verification = CreateDb(options)) { var disabled = await verification.PluginInstallations.SingleAsync(CancellationToken.None); Assert.Equal(PluginDesiredState.Disabled, disabled.DesiredState); Assert.Equal(PluginInstallationState.ActivationPending, disabled.State); } Assert.True(restart.Status.Required);
        await management.EnableAsync("plugin", CancellationToken.None); await using var finalVerification = CreateDb(options); var enabled = await finalVerification.PluginInstallations.SingleAsync(CancellationToken.None); Assert.Equal(PluginDesiredState.Enabled, enabled.DesiredState); Assert.Equal(PluginInstallationState.ActivationPending, enabled.State);
    }
    [Fact]
    public async Task Management_UninstallProtectsRequiredDependency()
    {
        var options = CreateOptions(); await using var db = CreateDb(options); db.PluginInstallations.AddRange(CreateInstallation("base", "1.0.0", PluginInstallationState.Loaded), CreateInstallation("dependent", "1.0.0", PluginInstallationState.Loaded)); db.PluginDependencies.Add(new PluginDependency { Id = Guid.NewGuid(), PluginId = "dependent", DependencyPluginId = "base", MinimumVersion = "1.0.0" }); await db.SaveChangesAsync(CancellationToken.None); var management = new PluginManagementService(new TestDbContextFactory(options), CreateDb(options), new ApplicationRestartService());
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => management.UninstallAsync("base", CancellationToken.None)); Assert.Contains("dependent", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
    [Fact]
    public async Task Management_RollbackSchedulesRetainedVersion()
    {
        var options = CreateOptions(); await using var db = CreateDb(options); db.PluginInstallations.Add(CreateInstallation("plugin", "2.0.0", PluginInstallationState.Loaded)); db.PluginVersions.AddRange(new PluginVersion { Id = Guid.NewGuid(), PluginId = "plugin", Version = "1.0.0", PackagePath = "old", PackageHash = "old", InstalledAt = DateTimeOffset.UtcNow.AddDays(-1), IsCurrent = false }, new PluginVersion { Id = Guid.NewGuid(), PluginId = "plugin", Version = "2.0.0", PackagePath = "new", PackageHash = "new", InstalledAt = DateTimeOffset.UtcNow, IsCurrent = true }); await db.SaveChangesAsync(CancellationToken.None); var management = new PluginManagementService(new TestDbContextFactory(options), CreateDb(options), new ApplicationRestartService());
        await management.RollbackAsync("plugin", "1.0.0", CancellationToken.None); await using var verification = CreateDb(options); var installation = await verification.PluginInstallations.SingleAsync(CancellationToken.None); Assert.Equal("1.0.0", installation.Version); Assert.Equal(PluginInstallationState.ActivationPending, installation.State); Assert.Equal("old", installation.PackagePath);
    }

    private static CommerceDbContext CreateDb(DbContextOptions<CommerceDbContext> options) => new(options, new TestApplicationContext());
    private static PluginPackageValidator CreatePackageValidator() => new(new PluginManifestValidator(), new PluginCompatibilityValidator());
    private static DbContextOptions<CommerceDbContext> CreateOptions() => new DbContextOptionsBuilder<CommerceDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
    private static PluginInstallation CreateInstallation(string id, string version, PluginInstallationState state, PluginDesiredState desired = PluginDesiredState.Enabled) => new() { Id = Guid.NewGuid(), PluginId = id, Version = version, PackagePath = Path.Combine(Path.GetTempPath(), id), PackageHash = "hash", State = state, DesiredState = desired, InstalledAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow };
    private static PluginManifest CreateManifest(string id = "candidate", IReadOnlyList<PluginDependencyDeclaration>? dependencies = null) => new(id, "Candidate", "LICENSE.md", "README.md", "1.0.0", "lib/net10.0/TestPlugin.dll", "Tests.TestPlugin", "1.0.0", "Candidate plugin", "TestPlugin", "test", "Candidate", "Tests", "Tests", "https://example.test", "git", false, "https://example.test", dependencies);
    private static async Task<string> CreatePackageAsync(bool includeReadme = true, bool includeLicense = true, string entryAssembly = "lib/net10.0/TestPlugin.dll", string entryType = "Tests.TestPlugin", string minHostVersion = "1.0.0")
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.nupkg"); using var archive = ZipFile.Open(path, ZipArchiveMode.Create); var manifest = CreateManifest() with { EntryAssembly = entryAssembly, EntryType = entryType, MinHostVersion = minHostVersion }; await WriteEntryAsync(archive, "plugin.manifest.json", JsonSerializer.Serialize(manifest)); if (includeReadme) await WriteEntryAsync(archive, "README.md", "# Test plugin"); if (includeLicense) await WriteEntryAsync(archive, "LICENSE.md", "MIT"); var entry = archive.CreateEntry(entryAssembly.Replace('\\', '/')); await using var output = entry.Open(); await using var input = System.IO.File.OpenRead(typeof(PluginAdministrationTests).Assembly.Location); await input.CopyToAsync(output); return path;
    }
    private static async Task WriteEntryAsync(ZipArchive archive, string path, string content) { await using var writer = new StreamWriter(archive.CreateEntry(path).Open()); await writer.WriteAsync(content); }
    private sealed class TestDbContextFactory(DbContextOptions<CommerceDbContext> options) : IDbContextFactory<CommerceDbContext>
    {
        public CommerceDbContext CreateDbContext() => CreateDb(options); public Task<CommerceDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) => Task.FromResult(CreateDb(options));
    }

    private sealed class TestApplicationContext : IApplicationContext
    {
        public Guid? UserId => null; public string Actor => "test"; public string CorrelationId => "test"; public string? IpAddress => null;
    }
}

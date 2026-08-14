namespace RemoteCommerce.Infrastructure.Persistence;

/// <summary>Creates <see cref="CommerceDbContext"/> instances for EF Core design-time operations.</summary>
public sealed class CommerceDbContextDesignTimeFactory : IDesignTimeDbContextFactory<CommerceDbContext>
{
    /// <summary>Creates a context using the configured database provider strategy.</summary>
    /// <param name="args">Optional EF Core design-time arguments.</param>
    /// <returns>A configured commerce database context.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the configured provider cannot be resolved.</exception>
    public CommerceDbContext CreateDbContext(string[] args)
    {
        var root = Directory.GetCurrentDirectory();
        var projectRoot = File.Exists(Path.Combine(root, "appsettings.json"))
            ? root
            : Path.Combine(root, "src", "RemoteCommerce");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(projectRoot)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var secretProvider = new ConfigurationSecretProvider(configuration);
        var databaseProvider = new DatabaseProviderResolver(configuration, secretProvider).Resolve();
        var connectionString = databaseProvider.GetConnectionString(DatabaseEndpoint.Primary);

        var options = new DbContextOptionsBuilder<CommerceDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new CommerceDbContext(options, new DesignTimeApplicationContext());
    }

    private sealed class DesignTimeApplicationContext : IApplicationContext
    {
        public Guid? UserId => null;
        public string Actor => "design-time";
        public string CorrelationId => "design-time";
        public string? IpAddress => null;
    }
}

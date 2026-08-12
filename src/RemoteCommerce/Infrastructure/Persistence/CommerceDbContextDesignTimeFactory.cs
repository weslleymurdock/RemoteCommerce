using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace RemoteCommerce.Infrastructure.Persistence;

/// <summary>Creates <see cref="CommerceDbContext"/> instances for EF Core design-time operations.</summary>
public sealed class CommerceDbContextDesignTimeFactory : IDesignTimeDbContextFactory<CommerceDbContext>
{
    /// <summary>Creates a database context using the repository application configuration.</summary>
    /// <param name="args">Optional EF Core design-time arguments.</param>
    /// <returns>A configured commerce database context.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the Commerce connection string is missing.</exception>
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

        var connectionString = configuration.GetConnectionString("Commerce");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("The Commerce connection string is required for EF Core design-time operations.");
        }

        var options = new DbContextOptionsBuilder<CommerceDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new CommerceDbContext(options);
    }
}

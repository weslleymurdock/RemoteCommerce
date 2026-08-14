namespace RemoteCommerce.Infrastructure.Persistence.Services;

/// <summary>Selects the configured relational database provider.</summary>
internal sealed class DatabaseProviderResolver(
    IConfiguration configuration,
    ISecretProvider secretProvider)
{
    /// <summary>Creates the configured relational provider.</summary>
    /// <returns>The configured database provider implementation.</returns>
    public IDatabaseProvider Resolve()
    {
        var provider = configuration["Persistence:Database:Provider"];
        provider = string.IsNullOrWhiteSpace(provider) ? "SqlServer" : provider;

        if (string.Equals(provider, "SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            return new SqlServerDatabaseProvider(configuration, secretProvider);
        }

        throw new InvalidOperationException(
            $"Unsupported database provider '{provider}'. Supported providers are SqlServer.");
    }
}

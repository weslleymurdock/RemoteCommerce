namespace RemoteCommerce.Plugins;

/// <summary>Collects plugin persistence registrations before the host service provider is built.</summary>
/// <param name="services">The host service collection.</param>
/// <param name="pluginId">The stable plugin identifier.</param>
public sealed class PluginPersistenceBuilder(IServiceCollection services, string pluginId) : IPluginPersistenceBuilder
{
    /// <summary>Registers one plugin-owned EF Core DbContext.</summary>
    /// <param name="dbContextType">The plugin-owned DbContext type.</param>
    /// <param name="migrationsAssembly">The assembly containing plugin migrations.</param>
    /// <param name="schema">The relational schema name.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dbContextType"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the context type or schema is invalid.</exception>
    public void AddDbContext(Type dbContextType, string? migrationsAssembly = null, string? schema = null)
    {
        ArgumentNullException.ThrowIfNull(dbContextType);

        if (!typeof(DbContext).IsAssignableFrom(dbContextType))
        {
            throw new ArgumentException("The plugin persistence type must derive from DbContext.", nameof(dbContextType));
        }

        var expectedSchema = GetDefaultSchema(pluginId);
        var resolvedSchema = string.IsNullOrWhiteSpace(schema) ? expectedSchema : schema.Trim();
        if (!string.Equals(resolvedSchema, expectedSchema, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"Plugin persistence schema must be '{expectedSchema}'.",
                nameof(schema));
        }

        var descriptor = new PluginPersistenceDescriptor(
            pluginId,
            dbContextType,
            migrationsAssembly,
            resolvedSchema);

        services.AddSingleton(descriptor);
        services.AddDbContext(dbContextType, (serviceProvider, options) =>
        {
            var provider = serviceProvider.GetRequiredService<IDatabaseProvider>();
            provider.ConfigureDbContext(options, migrationsAssembly);
        });
    }

    /// <summary>Builds the deterministic relational schema name for a plugin identifier.</summary>
    /// <param name="pluginId">The stable plugin identifier.</param>
    /// <returns>The provider-compatible schema name.</returns>
    public static string GetDefaultSchema(string pluginId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pluginId);
        var normalized = new string(pluginId
            .Trim()
            .ToLowerInvariant()
            .Select(character => char.IsLetterOrDigit(character) || character == '_' ? character : '_')
            .ToArray());

        return $"rc_plugin_{normalized}";
    }
}

/// <summary>Describes one plugin-owned persistence context registered with the host.</summary>
/// <param name="PluginId">The stable plugin identifier.</param>
/// <param name="DbContextType">The plugin-owned DbContext type.</param>
/// <param name="MigrationsAssembly">The assembly containing plugin migrations.</param>
/// <param name="Schema">The deterministic relational schema.</param>
public sealed record PluginPersistenceDescriptor(
    string PluginId,
    Type DbContextType,
    string? MigrationsAssembly,
    string Schema);

namespace RemoteCommerce.Plugins.Abstractions;

/// <summary>Defines the persistence capabilities requested by a plugin during startup.</summary>
public interface IRemoteCommercePluginPersistence
{
    /// <summary>Registers the plugin-owned persistence context and migration metadata.</summary>
    /// <param name="builder">The provider-independent persistence registration builder.</param>
    /// <remarks>The host invokes this contract before the dependency injection container is built.</remarks>
    void ConfigurePersistence(IPluginPersistenceBuilder builder);
}

/// <summary>Registers plugin-owned EF Core persistence without exposing host database implementation details.</summary>
public interface IPluginPersistenceBuilder
{
    /// <summary>Registers one plugin-owned DbContext for the current store database.</summary>
    /// <param name="dbContextType">The plugin-owned type deriving from <c>DbContext</c>.</param>
    /// <param name="migrationsAssembly">The assembly containing migrations, or <see langword="null"/> to use the DbContext assembly.</param>
    /// <param name="schema">The deterministic relational schema name requested by the plugin.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dbContextType"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when the type is not a DbContext or the schema is invalid.</exception>
    void AddDbContext(Type dbContextType, string? migrationsAssembly = null, string? schema = null);
}

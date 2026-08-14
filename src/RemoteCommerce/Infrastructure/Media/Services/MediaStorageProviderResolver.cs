namespace RemoteCommerce.Infrastructure.Media.Services;

/// <summary>Selects the configured media storage provider.</summary>
/// <param name="configuration">The deployment configuration source.</param>
/// <param name="secretProvider">The deployment secret boundary.</param>
/// <param name="environment">The host environment used by filesystem storage.</param>
public sealed class MediaStorageProviderResolver(
    IConfiguration configuration,
    ISecretProvider secretProvider,
    IWebHostEnvironment environment)
{
    /// <summary>Creates the configured media storage provider.</summary>
    /// <returns>The configured media provider implementation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the provider is not supported.</exception>
    public IMediaStorageProvider Resolve()
    {
        var provider = configuration["Media:Provider"];
        provider = string.IsNullOrWhiteSpace(provider) ? "FileSystem" : provider;

        if (string.Equals(provider, "FileSystem", StringComparison.OrdinalIgnoreCase))
        {
            return new FileSystemMediaStorageProvider(configuration, environment);
        }

        if (string.Equals(provider, "MongoGridFS", StringComparison.OrdinalIgnoreCase))
        {
            return new MongoGridFsMediaStorageProvider(configuration, secretProvider);
        }

        throw new InvalidOperationException(
            $"Unsupported media storage provider '{provider}'. Supported providers are FileSystem and MongoGridFS.");
    }
}

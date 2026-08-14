namespace RemoteCommerce.Infrastructure.Persistence.Services;

/// <summary>Validates selected database and media provider configuration during host startup.</summary>
/// <param name="databaseProvider">The selected database provider.</param>
/// <param name="mediaResolver">The media provider resolver.</param>
/// <param name="configuration">The deployment configuration source.</param>
/// <param name="secretProvider">The deployment secret boundary.</param>
public sealed class ProviderConfigurationValidationService(
    RemoteCommerce.Application.Persistence.Abstractions.IDatabaseProvider databaseProvider,
    MediaStorageProviderResolver mediaResolver,
    IConfiguration configuration,
    ISecretProvider secretProvider) : IHostedService
{
    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = databaseProvider.GetConnectionString(DatabaseEndpoint.Primary);

        if (databaseProvider.Topology == DatabaseTopology.PrimaryReplica)
        {
            _ = databaseProvider.GetConnectionString(DatabaseEndpoint.Replica);
        }

        var mediaProvider = mediaResolver.Resolve();
        if (string.Equals(mediaProvider.Name, "MongoGridFS", StringComparison.OrdinalIgnoreCase))
        {
            ValidateMongoConfiguration();
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

    private void ValidateMongoConfiguration()
    {
        if (string.IsNullOrWhiteSpace(
                secretProvider.Get("Media:MongoGridFs:ConnectionString")))
        {
            throw new InvalidOperationException(
                "Media:MongoGridFs:ConnectionString is required when MongoGridFS is selected.");
        }

        if (string.IsNullOrWhiteSpace(configuration["Media:MongoGridFs:DatabaseName"]))
        {
            throw new InvalidOperationException(
                "Media:MongoGridFs:DatabaseName is required when MongoGridFS is selected.");
        }

        if (string.IsNullOrWhiteSpace(configuration["Media:MongoGridFs:BucketName"]))
        {
            throw new InvalidOperationException(
                "Media:MongoGridFs:BucketName is required when MongoGridFS is selected.");
        }
    }
}

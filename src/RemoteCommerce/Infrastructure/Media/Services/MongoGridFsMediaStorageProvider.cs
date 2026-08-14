namespace RemoteCommerce.Infrastructure.Media.Services;

/// <summary>Stores media objects in a MongoDB GridFS bucket without using MongoDB as the transactional store.</summary>
/// <param name="configuration">The deployment configuration source.</param>
/// <param name="secretProvider">The deployment secret boundary used to resolve the MongoDB connection string.</param>
public sealed class MongoGridFsMediaStorageProvider(
    IConfiguration configuration,
    ISecretProvider secretProvider) : IMediaStorageProvider
{
    /// <inheritdoc />
    public string Name => "MongoGridFS";

    /// <inheritdoc />
    public async Task<string> StoreAsync(
        MediaUpload upload,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(upload);
        ValidateFileName(upload.FileName);

        var bucket = GetBucket();
        var options = new GridFSUploadOptions
        {
            Metadata = new BsonDocument
            {
                { "fileName", Path.GetFileName(upload.FileName) },
                { "contentType", upload.ContentType }
            }
        };

        var id = await bucket.UploadFromStreamAsync(
            Path.GetFileName(upload.FileName),
            upload.Content,
            options,
            cancellationToken);

        return id.ToString();
    }

    /// <inheritdoc />
    public async Task<MediaObject?> RetrieveAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        if (!ObjectId.TryParse(id, out var objectId))
        {
            throw new ArgumentException("The media identifier is invalid.", nameof(id));
        }

        var bucket = GetBucket();
        var cursor = await bucket.FindAsync(
            Builders<GridFSFileInfo>.Filter.Eq(x => x.Id, objectId),
            cancellationToken: cancellationToken);
        var file = await cursor.FirstOrDefaultAsync(cancellationToken);

        if (file is null)
        {
            return null;
        }

        var content = await bucket.OpenDownloadStreamAsync(objectId, cancellationToken: cancellationToken);
        var fileName = file.Metadata?.GetValue("fileName", file.Filename).AsString ?? file.Filename;
        var contentType = file.Metadata?.GetValue("contentType", "application/octet-stream").AsString
            ?? "application/octet-stream";
        var createdAt = file.UploadDateTime.ToUniversalTime();

        return new MediaObject(
            new MediaObjectDescriptor(
                id,
                fileName,
                contentType,
                file.Length,
                new DateTimeOffset(createdAt, TimeSpan.Zero)),
            content);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        if (!ObjectId.TryParse(id, out var objectId))
        {
            throw new ArgumentException("The media identifier is invalid.", nameof(id));
        }

        await GetBucket().DeleteAsync(objectId, cancellationToken);
    }

    /// <summary>Validates MongoDB connectivity and the selected GridFS configuration.</summary>
    /// <param name="cancellationToken">The token used to cancel validation.</param>
    /// <returns>A task representing the validation operation.</returns>
    public async Task ValidateAsync(CancellationToken cancellationToken = default)
    {
        var client = GetClient();
        await client.GetDatabase(GetDatabaseName())
            .RunCommandAsync<BsonDocument>(
                new BsonDocument("ping", 1),
                cancellationToken: cancellationToken);
        _ = GetBucket();
    }

    private MongoClient GetClient()
    {
        var connectionString = secretProvider.Get("Media:MongoGridFs:ConnectionString");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Media:MongoGridFs:ConnectionString is required when MongoGridFS is selected.");
        }

        return new MongoClient(connectionString);
    }

    private GridFSBucket GetBucket()
    {
        var databaseName = GetDatabaseName();
        var bucketName = configuration["Media:MongoGridFs:BucketName"];
        if (string.IsNullOrWhiteSpace(bucketName))
        {
            throw new InvalidOperationException(
                "Media:MongoGridFs:BucketName is required when MongoGridFS is selected.");
        }

        return new GridFSBucket(
            GetClient().GetDatabase(databaseName),
            new GridFSBucketOptions { BucketName = bucketName });
    }

    private string GetDatabaseName()
    {
        var databaseName = configuration["Media:MongoGridFs:DatabaseName"];
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new InvalidOperationException(
                "Media:MongoGridFs:DatabaseName is required when MongoGridFS is selected.");
        }

        return databaseName;
    }

    private static void ValidateFileName(string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        if (!string.Equals(fileName, Path.GetFileName(fileName), StringComparison.Ordinal))
        {
            throw new ArgumentException("A media file name must not contain directory segments.", nameof(fileName));
        }
    }
}

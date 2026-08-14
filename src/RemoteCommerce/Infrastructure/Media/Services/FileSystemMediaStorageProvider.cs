namespace RemoteCommerce.Infrastructure.Media.Services;

/// <summary>Stores media objects below a configured application-owned filesystem root.</summary>
/// <param name="configuration">The deployment configuration source.</param>
/// <param name="environment">The host environment used to resolve relative paths.</param>
public sealed class FileSystemMediaStorageProvider(
    IConfiguration configuration,
    IWebHostEnvironment environment) : IMediaStorageProvider
{
    /// <inheritdoc />
    public string Name => "FileSystem";

    /// <inheritdoc />
    public async Task<string> StoreAsync(
        MediaUpload upload,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(upload);
        ValidateFileName(upload.FileName);

        var id = Guid.NewGuid().ToString("N");
        var root = GetRootDirectory();
        Directory.CreateDirectory(root);

        var dataPath = GetDataPath(root, id);
        var metadataPath = GetMetadataPath(root, id);

        await using (var destination = new FileStream(
                         dataPath,
                         FileMode.CreateNew,
                         FileAccess.Write,
                         FileShare.None,
                         64 * 1024,
                         FileOptions.Asynchronous | FileOptions.SequentialScan))
        {
            await upload.Content.CopyToAsync(destination, cancellationToken);
        }

        var metadata = new FileSystemMediaMetadata(
            id,
            Path.GetFileName(upload.FileName),
            upload.ContentType,
            new FileInfo(dataPath).Length,
            DateTimeOffset.UtcNow);

        await File.WriteAllTextAsync(
            metadataPath,
            JsonSerializer.Serialize(metadata),
            cancellationToken);

        return id;
    }

    /// <inheritdoc />
    public async Task<MediaObject?> RetrieveAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        ValidateId(id);

        var root = GetRootDirectory();
        var metadataPath = GetMetadataPath(root, id);
        var dataPath = GetDataPath(root, id);

        if (!File.Exists(metadataPath) || !File.Exists(dataPath))
        {
            return null;
        }

        var metadata = JsonSerializer.Deserialize<FileSystemMediaMetadata>(
            await File.ReadAllTextAsync(metadataPath, cancellationToken));
        if (metadata is null)
        {
            throw new InvalidDataException("The media metadata is invalid.");
        }

        var stream = new FileStream(
            dataPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            64 * 1024,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        return new MediaObject(
            new MediaObjectDescriptor(
                metadata.Id,
                metadata.FileName,
                metadata.ContentType,
                metadata.Length,
                metadata.CreatedAt),
            stream);
    }

    /// <inheritdoc />
    public Task DeleteAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        ValidateId(id);

        var root = GetRootDirectory();
        var dataPath = GetDataPath(root, id);
        var metadataPath = GetMetadataPath(root, id);

        File.Delete(dataPath);
        File.Delete(metadataPath);

        return Task.CompletedTask;
    }

    private string GetRootDirectory()
    {
        var configured = configuration["Media:FileSystem:RootDirectory"];
        var root = string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(environment.ContentRootPath, "App_Data", "media")
            : configured;

        return Path.GetFullPath(root, environment.ContentRootPath);
    }

    private static string GetDataPath(string root, string id)
        => Path.Combine(root, id + ".bin");

    private static string GetMetadataPath(string root, string id)
        => Path.Combine(root, id + ".json");

    private static void ValidateId(string id)
    {
        if (!Guid.TryParseExact(id, "N", out _))
        {
            throw new ArgumentException("The media identifier is invalid.", nameof(id));
        }
    }

    private static void ValidateFileName(string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        if (!string.Equals(fileName, Path.GetFileName(fileName), StringComparison.Ordinal))
        {
            throw new ArgumentException("A media file name must not contain directory segments.", nameof(fileName));
        }
    }

    private sealed record FileSystemMediaMetadata(
        string Id,
        string FileName,
        string ContentType,
        long Length,
        DateTimeOffset CreatedAt);
}

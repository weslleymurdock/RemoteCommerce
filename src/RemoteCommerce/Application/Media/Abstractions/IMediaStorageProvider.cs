namespace RemoteCommerce.Application.Media.Abstractions;

/// <summary>Provides provider-independent storage operations for media and large assets.</summary>
public interface IMediaStorageProvider
{
    /// <summary>Gets the stable storage provider identifier.</summary>
    string Name { get; }

    /// <summary>Stores an uploaded media object.</summary>
    /// <param name="upload">The content and metadata to store.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The provider-generated media identifier.</returns>
    Task<string> StoreAsync(MediaUpload upload, CancellationToken cancellationToken = default);

    /// <summary>Retrieves a media object by its provider-scoped identifier.</summary>
    /// <param name="id">The provider-scoped media identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>The stored object, or <see langword="null"/> when it does not exist.</returns>
    Task<MediaObject?> RetrieveAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>Deletes a media object from the storage provider.</summary>
    /// <param name="id">The provider-scoped media identifier.</param>
    /// <param name="cancellationToken">The token used to cancel the operation.</param>
    /// <returns>A task representing the deletion operation.</returns>
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}

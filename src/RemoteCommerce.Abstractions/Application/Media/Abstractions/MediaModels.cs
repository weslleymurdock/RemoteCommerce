namespace RemoteCommerce.Application.Media.Abstractions;

/// <summary>Describes a media object returned by a storage provider.</summary>
public sealed class MediaObjectDescriptor
{
    /// <summary>Initializes media metadata.</summary>
    /// <param name="id">The provider-scoped object identifier.</param>
    /// <param name="fileName">The client-facing file name.</param>
    /// <param name="contentType">The media content type.</param>
    /// <param name="length">The content length in bytes.</param>
    /// <param name="createdAt">The UTC creation timestamp.</param>
    public MediaObjectDescriptor(
        string id,
        string fileName,
        string contentType,
        long length,
        DateTimeOffset createdAt)
    {
        Id = id;
        FileName = fileName;
        ContentType = contentType;
        Length = length;
        CreatedAt = createdAt;
    }

    /// <summary>Gets the provider-scoped object identifier.</summary>
    public string Id { get; }

    /// <summary>Gets the client-facing file name.</summary>
    public string FileName { get; }

    /// <summary>Gets the media content type.</summary>
    public string ContentType { get; }

    /// <summary>Gets the content length in bytes.</summary>
    public long Length { get; }

    /// <summary>Gets the UTC creation timestamp.</summary>
    public DateTimeOffset CreatedAt { get; }
}

/// <summary>Describes content supplied to a media storage provider.</summary>
public sealed class MediaUpload
{
    /// <summary>Initializes media upload data.</summary>
    /// <param name="fileName">The client-supplied file name.</param>
    /// <param name="contentType">The media content type.</param>
    /// <param name="content">The readable content stream.</param>
    public MediaUpload(
        string fileName,
        string contentType,
        Stream content)
    {
        FileName = fileName;
        ContentType = contentType;
        Content = content ?? throw new ArgumentNullException(nameof(content));
    }

    /// <summary>Gets the client-supplied file name.</summary>
    public string FileName { get; }

    /// <summary>Gets the media content type.</summary>
    public string ContentType { get; }

    /// <summary>Gets the readable content stream.</summary>
    public Stream Content { get; }
}

/// <summary>Contains the content and metadata of a stored media object.</summary>
public sealed class MediaObject : IAsyncDisposable
{
    /// <summary>Initializes a media object.</summary>
    /// <param name="descriptor">The object metadata.</param>
    /// <param name="content">The readable content stream.</param>
    public MediaObject(
        MediaObjectDescriptor descriptor,
        Stream content)
    {
        Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
        Content = content ?? throw new ArgumentNullException(nameof(content));
    }

    /// <summary>Gets the media metadata.</summary>
    public MediaObjectDescriptor Descriptor { get; }

    /// <summary>Gets the readable content stream.</summary>
    public Stream Content { get; }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        Content.Dispose();
        return ValueTask.CompletedTask;
    }
}

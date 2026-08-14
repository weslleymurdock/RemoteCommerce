namespace RemoteCommerce.Application.Media.Abstractions;

/// <summary>Describes a media object returned by a storage provider.</summary>
public sealed record MediaObjectDescriptor(
    string Id,
    string FileName,
    string ContentType,
    long Length,
    DateTimeOffset CreatedAt);

/// <summary>Contains the content and metadata of a stored media object.</summary>
public sealed class MediaObject : IAsyncDisposable
{
    /// <summary>Initializes a media object.</summary>
    /// <param name="descriptor">The object metadata.</param>
    /// <param name="content">The readable content stream.</param>
    public MediaObject(MediaObjectDescriptor descriptor, Stream content)
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

/// <summary>Describes content supplied to a media storage provider.</summary>
public sealed record MediaUpload(
    string FileName,
    string ContentType,
    Stream Content);

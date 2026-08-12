namespace HelpDev.Modules.Media.Application.Storage;

/// <summary>Abstraction over blob/object storage. Local implementation in Infrastructure; future S3-compatible.</summary>
public interface IMediaStorage
{
    Task StoreAsync(
        Stream content,
        string storageKey,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string storageKey, CancellationToken cancellationToken = default);

    /// <summary>Internal rollback cleanup only — not exposed as a user delete API in v1.</summary>
    Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default);
}

public sealed record StoredMediaObject(string StorageKey, string ContentType, long SizeBytes);

using HelpDev.Modules.Media.Application.Options;
using HelpDev.Modules.Media.Application.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HelpDev.Modules.Media.Infrastructure.Storage;

/// <summary>
/// Local filesystem storage. Server-generated keys only; path traversal prevented.
/// Partial files cleaned up on failure. Not for source-controlled directories.
/// </summary>
public sealed class LocalMediaStorage : IMediaStorage
{
    private readonly string _root;
    private readonly ILogger<LocalMediaStorage> _logger;

    public LocalMediaStorage(IOptions<MediaOptions> options, ILogger<LocalMediaStorage> logger)
    {
        _logger = logger;
        var configured = options.Value.LocalStorageRoot;
        _root = string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "HelpDev",
                "media-uploads")
            : Path.GetFullPath(configured);

        Directory.CreateDirectory(_root);
    }

    public string RootPath => _root;

    public async Task StoreAsync(
        Stream content,
        string storageKey,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        var fullPath = ResolveSafePath(storageKey);
        var directory = Path.GetDirectoryName(fullPath)
            ?? throw new InvalidOperationException("Invalid storage path.");
        Directory.CreateDirectory(directory);

        var tempPath = fullPath + ".tmp-" + Guid.NewGuid().ToString("N");
        try
        {
            await using (var file = new FileStream(
                             tempPath,
                             FileMode.CreateNew,
                             FileAccess.Write,
                             FileShare.None,
                             bufferSize: 81920,
                             options: FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                await content.CopyToAsync(file, cancellationToken).ConfigureAwait(false);
                await file.FlushAsync(cancellationToken).ConfigureAwait(false);
            }

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            File.Move(tempPath, fullPath);
        }
        catch
        {
            TryDelete(tempPath);
            throw;
        }
    }

    public Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var fullPath = ResolveSafePath(storageKey);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("Media object not found.");
        }

        Stream stream = new FileStream(
            fullPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            options: FileOptions.Asynchronous | FileOptions.SequentialScan);
        return Task.FromResult(stream);
    }

    public Task<bool> ExistsAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var fullPath = ResolveSafePath(storageKey);
        return Task.FromResult(File.Exists(fullPath));
    }

    public Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var fullPath = ResolveSafePath(storageKey);
        TryDelete(fullPath);
        return Task.CompletedTask;
    }

    internal string ResolveSafePath(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
        {
            throw new ArgumentException("Storage key is required.", nameof(storageKey));
        }

        var normalized = storageKey.Replace('\\', '/').Trim().TrimStart('/');
        if (normalized.Length == 0
            || normalized.Contains("..", StringComparison.Ordinal)
            || Path.IsPathRooted(normalized)
            || normalized.Contains(':', StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Invalid storage key.");
        }

        var combined = Path.GetFullPath(Path.Combine(_root, normalized.Replace('/', Path.DirectorySeparatorChar)));
        var rootWithSep = _root.EndsWith(Path.DirectorySeparatorChar)
            ? _root
            : _root + Path.DirectorySeparatorChar;

        if (!combined.StartsWith(rootWithSep, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(combined, _root, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Storage path escapes configured root.");
        }

        return combined;
    }

    private void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete media temp/orphan file");
        }
    }
}

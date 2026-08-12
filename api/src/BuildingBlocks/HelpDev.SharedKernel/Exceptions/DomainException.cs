namespace HelpDev.SharedKernel.Exceptions;

public class DomainException : Exception
{
    private readonly Dictionary<string, object?> _metadata;

    public DomainException(string message)
        : base(message)
    {
        _metadata = new Dictionary<string, object?>(StringComparer.Ordinal);
    }

    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
        _metadata = new Dictionary<string, object?>(StringComparer.Ordinal);
    }

    public DomainException(string message, string? code)
        : base(message)
    {
        Code = code;
        _metadata = new Dictionary<string, object?>(StringComparer.Ordinal);
    }

    public DomainException(
        string message,
        string? code,
        IReadOnlyDictionary<string, object?> metadata)
        : base(message)
    {
        Code = code;
        _metadata = metadata is null
            ? new Dictionary<string, object?>(StringComparer.Ordinal)
            : new Dictionary<string, object?>(metadata, StringComparer.Ordinal);
    }

    public DomainException(
        string message,
        string? code,
        IReadOnlyDictionary<string, object?> metadata,
        Exception innerException)
        : base(message, innerException)
    {
        Code = code;
        _metadata = metadata is null
            ? new Dictionary<string, object?>(StringComparer.Ordinal)
            : new Dictionary<string, object?>(metadata, StringComparer.Ordinal);
    }

    /// <summary>
    /// Optional stable error code for mapping to application/API errors.
    /// </summary>
    public string? Code { get; }

    public IReadOnlyDictionary<string, object?> Metadata => _metadata;

    public DomainException WithMetadata(string key, object? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        _metadata[key] = value;
        return this;
    }
}

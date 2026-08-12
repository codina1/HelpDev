using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.Content.Domain.News;

/// <summary>
/// News-specific metadata satellite. Content owns lifecycle; this entity holds
/// source, date, priority, and external reference only.
/// </summary>
public sealed class NewsMetadata
{
    public const int MaxSourceNameLength = 200;
    public const int MaxSourceUrlLength = 2048;
    public const int MaxExternalReferenceLength = 500;

    /// <summary>Required for EF Core materialization.</summary>
    private NewsMetadata()
    {
    }

    private NewsMetadata(
        Guid id,
        Guid contentId,
        string sourceName,
        string? sourceUrl,
        DateTime newsDateUtc,
        NewsPriority priority,
        string? externalReference,
        DateTime createdAtUtc,
        DateTime updatedAtUtc)
    {
        Id = id;
        ContentId = contentId;
        SourceName = sourceName;
        SourceUrl = sourceUrl;
        NewsDateUtc = newsDateUtc;
        Priority = priority;
        ExternalReference = externalReference;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid ContentId { get; private set; }

    public string SourceName { get; private set; } = string.Empty;

    public string? SourceUrl { get; private set; }

    public DateTime NewsDateUtc { get; private set; }

    public NewsPriority Priority { get; private set; }

    public string? ExternalReference { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public static NewsMetadata Create(
        Guid id,
        Guid contentId,
        string sourceName,
        string? sourceUrl,
        DateTime newsDateUtc,
        NewsPriority priority,
        string? externalReference,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("شناسه متادیتای خبر الزامی است.");
        }

        if (contentId == Guid.Empty)
        {
            throw new DomainException("شناسه محتوا الزامی است.");
        }

        if (!Enum.IsDefined(priority))
        {
            throw new DomainException("اولویت خبر معتبر نیست.");
        }

        var normalizedSource = NormalizeRequiredSource(sourceName);
        var normalizedUrl = NormalizeOptionalUrl(sourceUrl, "آدرس منبع");
        var normalizedExternal = NormalizeOptionalLength(
            externalReference,
            MaxExternalReferenceLength,
            "ارجاع خارجی");

        return new NewsMetadata(
            id,
            contentId,
            normalizedSource,
            normalizedUrl,
            newsDateUtc,
            priority,
            normalizedExternal,
            createdAtUtc,
            createdAtUtc);
    }

    public void Update(
        string sourceName,
        string? sourceUrl,
        DateTime newsDateUtc,
        NewsPriority priority,
        string? externalReference,
        DateTime updatedAtUtc)
    {
        if (!Enum.IsDefined(priority))
        {
            throw new DomainException("اولویت خبر معتبر نیست.");
        }

        SourceName = NormalizeRequiredSource(sourceName);
        SourceUrl = NormalizeOptionalUrl(sourceUrl, "آدرس منبع");
        NewsDateUtc = newsDateUtc;
        Priority = priority;
        ExternalReference = NormalizeOptionalLength(
            externalReference,
            MaxExternalReferenceLength,
            "ارجاع خارجی");
        UpdatedAtUtc = updatedAtUtc;
    }

    private static string NormalizeRequiredSource(string? sourceName)
    {
        if (string.IsNullOrWhiteSpace(sourceName))
        {
            throw new DomainException("نام منبع خبر الزامی است.");
        }

        var normalized = sourceName.Trim();
        if (normalized.Length > MaxSourceNameLength)
        {
            throw new DomainException("نام منبع بیش از حد مجاز است.");
        }

        return normalized;
    }

    private static string? NormalizeOptionalUrl(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > MaxSourceUrlLength)
        {
            throw new DomainException($"{fieldName} بیش از حد مجاز است.");
        }

        if (!Uri.TryCreate(normalized, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new DomainException($"{fieldName} معتبر نیست.");
        }

        return normalized;
    }

    private static string? NormalizeOptionalLength(string? value, int maxLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new DomainException($"{fieldName} بیش از حد مجاز است.");
        }

        return normalized;
    }
}

using HelpDev.Modules.Content.Domain.Articles;
using HelpDev.Modules.Content.Domain.Enums;
using HelpDev.Modules.Content.Domain.Workflow;
using HelpDev.Modules.Content.Domain.Events;
using HelpDev.Modules.Content.Domain.ValueObjects;
using HelpDev.SharedKernel.Common;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.Content.Domain.Entities;

public class Content : AggregateRoot<Guid>
{
    public const int MaxExcerptLength = 500;

    public const int MaxCoverImageLength = 2048;

    /// <summary>Required for EF Core materialization. Does not raise domain events.</summary>
    private Content()
    {
    }

    private Content(Guid id)
        : base(id)
    {
    }

    public string Title { get; private set; } = string.Empty;

    public Slug Slug { get; private set; } = null!;

    public string Body { get; private set; } = string.Empty;

    public string Excerpt { get; private set; } = string.Empty;

    public string? CoverImage { get; private set; }

    public SeoMetadata SeoMetadata { get; private set; } = SeoMetadata.Empty;

    public ContentType Type { get; private set; }

    public Guid AuthorId { get; private set; }

    public ContentStatus Status { get; private set; } = ContentStatus.Draft;

    public int Views { get; private set; }

    public int Saves { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public DateTime? PublishedAtUtc { get; private set; }

    public string? ContentJson { get; private set; }

    public string? ContentHtml { get; private set; }

    public string? ContentFormat { get; private set; }

    public string? EditorVersion { get; private set; }

    public int? WordCount { get; private set; }

    public int? ReadingTimeMinutes { get; private set; }

    public DateTime? LastAutosavedAtUtc { get; private set; }

    public static Content Create(
        Guid id,
        string title,
        Slug slug,
        string body,
        ContentType type,
        Guid authorId,
        ContentStatus status,
        DateTime createdAtUtc,
        string? excerpt = null,
        string? coverImage = null)
    {
        ArgumentNullException.ThrowIfNull(slug);

        var content = new Content(id);
        content.AuthorId = authorId;
        content.Type = type;
        content.Views = 0;
        content.Saves = 0;
        content.CreatedAt = createdAtUtc;
        content.UpdatedAt = createdAtUtc;
        content.Status = ContentStatus.Draft;
        content.ApplyDetails(title, slug, type, body, excerpt, coverImage, createdAtUtc, raiseUpdatedEvent: false);

        if (status == ContentStatus.Published)
        {
            throw new DomainException("برای انتشار از گردش کار استفاده کنید.");
        }

        if (status != ContentStatus.Draft)
        {
            throw new DomainException("وضعیت محتوا معتبر نیست.");
        }

        return content;
    }

    /// <summary>
    /// Reconstitutes a published content item for seeding without raising domain events.
    /// </summary>
    public static Content CreatePublishedSeed(
        Guid id,
        string title,
        Slug slug,
        string body,
        ContentType type,
        Guid authorId,
        DateTime createdAtUtc,
        int views,
        int saves)
    {
        ArgumentNullException.ThrowIfNull(slug);

        var content = new Content(id);
        content.ApplyDetails(title, slug, type, body, excerpt: null, coverImage: null, createdAtUtc, raiseUpdatedEvent: false);
        content.AuthorId = authorId;
        content.Status = ContentStatus.Published;
        content.Views = views;
        content.Saves = saves;
        content.CreatedAt = createdAtUtc;
        content.UpdatedAt = createdAtUtc;
        content.PublishedAtUtc = createdAtUtc;
        content.ClearDomainEvents();
        return content;
    }

    public bool UpdateDetails(
        string title,
        Slug slug,
        ContentType type,
        string body,
        string? excerpt,
        string? coverImage,
        DateTime updatedAtUtc,
        ArticleEditorDocument? editorDocument = null)
    {
        ArgumentNullException.ThrowIfNull(slug);
        return ApplyDetails(title, slug, type, body, excerpt, coverImage, updatedAtUtc, raiseUpdatedEvent: true, editorDocument);
    }

    public bool MarkAutosaved(DateTime utcNow)
    {
        LastAutosavedAtUtc = utcNow;
        return true;
    }

    public ContentWorkflowTransition SubmitForReview(Guid actorUserId, DateTime utc)
    {
        var from = Status;
        ContentWorkflowRules.EnsureAllowed(from, ContentStatus.ReviewPending);
        Status = ContentStatus.ReviewPending;
        UpdatedAt = utc;
        return CreateTransition(from, ContentStatus.ReviewPending, actorUserId, comment: null, utc);
    }

    public ContentWorkflowTransition Approve(Guid actorUserId, DateTime utc)
    {
        var from = Status;
        ContentWorkflowRules.EnsureAllowed(from, ContentStatus.Approved);
        Status = ContentStatus.Approved;
        UpdatedAt = utc;
        return CreateTransition(from, ContentStatus.Approved, actorUserId, comment: null, utc);
    }

    public ContentWorkflowTransition Reject(string comment, Guid actorUserId, DateTime utc)
    {
        var normalizedComment = ContentWorkflowTransition.NormalizeComment(comment, required: true)!;
        var from = Status;
        ContentWorkflowRules.EnsureAllowed(from, ContentStatus.Draft);
        Status = ContentStatus.Draft;
        UpdatedAt = utc;
        return CreateTransition(from, ContentStatus.Draft, actorUserId, normalizedComment, utc);
    }

    public ContentWorkflowTransition Publish(Guid actorUserId, DateTime publishedAtUtc)
    {
        if (Status == ContentStatus.Published)
        {
            throw new DomainException("محتوا قبلاً منتشر شده است.");
        }

        var from = Status;
        ApplyPublish(publishedAtUtc);
        return CreateTransition(from, ContentStatus.Published, actorUserId, comment: null, publishedAtUtc);
    }

    public ContentWorkflowTransition Archive(Guid actorUserId, DateTime utc)
    {
        var from = Status;
        ContentWorkflowRules.EnsureAllowed(from, ContentStatus.Archived);
        Status = ContentStatus.Archived;
        UpdatedAt = utc;
        return CreateTransition(from, ContentStatus.Archived, actorUserId, comment: null, utc);
    }

    private void ApplyPublish(DateTime publishedAtUtc)
    {
        if (Status == ContentStatus.Published)
        {
            return;
        }

        ContentWorkflowRules.EnsureAllowed(Status, ContentStatus.Published);
        Status = ContentStatus.Published;
        PublishedAtUtc = publishedAtUtc;
        UpdatedAt = publishedAtUtc;
        AddDomainEvent(new ContentPublishedDomainEvent(Id, Slug.Value));
    }

    private ContentWorkflowTransition CreateTransition(
        ContentStatus from,
        ContentStatus to,
        Guid actorUserId,
        string? comment,
        DateTime utc) =>
        ContentWorkflowTransition.Create(
            Guid.NewGuid(),
            Id,
            from,
            to,
            actorUserId,
            comment,
            utc);

    public bool UpdateSeoMetadata(SeoMetadata seoMetadata, DateTime updatedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(seoMetadata);

        if (SeoMetadata == seoMetadata)
        {
            return false;
        }

        SeoMetadata = seoMetadata;
        UpdatedAt = updatedAtUtc;

        // Draft edits stay silent; only published changes drive search/read-model refresh.
        if (Status == ContentStatus.Published)
        {
            AddDomainEvent(new ContentUpdatedDomainEvent(Id, Slug.Value));
        }

        return true;
    }

    /// <summary>
    /// Restores editable fields from a revision snapshot. Always applies and returns true
    /// (restore is an explicit audit action even when identical to current state).
    /// </summary>
    public bool RestoreFromSnapshot(ContentRevisionSnapshot snapshot, DateTime updatedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        if (!Slug.TryCreate(snapshot.Slug, out var slug) || slug is null)
        {
            throw new DomainException("اسلاگ معتبر نیست.");
        }

        if (!Enum.TryParse<ContentType>(snapshot.ContentType, ignoreCase: true, out var type))
        {
            throw new DomainException("نوع محتوا معتبر نیست.");
        }

        var seo = SeoMetadata.Create(
            snapshot.SeoMetadata.SeoTitle,
            snapshot.SeoMetadata.SeoDescription,
            snapshot.SeoMetadata.CanonicalUrl,
            snapshot.SeoMetadata.OgImage,
            snapshot.SeoMetadata.FocusKeyword);

        ApplyDetails(
            snapshot.Title,
            slug,
            type,
            snapshot.Body,
            snapshot.Excerpt,
            snapshot.CoverImage,
            updatedAtUtc,
            raiseUpdatedEvent: false,
            snapshot.ToEditorDocument());

        SeoMetadata = seo;
        UpdatedAt = updatedAtUtc;

        if (Status == ContentStatus.Published)
        {
            AddDomainEvent(new ContentUpdatedDomainEvent(Id, Slug.Value));
        }

        return true;
    }

    private bool ApplyDetails(
        string title,
        Slug slug,
        ContentType type,
        string body,
        string? excerpt,
        string? coverImage,
        DateTime timestampUtc,
        bool raiseUpdatedEvent,
        ArticleEditorDocument? editorDocument = null)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Length > 300)
        {
            throw new DomainException("عنوان معتبر نیست.");
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            throw new DomainException("متن محتوا الزامی است.");
        }

        var normalizedExcerpt = NormalizeExcerpt(excerpt);
        var normalizedCoverImage = NormalizeCoverImage(coverImage);
        var normalizedTitle = title.Trim();
        var normalizedBody = body.Trim();
        var normalizedDocument = NormalizeEditorDocument(editorDocument);

        var changed =
            !string.Equals(Title, normalizedTitle, StringComparison.Ordinal)
            || Slug is null
            || Slug != slug
            || Type != type
            || !string.Equals(Body, normalizedBody, StringComparison.Ordinal)
            || !string.Equals(Excerpt, normalizedExcerpt, StringComparison.Ordinal)
            || !string.Equals(CoverImage, normalizedCoverImage, StringComparison.Ordinal)
            || !EditorDocumentEquals(normalizedDocument);

        Title = normalizedTitle;
        Slug = slug;
        Type = type;
        Body = normalizedBody;
        Excerpt = normalizedExcerpt;
        CoverImage = normalizedCoverImage;
        ApplyEditorDocument(normalizedDocument);

        if (!changed)
        {
            return false;
        }

        UpdatedAt = timestampUtc;

        // Draft edits stay silent; only published changes drive search/read-model refresh.
        if (raiseUpdatedEvent && Status == ContentStatus.Published)
        {
            AddDomainEvent(new ContentUpdatedDomainEvent(Id, Slug.Value));
        }

        return true;
    }

    private bool EditorDocumentEquals(ArticleEditorDocument? document)
    {
        if (document is null)
        {
            return true;
        }

        return string.Equals(ContentJson, document.ContentJson, StringComparison.Ordinal)
            && string.Equals(ContentHtml, document.ContentHtml, StringComparison.Ordinal)
            && string.Equals(ContentFormat, document.ContentFormat, StringComparison.Ordinal)
            && string.Equals(EditorVersion, document.EditorVersion, StringComparison.Ordinal)
            && WordCount == document.WordCount
            && ReadingTimeMinutes == document.ReadingTimeMinutes;
    }

    private void ApplyEditorDocument(ArticleEditorDocument? document)
    {
        if (document is null)
        {
            return;
        }

        ContentJson = document.ContentJson;
        ContentHtml = document.ContentHtml;
        ContentFormat = document.ContentFormat;
        EditorVersion = document.EditorVersion;
        WordCount = document.WordCount;
        ReadingTimeMinutes = document.ReadingTimeMinutes;
    }

    private static ArticleEditorDocument? NormalizeEditorDocument(ArticleEditorDocument? document)
    {
        if (document is null)
        {
            return null;
        }

        var format = string.IsNullOrWhiteSpace(document.ContentFormat)
            ? null
            : document.ContentFormat.Trim().ToLowerInvariant();
        if (format is not null && format.Length > ArticleEditorLimits.MaxContentFormatLength)
        {
            throw new DomainException("فرمت محتوا معتبر نیست.");
        }

        if (format is not null
            && format != ArticleEditorLimits.MarkdownFormat
            && format != ArticleEditorLimits.BlocksFormat)
        {
            throw new DomainException("فرمت محتوا معتبر نیست.");
        }

        var version = string.IsNullOrWhiteSpace(document.EditorVersion)
            ? null
            : document.EditorVersion.Trim();
        if (version is not null && version.Length > ArticleEditorLimits.MaxEditorVersionLength)
        {
            throw new DomainException("نسخه ویرایشگر معتبر نیست.");
        }

        if (document.ContentJson is { Length: > ArticleEditorLimits.MaxContentJsonLength })
        {
            throw new DomainException("ساختار بلوکی محتوا بیش از حد مجاز است.");
        }

        if (document.ContentHtml is { Length: > ArticleEditorLimits.MaxContentHtmlLength })
        {
            throw new DomainException("خروجی HTML محتوا بیش از حد مجاز است.");
        }

        if (document.WordCount is < 0)
        {
            throw new DomainException("تعداد کلمات معتبر نیست.");
        }

        if (document.ReadingTimeMinutes is < 0 or > ArticleEditorLimits.MaxReadingTimeMinutes)
        {
            throw new DomainException("زمان مطالعه معتبر نیست.");
        }

        return document with
        {
            ContentFormat = format,
            EditorVersion = version,
            ContentJson = string.IsNullOrWhiteSpace(document.ContentJson) ? null : document.ContentJson,
            ContentHtml = string.IsNullOrWhiteSpace(document.ContentHtml) ? null : document.ContentHtml,
        };
    }

    private static string NormalizeExcerpt(string? excerpt)
    {
        if (string.IsNullOrWhiteSpace(excerpt))
        {
            return string.Empty;
        }

        var normalized = excerpt.Trim();
        if (normalized.Length > MaxExcerptLength)
        {
            throw new DomainException("خلاصه محتوا بیش از حد مجاز است.");
        }

        return normalized;
    }

    private static string? NormalizeCoverImage(string? coverImage)
    {
        if (string.IsNullOrWhiteSpace(coverImage))
        {
            return null;
        }

        var normalized = coverImage.Trim();
        if (normalized.Length > MaxCoverImageLength)
        {
            throw new DomainException("آدرس تصویر کاور بیش از حد مجاز است.");
        }

        return normalized;
    }
}

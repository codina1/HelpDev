using HelpDev.SharedKernel.Common;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.PromptLab.Domain.Prompts;

public sealed class Prompt : AggregateRoot<Guid>
{
    public const int TitleMaxLength = 150;
    public const int SlugMaxLength = 120;
    public const int DescriptionMaxLength = 3000;

    private Prompt()
    {
    }

    private Prompt(Guid id)
        : base(id)
    {
    }

    public string Title { get; private set; } = string.Empty;

    public PromptSlug Slug { get; private set; } = null!;

    public string? Description { get; private set; }

    public string Content { get; private set; } = string.Empty;

    public string? CoverImage { get; private set; }

    public PromptMediaType MediaType { get; private set; }

    public string AiModel { get; private set; } = string.Empty;

    public PromptStatus Status { get; private set; }

    public Guid AuthorId { get; private set; }

    public int Views { get; private set; }

    public int CopyCount { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public DateTime? PublishedAt { get; private set; }

    public bool IsPublic => Status == PromptStatus.Approved;

    public bool CanBeEditedBy(Guid actorUserId) =>
        Status == PromptStatus.Draft && actorUserId != Guid.Empty && actorUserId == AuthorId;

    public static Prompt Create(
        Guid id,
        string title,
        string slug,
        string? description,
        string content,
        string? coverImage,
        PromptMediaType mediaType,
        string aiModel,
        Guid authorId,
        DateTime utcNow)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("Prompt id must not be empty.", PromptLabErrorCodes.PromptTitleInvalid);
        }

        if (authorId == Guid.Empty)
        {
            throw new DomainException("Author id is required.", PromptLabErrorCodes.PromptAuthorInvalid);
        }

        EnsureMediaType(mediaType);

        var prompt = new Prompt(id)
        {
            AuthorId = authorId,
            Status = PromptStatus.Draft,
            Views = 0,
            CopyCount = 0,
            CreatedAt = utcNow,
            UpdatedAt = utcNow,
            PublishedAt = null,
            Slug = PromptSlug.Create(
                slug,
                SlugMaxLength,
                PromptLabErrorCodes.PromptSlugRequired,
                PromptLabErrorCodes.PromptSlugInvalid),
        };

        prompt.ApplyDetails(title, description, content, coverImage, mediaType, aiModel, force: true);
        return prompt;
    }

    public bool Update(
        Guid actorUserId,
        string title,
        string slug,
        string? description,
        string content,
        string? coverImage,
        PromptMediaType mediaType,
        string aiModel,
        DateTime utcNow)
    {
        EnsureOwner(actorUserId);
        EnsureDraft();
        EnsureMediaType(mediaType);

        var nextSlug = PromptSlug.Create(
            slug,
            SlugMaxLength,
            PromptLabErrorCodes.PromptSlugRequired,
            PromptLabErrorCodes.PromptSlugInvalid);

        var changed = ApplyDetails(title, description, content, coverImage, mediaType, aiModel, force: false);
        if (Slug != nextSlug)
        {
            Slug = nextSlug;
            changed = true;
        }

        if (!changed)
        {
            return false;
        }

        UpdatedAt = utcNow;
        return true;
    }

    public void Submit(Guid actorUserId, DateTime utcNow)
    {
        EnsureOwner(actorUserId);
        PromptWorkflowRules.EnsureAllowed(Status, PromptStatus.Submitted);
        Status = PromptStatus.Submitted;
        UpdatedAt = utcNow;
    }

    public void Approve(DateTime utcNow)
    {
        PromptWorkflowRules.EnsureAllowed(Status, PromptStatus.Approved);
        Status = PromptStatus.Approved;
        PublishedAt = utcNow;
        UpdatedAt = utcNow;
        AddDomainEvent(new PromptApprovedDomainEvent(Id, Slug.Value));
    }

    public void Reject(DateTime utcNow)
    {
        PromptWorkflowRules.EnsureAllowed(Status, PromptStatus.Rejected);
        Status = PromptStatus.Rejected;
        UpdatedAt = utcNow;
    }

    public void ReturnToDraft(Guid actorUserId, DateTime utcNow)
    {
        EnsureOwner(actorUserId);
        PromptWorkflowRules.EnsureAllowed(Status, PromptStatus.Draft);
        Status = PromptStatus.Draft;
        UpdatedAt = utcNow;
    }

    public string GetPublicContent()
    {
        EnsurePublic();
        return Content;
    }

    public void RecordView()
    {
        EnsurePublic();
        Views++;
    }

    public void RecordCopy()
    {
        EnsurePublic();
        CopyCount++;
    }

    public void EnsurePublic()
    {
        if (!IsPublic)
        {
            throw new DomainException(
                "Only approved prompts are public.",
                PromptLabErrorCodes.PromptNotPublic);
        }
    }

    private void EnsureOwner(Guid actorUserId)
    {
        if (actorUserId == Guid.Empty || actorUserId != AuthorId)
        {
            throw new DomainException(
                "Only the owner author can change this prompt.",
                PromptLabErrorCodes.PromptEditForbidden);
        }
    }

    private void EnsureDraft()
    {
        if (Status != PromptStatus.Draft)
        {
            throw new DomainException(
                "Only draft prompts can be edited.",
                PromptLabErrorCodes.PromptNotDraft);
        }
    }

    private bool ApplyDetails(
        string title,
        string? description,
        string content,
        string? coverImage,
        PromptMediaType mediaType,
        string aiModel,
        bool force)
    {
        var normalizedTitle = NormalizeRequired(
            title,
            TitleMaxLength,
            PromptLabErrorCodes.PromptTitleRequired,
            PromptLabErrorCodes.PromptTitleInvalid);
        var normalizedDescription = NormalizeOptional(
            description,
            DescriptionMaxLength,
            PromptLabErrorCodes.PromptTitleInvalid);
        var normalizedContent = NormalizeRequired(
            content,
            PromptLabLimits.MaxPromptContentLength,
            PromptLabErrorCodes.PromptContentRequired,
            PromptLabErrorCodes.PromptContentInvalid);
        var normalizedCoverImage = NormalizeOptional(
            coverImage,
            PromptLabLimits.MaxPromptCoverImageLength,
            PromptLabErrorCodes.PromptCoverImageInvalid);
        var normalizedAiModel = NormalizeRequired(
            aiModel,
            PromptLabLimits.MaxPromptAiModelLength,
            PromptLabErrorCodes.PromptAiModelRequired,
            PromptLabErrorCodes.PromptAiModelInvalid);

        var changed =
            force
            || !string.Equals(Title, normalizedTitle, StringComparison.Ordinal)
            || !string.Equals(Description, normalizedDescription, StringComparison.Ordinal)
            || !string.Equals(Content, normalizedContent, StringComparison.Ordinal)
            || !string.Equals(CoverImage, normalizedCoverImage, StringComparison.Ordinal)
            || MediaType != mediaType
            || !string.Equals(AiModel, normalizedAiModel, StringComparison.Ordinal);

        Title = normalizedTitle;
        Description = normalizedDescription;
        Content = normalizedContent;
        CoverImage = normalizedCoverImage;
        MediaType = mediaType;
        AiModel = normalizedAiModel;
        return changed;
    }

    private static void EnsureMediaType(PromptMediaType mediaType)
    {
        if (!Enum.IsDefined(mediaType))
        {
            throw new DomainException("Media type is invalid.", PromptLabErrorCodes.PromptMediaTypeInvalid);
        }
    }

    private static string NormalizeRequired(string value, int maxLength, string requiredCode, string invalidCode)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Value is required.", requiredCode);
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new DomainException("Value is invalid.", invalidCode);
        }

        return trimmed;
    }

    private static string? NormalizeOptional(string? value, int maxLength, string invalidCode)
    {
        if (value is null)
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length == 0)
        {
            return null;
        }

        if (trimmed.Length > maxLength)
        {
            throw new DomainException("Value is invalid.", invalidCode);
        }

        return trimmed;
    }
}

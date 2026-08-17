using HelpDev.Modules.PromptLab.Domain.AiModels;
using HelpDev.Modules.PromptLab.Domain.Categories;
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

    public Guid CategoryId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public PromptSlug Slug { get; private set; } = null!;

    public string? Description { get; private set; }

    public string Content { get; private set; } = string.Empty;

    public string? CoverImage { get; private set; }

    public PromptMediaType MediaType { get; private set; }

    public Guid AiModelId { get; private set; }

    public PromptStatus Status { get; private set; }

    public Guid AuthorId { get; private set; }

    public int Views { get; private set; }

    public int CopyCount { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public DateTime? PublishedAt { get; private set; }

    public string? RejectionReason { get; private set; }

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
        Guid aiModelId,
        Guid categoryId,
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
            CategoryId = EnsureCategoryId(categoryId),
            AiModelId = EnsureAiModelId(aiModelId),
            AuthorId = authorId,
            Status = PromptStatus.Draft,
            Views = 0,
            CopyCount = 0,
            CreatedAt = utcNow,
            UpdatedAt = utcNow,
            PublishedAt = null,
            RejectionReason = null,
            Slug = PromptSlug.Create(
                slug,
                SlugMaxLength,
                PromptLabErrorCodes.PromptSlugRequired,
                PromptLabErrorCodes.PromptSlugInvalid),
        };

        prompt.ApplyDetails(title, description, content, coverImage, mediaType, force: true);
        return prompt;
    }

    public static Prompt Create(
        Guid id,
        string title,
        string slug,
        string? description,
        string content,
        string? coverImage,
        PromptMediaType mediaType,
        AiModel aiModel,
        PromptCategory category,
        Guid authorId,
        DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(aiModel);
        ArgumentNullException.ThrowIfNull(category);
        aiModel.EnsureActive();
        category.EnsureActive();
        return Create(
            id,
            title,
            slug,
            description,
            content,
            coverImage,
            mediaType,
            aiModel.Id,
            category.Id,
            authorId,
            utcNow);
    }

    public bool Update(
        Guid actorUserId,
        string title,
        string slug,
        string? description,
        string content,
        string? coverImage,
        PromptMediaType mediaType,
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

        var changed = ApplyDetails(title, description, content, coverImage, mediaType, force: false);
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

    public bool ChangeCategory(Guid actorUserId, Guid categoryId, DateTime utcNow)
    {
        EnsureOwner(actorUserId);
        EnsureDraft();

        var nextCategoryId = EnsureCategoryId(categoryId);
        if (CategoryId == nextCategoryId)
        {
            return false;
        }

        CategoryId = nextCategoryId;
        UpdatedAt = utcNow;
        return true;
    }

    public bool ChangeCategory(Guid actorUserId, PromptCategory category, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(category);
        category.EnsureActive();
        return ChangeCategory(actorUserId, category.Id, utcNow);
    }

    public bool ChangeAiModel(Guid actorUserId, Guid aiModelId, DateTime utcNow)
    {
        EnsureOwner(actorUserId);
        EnsureDraft();

        var nextAiModelId = EnsureAiModelId(aiModelId);
        if (AiModelId == nextAiModelId)
        {
            return false;
        }

        AiModelId = nextAiModelId;
        UpdatedAt = utcNow;
        return true;
    }

    public bool ChangeAiModel(Guid actorUserId, AiModel aiModel, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(aiModel);
        aiModel.EnsureActive();
        return ChangeAiModel(actorUserId, aiModel.Id, utcNow);
    }

    public void Submit(Guid actorUserId, DateTime utcNow)
    {
        EnsureOwner(actorUserId);
        PromptWorkflowRules.EnsureAllowed(Status, PromptStatus.Submitted);
        Status = PromptStatus.Submitted;
        RejectionReason = null;
        UpdatedAt = utcNow;
    }

    public void Approve(DateTime utcNow)
    {
        PromptWorkflowRules.EnsureAllowed(Status, PromptStatus.Approved);
        Status = PromptStatus.Approved;
        PublishedAt = utcNow;
        RejectionReason = null;
        UpdatedAt = utcNow;
        AddDomainEvent(new PromptApprovedDomainEvent(Id, Slug.Value));
    }

    public void Reject(DateTime utcNow, string? reason = null)
    {
        PromptWorkflowRules.EnsureAllowed(Status, PromptStatus.Rejected);
        Status = PromptStatus.Rejected;
        RejectionReason = NormalizeOptional(
            reason,
            PromptLabLimits.MaxPromptRejectionReasonLength,
            PromptLabErrorCodes.PromptRejectionReasonInvalid);
        UpdatedAt = utcNow;
    }

    public void ReturnToDraft(Guid actorUserId, DateTime utcNow)
    {
        EnsureOwner(actorUserId);
        PromptWorkflowRules.EnsureAllowed(Status, PromptStatus.Draft);
        Status = PromptStatus.Draft;
        RejectionReason = null;
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

        var changed =
            force
            || !string.Equals(Title, normalizedTitle, StringComparison.Ordinal)
            || !string.Equals(Description, normalizedDescription, StringComparison.Ordinal)
            || !string.Equals(Content, normalizedContent, StringComparison.Ordinal)
            || !string.Equals(CoverImage, normalizedCoverImage, StringComparison.Ordinal)
            || MediaType != mediaType;

        Title = normalizedTitle;
        Description = normalizedDescription;
        Content = normalizedContent;
        CoverImage = normalizedCoverImage;
        MediaType = mediaType;
        return changed;
    }

    private static void EnsureMediaType(PromptMediaType mediaType)
    {
        if (!Enum.IsDefined(mediaType))
        {
            throw new DomainException("Media type is invalid.", PromptLabErrorCodes.PromptMediaTypeInvalid);
        }
    }

    private static Guid EnsureCategoryId(Guid categoryId)
    {
        if (categoryId == Guid.Empty)
        {
            throw new DomainException("Category id is required.", PromptLabErrorCodes.PromptCategoryInvalid);
        }

        return categoryId;
    }

    private static Guid EnsureAiModelId(Guid aiModelId)
    {
        if (aiModelId == Guid.Empty)
        {
            throw new DomainException("AI model id is required.", PromptLabErrorCodes.PromptAiModelInvalid);
        }

        return aiModelId;
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

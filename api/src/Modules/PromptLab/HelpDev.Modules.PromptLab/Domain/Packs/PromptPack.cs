using HelpDev.Modules.PromptLab.Domain.Prompts;
using HelpDev.SharedKernel.Common;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.PromptLab.Domain.Packs;

public sealed class PromptPack : AggregateRoot<Guid>
{
    public const int TitleMaxLength = 150;
    public const int SlugMaxLength = 120;
    public const int DescriptionMaxLength = 3000;

    private readonly List<PromptPackItem> _items = [];

    private PromptPack()
    {
    }

    private PromptPack(Guid id)
        : base(id)
    {
    }

    public string Title { get; private set; } = string.Empty;

    public PromptSlug Slug { get; private set; } = null!;

    public string? Description { get; private set; }

    public string? CoverImage { get; private set; }

    public Guid AuthorId { get; private set; }

    public PromptPackStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public DateTime? PublishedAt { get; private set; }

    public IReadOnlyList<PromptPackItem> Items => _items.AsReadOnly();

    public bool IsPublic => Status == PromptPackStatus.Approved;

    public bool CanBeEditedBy(Guid actorUserId) =>
        Status == PromptPackStatus.Draft && actorUserId != Guid.Empty && actorUserId == AuthorId;

    public static PromptPack Create(
        Guid id,
        string title,
        string slug,
        string? description,
        string? coverImage,
        Guid authorId,
        DateTime utcNow)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("Pack id must not be empty.", PromptLabErrorCodes.PackTitleInvalid);
        }

        if (authorId == Guid.Empty)
        {
            throw new DomainException("Author id is required.", PromptLabErrorCodes.PackAuthorInvalid);
        }

        var pack = new PromptPack(id)
        {
            AuthorId = authorId,
            Status = PromptPackStatus.Draft,
            CreatedAt = utcNow,
            UpdatedAt = utcNow,
            PublishedAt = null,
            Slug = PromptSlug.Create(
                slug,
                SlugMaxLength,
                PromptLabErrorCodes.PackSlugRequired,
                PromptLabErrorCodes.PackSlugInvalid),
        };

        pack.ApplyDetails(title, description, coverImage, force: true);
        return pack;
    }

    public bool Update(
        Guid actorUserId,
        string title,
        string slug,
        string? description,
        string? coverImage,
        DateTime utcNow)
    {
        EnsureOwner(actorUserId);
        EnsureDraft();

        var nextSlug = PromptSlug.Create(
            slug,
            SlugMaxLength,
            PromptLabErrorCodes.PackSlugRequired,
            PromptLabErrorCodes.PackSlugInvalid);

        var changed = ApplyDetails(title, description, coverImage, force: false);
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

    public PromptPackItem AddItem(Guid actorUserId, Guid itemId, Prompt prompt, DateTime utcNow)
    {
        EnsureOwner(actorUserId);
        EnsureDraft();
        ArgumentNullException.ThrowIfNull(prompt);

        if (!prompt.IsPublic)
        {
            throw new DomainException(
                "Only approved prompts can be added to a pack.",
                PromptLabErrorCodes.PackItemPromptNotPublic);
        }

        if (_items.Count >= PromptLabLimits.MaxPromptPackItems)
        {
            throw new DomainException("Pack cannot contain more prompts.", PromptLabErrorCodes.PackItemInvalid);
        }

        if (_items.Any(item => item.PromptId == prompt.Id))
        {
            throw new DomainException("Prompt is already in this pack.", PromptLabErrorCodes.PackItemDuplicate);
        }

        var item = PromptPackItem.Create(itemId, Id, prompt.Id, _items.Count + 1);
        _items.Add(item);
        UpdatedAt = utcNow;
        return item;
    }

    public bool RemoveItem(Guid actorUserId, Guid promptId, DateTime utcNow)
    {
        EnsureOwner(actorUserId);
        EnsureDraft();

        var item = _items.FirstOrDefault(candidate => candidate.PromptId == promptId);
        if (item is null)
        {
            throw new DomainException("Pack item was not found.", PromptLabErrorCodes.PackItemNotFound);
        }

        _items.Remove(item);
        Renumber();
        UpdatedAt = utcNow;
        return true;
    }

    public void ReorderItem(Guid actorUserId, Guid promptId, int newOrder, DateTime utcNow)
    {
        EnsureOwner(actorUserId);
        EnsureDraft();

        if (newOrder < 1 || newOrder > _items.Count)
        {
            throw new DomainException("Pack item order is invalid.", PromptLabErrorCodes.PackItemOrderInvalid);
        }

        var item = _items.FirstOrDefault(candidate => candidate.PromptId == promptId);
        if (item is null)
        {
            throw new DomainException("Pack item was not found.", PromptLabErrorCodes.PackItemNotFound);
        }

        if (item.Order == newOrder)
        {
            return;
        }

        _items.Remove(item);
        _items.Insert(newOrder - 1, item);
        Renumber();
        UpdatedAt = utcNow;
    }

    public void Submit(Guid actorUserId, DateTime utcNow)
    {
        EnsureOwner(actorUserId);
        PromptPackWorkflowRules.EnsureAllowed(Status, PromptPackStatus.Submitted);
        Status = PromptPackStatus.Submitted;
        UpdatedAt = utcNow;
    }

    public void Approve(DateTime utcNow)
    {
        if (_items.Count == 0)
        {
            throw new DomainException("An empty pack cannot be approved.", PromptLabErrorCodes.PackEmpty);
        }

        PromptPackWorkflowRules.EnsureAllowed(Status, PromptPackStatus.Approved);
        Status = PromptPackStatus.Approved;
        PublishedAt = utcNow;
        UpdatedAt = utcNow;
        AddDomainEvent(new PromptPackApprovedDomainEvent(Id, Slug.Value));
    }

    public void Reject(DateTime utcNow)
    {
        PromptPackWorkflowRules.EnsureAllowed(Status, PromptPackStatus.Rejected);
        Status = PromptPackStatus.Rejected;
        UpdatedAt = utcNow;
    }

    public void ReturnToDraft(Guid actorUserId, DateTime utcNow)
    {
        EnsureOwner(actorUserId);
        PromptPackWorkflowRules.EnsureAllowed(Status, PromptPackStatus.Draft);
        Status = PromptPackStatus.Draft;
        UpdatedAt = utcNow;
    }

    public IReadOnlyList<PromptPackItem> GetPublicItems()
    {
        EnsurePublic();
        return Items;
    }

    public void EnsurePublic()
    {
        if (!IsPublic)
        {
            throw new DomainException(
                "Only approved packs are public.",
                PromptLabErrorCodes.PackNotPublic);
        }
    }

    private void Renumber()
    {
        for (var index = 0; index < _items.Count; index++)
        {
            _items[index].SetOrder(index + 1);
        }
    }

    private void EnsureOwner(Guid actorUserId)
    {
        if (actorUserId == Guid.Empty || actorUserId != AuthorId)
        {
            throw new DomainException(
                "Only the owner author can change this pack.",
                PromptLabErrorCodes.PackEditForbidden);
        }
    }

    private void EnsureDraft()
    {
        if (Status != PromptPackStatus.Draft)
        {
            throw new DomainException(
                "Only draft packs can be edited.",
                PromptLabErrorCodes.PackNotDraft);
        }
    }

    private bool ApplyDetails(string title, string? description, string? coverImage, bool force)
    {
        var normalizedTitle = NormalizeRequired(
            title,
            TitleMaxLength,
            PromptLabErrorCodes.PackTitleRequired,
            PromptLabErrorCodes.PackTitleInvalid);
        var normalizedDescription = NormalizeOptional(
            description,
            DescriptionMaxLength,
            PromptLabErrorCodes.PackTitleInvalid);
        var normalizedCoverImage = NormalizeOptional(
            coverImage,
            PromptLabLimits.MaxPromptCoverImageLength,
            PromptLabErrorCodes.PackCoverImageInvalid);

        var changed =
            force
            || !string.Equals(Title, normalizedTitle, StringComparison.Ordinal)
            || !string.Equals(Description, normalizedDescription, StringComparison.Ordinal)
            || !string.Equals(CoverImage, normalizedCoverImage, StringComparison.Ordinal);

        Title = normalizedTitle;
        Description = normalizedDescription;
        CoverImage = normalizedCoverImage;
        return changed;
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

using HelpDev.SharedKernel.Common;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.PromptLab.Domain.Prompts;

public sealed class PromptDefinition : AggregateRoot<Guid>
{
    public const int NameMaxLength = 150;
    public const int SlugMaxLength = 120;
    public const int SummaryMaxLength = 300;
    public const int DescriptionMaxLength = 3000;

    private readonly List<PromptVersion> _versions = [];

    private PromptDefinition()
    {
    }

    private PromptDefinition(Guid id)
        : base(id)
    {
    }

    public Guid CategoryId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public PromptSlug Slug { get; private set; } = null!;

    public string Summary { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public PromptPurpose Purpose { get; private set; }

    public PromptVisibility Visibility { get; private set; }

    public bool IsEnabled { get; private set; }

    public bool IsPublished { get; private set; }

    public bool RequiresAuthentication { get; private set; }

    public bool AllowHistory { get; private set; }

    public int DisplayOrder { get; private set; }

    public int LatestVersionNumber { get; private set; }

    public int? PublishedVersionNumber { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public DateTime? PublishedAtUtc { get; private set; }

    public IReadOnlyCollection<PromptVersion> Versions => _versions.AsReadOnly();

    public static PromptDefinition CreateDraft(
        Guid id,
        Guid categoryId,
        string name,
        string slug,
        string summary,
        string? description,
        PromptPurpose purpose,
        PromptVisibility visibility,
        bool requiresAuthentication,
        bool allowHistory,
        int displayOrder,
        DateTime utcNow)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("Prompt id must not be empty.", PromptLabErrorCodes.PromptNameInvalid);
        }

        if (categoryId == Guid.Empty)
        {
            throw new DomainException("Category id is required.", PromptLabErrorCodes.PromptCategoryInvalid);
        }

        if (!Enum.IsDefined(purpose))
        {
            throw new DomainException("Prompt purpose is invalid.", PromptLabErrorCodes.PromptNameInvalid);
        }

        if (!Enum.IsDefined(visibility))
        {
            throw new DomainException("Prompt visibility is invalid.", PromptLabErrorCodes.PromptNameInvalid);
        }

        var prompt = new PromptDefinition(id)
        {
            CategoryId = categoryId,
            Purpose = purpose,
            Visibility = visibility,
            IsPublished = false,
            IsEnabled = true,
            RequiresAuthentication = requiresAuthentication,
            AllowHistory = allowHistory,
            LatestVersionNumber = 0,
            PublishedVersionNumber = null,
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
            Slug = PromptSlug.Create(
                slug,
                SlugMaxLength,
                PromptLabErrorCodes.PromptSlugRequired,
                PromptLabErrorCodes.PromptSlugInvalid),
        };

        prompt.ApplyMetadata(name, summary, description, displayOrder, force: true);
        return prompt;
    }

    public bool UpdateMetadata(string name, string summary, string? description, DateTime utcNow)
    {
        var changed = ApplyMetadata(name, summary, description, DisplayOrder, force: false);
        if (!changed)
        {
            return false;
        }

        UpdatedAtUtc = utcNow;
        return true;
    }

    public bool ChangeCategory(Guid categoryId, DateTime utcNow)
    {
        if (categoryId == Guid.Empty)
        {
            throw new DomainException("Category id is required.", PromptLabErrorCodes.PromptCategoryInvalid);
        }

        if (CategoryId == categoryId)
        {
            return false;
        }

        CategoryId = categoryId;
        UpdatedAtUtc = utcNow;
        return true;
    }

    public bool ChangePurpose(PromptPurpose purpose, DateTime utcNow)
    {
        if (!Enum.IsDefined(purpose))
        {
            throw new DomainException("Prompt purpose is invalid.", PromptLabErrorCodes.PromptNameInvalid);
        }

        if (Purpose == purpose)
        {
            return false;
        }

        Purpose = purpose;
        UpdatedAtUtc = utcNow;
        return true;
    }

    public bool ChangeVisibility(PromptVisibility visibility, DateTime utcNow)
    {
        if (!Enum.IsDefined(visibility))
        {
            throw new DomainException("Prompt visibility is invalid.", PromptLabErrorCodes.PromptNameInvalid);
        }

        if (Visibility == visibility)
        {
            return false;
        }

        Visibility = visibility;
        UpdatedAtUtc = utcNow;
        return true;
    }

    public bool ChangeAuthenticationRequirement(bool requiresAuthentication, DateTime utcNow)
    {
        if (RequiresAuthentication == requiresAuthentication)
        {
            return false;
        }

        RequiresAuthentication = requiresAuthentication;
        UpdatedAtUtc = utcNow;
        return true;
    }

    public bool ChangeHistoryPolicy(bool allowHistory, DateTime utcNow)
    {
        if (AllowHistory == allowHistory)
        {
            return false;
        }

        AllowHistory = allowHistory;
        UpdatedAtUtc = utcNow;
        return true;
    }

    public bool ChangeDisplayOrder(int displayOrder, DateTime utcNow)
    {
        if (displayOrder < 0)
        {
            throw new DomainException("Display order must be >= 0.", PromptLabErrorCodes.PromptNameInvalid);
        }

        if (DisplayOrder == displayOrder)
        {
            return false;
        }

        DisplayOrder = displayOrder;
        UpdatedAtUtc = utcNow;
        return true;
    }

    public bool Enable(DateTime utcNow)
    {
        if (IsEnabled)
        {
            return false;
        }

        IsEnabled = true;
        UpdatedAtUtc = utcNow;
        return true;
    }

    public bool Disable(DateTime utcNow)
    {
        if (!IsEnabled)
        {
            return false;
        }

        IsEnabled = false;
        UpdatedAtUtc = utcNow;
        return true;
    }

    public PromptVersion RegisterVersion(
        Guid versionId,
        string template,
        string? changeNotes,
        Guid? createdByUserId,
        IReadOnlyList<PromptVariable> variables,
        IReadOnlyList<string> placeholderNames,
        DateTime utcNow)
    {
        var versionNumber = LatestVersionNumber + 1;
        var version = PromptVersion.Create(
            versionId,
            Id,
            versionNumber,
            template,
            changeNotes,
            createdByUserId,
            variables,
            placeholderNames,
            utcNow);

        _versions.Add(version);
        LatestVersionNumber = versionNumber;
        UpdatedAtUtc = utcNow;
        return version;
    }

    public bool PublishVersion(int versionNumber, DateTime utcNow)
    {
        if (!IsEnabled)
        {
            throw new DomainException(
                "Disabled prompts cannot be published.",
                PromptLabErrorCodes.PromptCannotPublish);
        }

        var version = GetVersion(versionNumber);
        if (string.IsNullOrWhiteSpace(version.Template))
        {
            throw new DomainException(
                "Prompt cannot be published.",
                PromptLabErrorCodes.PromptCannotPublish);
        }

        if (IsPublished && PublishedVersionNumber == versionNumber)
        {
            return false;
        }

        IsPublished = true;
        PublishedVersionNumber = versionNumber;
        PublishedAtUtc ??= utcNow;
        UpdatedAtUtc = utcNow;
        AddDomainEvent(new PromptPublishedDomainEvent(Id, Slug.Value, versionNumber));
        return true;
    }

    public bool Unpublish(DateTime utcNow)
    {
        if (!IsPublished)
        {
            return false;
        }

        IsPublished = false;
        PublishedVersionNumber = null;
        UpdatedAtUtc = utcNow;
        AddDomainEvent(new PromptUnpublishedDomainEvent(Id, Slug.Value));
        return true;
    }

    public PromptVersion GetVersion(int versionNumber)
    {
        var version = _versions.FirstOrDefault(v => v.VersionNumber == versionNumber);
        if (version is null)
        {
            throw new DomainException("Prompt version was not found.", PromptLabErrorCodes.PromptVersionNotFound);
        }

        return version;
    }

    public PromptVersion GetPublishedVersion()
    {
        if (PublishedVersionNumber is null)
        {
            throw new DomainException("Prompt is unpublished.", PromptLabErrorCodes.PromptUnpublished);
        }

        return GetVersion(PublishedVersionNumber.Value);
    }

    public void EnsureRenderable()
    {
        if (!IsPublished)
        {
            throw new DomainException("Prompt was not found.", PromptLabErrorCodes.PromptNotFound);
        }

        if (!IsEnabled)
        {
            throw new DomainException("Prompt is disabled.", PromptLabErrorCodes.PromptDisabled);
        }
    }

    private bool ApplyMetadata(string name, string summary, string? description, int displayOrder, bool force)
    {
        if (displayOrder < 0)
        {
            throw new DomainException("Display order must be >= 0.", PromptLabErrorCodes.PromptNameInvalid);
        }

        var normalizedName = NormalizeRequired(
            name,
            NameMaxLength,
            PromptLabErrorCodes.PromptNameRequired,
            PromptLabErrorCodes.PromptNameInvalid);
        var normalizedSummary = NormalizeRequired(
            summary,
            SummaryMaxLength,
            PromptLabErrorCodes.PromptSummaryRequired,
            PromptLabErrorCodes.PromptSummaryInvalid);
        var normalizedDescription = NormalizeOptional(
            description,
            DescriptionMaxLength,
            PromptLabErrorCodes.PromptSummaryInvalid);

        var changed =
            force
            || !string.Equals(Name, normalizedName, StringComparison.Ordinal)
            || !string.Equals(Summary, normalizedSummary, StringComparison.Ordinal)
            || !string.Equals(Description, normalizedDescription, StringComparison.Ordinal)
            || DisplayOrder != displayOrder;

        Name = normalizedName;
        Summary = normalizedSummary;
        Description = normalizedDescription;
        DisplayOrder = displayOrder;
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

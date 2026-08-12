using System.Text.Json;
using HelpDev.SharedKernel.Common;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.Toolbox.Domain.Tools;

public sealed class ToolDefinition : AggregateRoot<Guid>
{
    public const int NameMaxLength = 150;
    public const int SlugMaxLength = 120;
    public const int SummaryMaxLength = 300;
    public const int DescriptionMaxLength = 3000;
    public const int SchemaMaxLength = 8000;

    private ToolDefinition()
    {
    }

    private ToolDefinition(Guid id)
        : base(id)
    {
    }

    public Guid CategoryId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public ToolSlug Slug { get; private set; } = null!;

    public string Summary { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public ToolType Type { get; private set; }

    public string InputSchema { get; private set; } = string.Empty;

    public string? ExampleInput { get; private set; }

    public bool IsPublished { get; private set; }

    public bool IsEnabled { get; private set; }

    public bool RequiresAuthentication { get; private set; }

    public bool AllowHistory { get; private set; }

    public int DisplayOrder { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public DateTime? PublishedAtUtc { get; private set; }

    public static ToolDefinition CreateDraft(
        Guid id,
        Guid categoryId,
        string name,
        string slug,
        string summary,
        string? description,
        ToolType type,
        string inputSchema,
        string? exampleInput,
        bool requiresAuthentication,
        bool allowHistory,
        int displayOrder,
        DateTime utcNow)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("Tool id must not be empty.", ToolboxErrorCodes.ToolNameInvalid);
        }

        if (categoryId == Guid.Empty)
        {
            throw new DomainException("Category id is required.", ToolboxErrorCodes.ToolCategoryInvalid);
        }

        if (!Enum.IsDefined(type))
        {
            throw new DomainException("Tool type is invalid.", ToolboxErrorCodes.ToolTypeInvalid);
        }

        var tool = new ToolDefinition(id)
        {
            CategoryId = categoryId,
            Type = type,
            IsPublished = false,
            IsEnabled = true,
            RequiresAuthentication = requiresAuthentication,
            AllowHistory = allowHistory,
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
            Slug = ToolSlug.Create(
                slug,
                SlugMaxLength,
                ToolboxErrorCodes.ToolSlugRequired,
                ToolboxErrorCodes.ToolSlugInvalid),
        };

        tool.ApplyDetails(name, summary, description, displayOrder, force: true);
        tool.ApplySchema(inputSchema, exampleInput, force: true);
        return tool;
    }

    public bool UpdateDetails(
        string name,
        string summary,
        string? description,
        bool requiresAuthentication,
        bool allowHistory,
        int displayOrder,
        DateTime utcNow)
    {
        var changed = ApplyDetails(name, summary, description, displayOrder, force: false);
        if (RequiresAuthentication != requiresAuthentication)
        {
            RequiresAuthentication = requiresAuthentication;
            changed = true;
        }

        if (AllowHistory != allowHistory)
        {
            AllowHistory = allowHistory;
            changed = true;
        }

        if (!changed)
        {
            return false;
        }

        UpdatedAtUtc = utcNow;
        return true;
    }

    public bool UpdateInputSchema(string inputSchema, string? exampleInput, DateTime utcNow)
    {
        var changed = ApplySchema(inputSchema, exampleInput, force: false);
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
            throw new DomainException("Category id is required.", ToolboxErrorCodes.ToolCategoryInvalid);
        }

        if (CategoryId == categoryId)
        {
            return false;
        }

        CategoryId = categoryId;
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

    public bool Publish(DateTime utcNow)
    {
        if (IsPublished)
        {
            return false;
        }

        EnsureCanPublish();
        IsPublished = true;
        PublishedAtUtc ??= utcNow;
        UpdatedAtUtc = utcNow;
        AddDomainEvent(new ToolPublishedDomainEvent(Id, Slug.Value));
        return true;
    }

    public bool Unpublish(DateTime utcNow)
    {
        if (!IsPublished)
        {
            return false;
        }

        IsPublished = false;
        UpdatedAtUtc = utcNow;
        AddDomainEvent(new ToolUnpublishedDomainEvent(Id, Slug.Value));
        return true;
    }

    public bool ChangeDisplayOrder(int displayOrder, DateTime utcNow)
    {
        if (displayOrder < 0)
        {
            throw new DomainException("Display order must be >= 0.", ToolboxErrorCodes.ToolNameInvalid);
        }

        if (DisplayOrder == displayOrder)
        {
            return false;
        }

        DisplayOrder = displayOrder;
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

    public void EnsureExecutable()
    {
        if (!IsPublished)
        {
            throw new DomainException("Tool was not found.", ToolboxErrorCodes.ToolNotFound);
        }

        if (!IsEnabled)
        {
            throw new DomainException("Tool is disabled.", ToolboxErrorCodes.ToolDisabled);
        }
    }

    private void EnsureCanPublish()
    {
        if (!IsEnabled)
        {
            throw new DomainException("Disabled tools cannot be published.", ToolboxErrorCodes.ToolCannotPublish);
        }

        if (string.IsNullOrWhiteSpace(Name)
            || string.IsNullOrWhiteSpace(Summary)
            || string.IsNullOrWhiteSpace(InputSchema)
            || !Enum.IsDefined(Type))
        {
            throw new DomainException("Tool cannot be published.", ToolboxErrorCodes.ToolCannotPublish);
        }
    }

    private bool ApplyDetails(string name, string summary, string? description, int displayOrder, bool force)
    {
        if (displayOrder < 0)
        {
            throw new DomainException("Display order must be >= 0.", ToolboxErrorCodes.ToolNameInvalid);
        }

        var normalizedName = NormalizeRequired(name, NameMaxLength, ToolboxErrorCodes.ToolNameRequired, ToolboxErrorCodes.ToolNameInvalid);
        var normalizedSummary = NormalizeRequired(summary, SummaryMaxLength, ToolboxErrorCodes.ToolSummaryRequired, ToolboxErrorCodes.ToolSummaryInvalid);
        var normalizedDescription = NormalizeOptional(description, DescriptionMaxLength, ToolboxErrorCodes.ToolSummaryInvalid);

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

    private bool ApplySchema(string inputSchema, string? exampleInput, bool force)
    {
        var normalizedSchema = NormalizeJson(inputSchema, required: true);
        var normalizedExample = exampleInput is null
            ? null
            : NormalizeJson(exampleInput, required: false);

        var changed =
            force
            || !string.Equals(InputSchema, normalizedSchema, StringComparison.Ordinal)
            || !string.Equals(ExampleInput, normalizedExample, StringComparison.Ordinal);

        InputSchema = normalizedSchema;
        ExampleInput = normalizedExample;
        return changed;
    }

    private static string NormalizeJson(string value, bool required)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            if (required)
            {
                throw new DomainException("Input schema is required.", ToolboxErrorCodes.ToolSchemaInvalid);
            }

            throw new DomainException("Example input is invalid.", ToolboxErrorCodes.ToolSchemaInvalid);
        }

        var trimmed = value.Trim();
        if (trimmed.Length > SchemaMaxLength)
        {
            throw new DomainException("JSON payload is too large.", ToolboxErrorCodes.ToolSchemaInvalid);
        }

        if (trimmed.Contains("http://", StringComparison.OrdinalIgnoreCase)
            || trimmed.Contains("https://", StringComparison.OrdinalIgnoreCase)
            || trimmed.Contains("javascript:", StringComparison.OrdinalIgnoreCase)
            || trimmed.Contains("eval(", StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException("Schema must not contain executable or remote references.", ToolboxErrorCodes.ToolSchemaInvalid);
        }

        try
        {
            using var document = JsonDocument.Parse(trimmed);
            return document.RootElement.GetRawText();
        }
        catch (JsonException)
        {
            throw new DomainException("Schema must be valid JSON.", ToolboxErrorCodes.ToolSchemaInvalid);
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

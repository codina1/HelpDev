using HelpDev.Modules.Toolbox.Domain.Tools;
using HelpDev.SharedKernel.Common;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.Toolbox.Domain.Categories;

public sealed class ToolCategory : AggregateRoot<Guid>
{
    public const int NameMaxLength = 100;
    public const int SlugMaxLength = 100;
    public const int DescriptionMaxLength = 500;
    public const int IconMaxLength = 100;

    private ToolCategory()
    {
    }

    private ToolCategory(Guid id)
        : base(id)
    {
    }

    public string Name { get; private set; } = string.Empty;

    public ToolSlug Slug { get; private set; } = null!;

    public string? Description { get; private set; }

    public string? Icon { get; private set; }

    public int DisplayOrder { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public static ToolCategory Create(
        Guid id,
        string name,
        string slug,
        string? description,
        string? icon,
        int displayOrder,
        DateTime utcNow)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("Category id must not be empty.", ToolboxErrorCodes.CategoryNameInvalid);
        }

        var category = new ToolCategory(id)
        {
            IsActive = true,
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
            Slug = ToolSlug.Create(
                slug,
                SlugMaxLength,
                ToolboxErrorCodes.CategorySlugRequired,
                ToolboxErrorCodes.CategorySlugInvalid),
        };

        category.ApplyDetails(name, description, icon, displayOrder, force: true);
        return category;
    }

    public bool UpdateDetails(string name, string? description, string? icon, int displayOrder, DateTime utcNow)
    {
        var changed = ApplyDetails(name, description, icon, displayOrder, force: false);
        if (!changed)
        {
            return false;
        }

        UpdatedAtUtc = utcNow;
        return true;
    }

    public bool Activate(DateTime utcNow)
    {
        if (IsActive)
        {
            return false;
        }

        IsActive = true;
        UpdatedAtUtc = utcNow;
        return true;
    }

    public bool Deactivate(DateTime utcNow)
    {
        if (!IsActive)
        {
            return false;
        }

        IsActive = false;
        UpdatedAtUtc = utcNow;
        return true;
    }

    public bool ChangeDisplayOrder(int displayOrder, DateTime utcNow)
    {
        if (displayOrder < 0)
        {
            throw new DomainException("Display order must be >= 0.", ToolboxErrorCodes.CategoryNameInvalid);
        }

        if (DisplayOrder == displayOrder)
        {
            return false;
        }

        DisplayOrder = displayOrder;
        UpdatedAtUtc = utcNow;
        return true;
    }

    private bool ApplyDetails(string name, string? description, string? icon, int displayOrder, bool force)
    {
        if (displayOrder < 0)
        {
            throw new DomainException("Display order must be >= 0.", ToolboxErrorCodes.CategoryNameInvalid);
        }

        var normalizedName = NormalizeName(name);
        var normalizedDescription = NormalizeOptional(description, DescriptionMaxLength, ToolboxErrorCodes.CategoryNameInvalid);
        var normalizedIcon = NormalizeIcon(icon);

        var changed =
            force
            || !string.Equals(Name, normalizedName, StringComparison.Ordinal)
            || !string.Equals(Description, normalizedDescription, StringComparison.Ordinal)
            || !string.Equals(Icon, normalizedIcon, StringComparison.Ordinal)
            || DisplayOrder != displayOrder;

        Name = normalizedName;
        Description = normalizedDescription;
        Icon = normalizedIcon;
        DisplayOrder = displayOrder;
        return changed;
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Category name is required.", ToolboxErrorCodes.CategoryNameRequired);
        }

        var trimmed = name.Trim();
        if (trimmed.Length > NameMaxLength)
        {
            throw new DomainException("Category name is invalid.", ToolboxErrorCodes.CategoryNameInvalid);
        }

        return trimmed;
    }

    private static string? NormalizeIcon(string? icon)
    {
        if (icon is null)
        {
            return null;
        }

        var trimmed = icon.Trim();
        if (trimmed.Length == 0)
        {
            return null;
        }

        if (trimmed.Length > IconMaxLength
            || trimmed.Contains('<', StringComparison.Ordinal)
            || trimmed.Contains('>', StringComparison.Ordinal)
            || trimmed.Contains('{', StringComparison.Ordinal))
        {
            throw new DomainException("Category icon must be a safe icon key.", ToolboxErrorCodes.CategoryNameInvalid);
        }

        return trimmed;
    }

    private static string? NormalizeOptional(string? value, int maxLength, string code)
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
            throw new DomainException("Value is invalid.", code);
        }

        return trimmed;
    }
}

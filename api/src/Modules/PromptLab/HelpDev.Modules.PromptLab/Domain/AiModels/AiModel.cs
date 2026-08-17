using HelpDev.Modules.PromptLab.Domain.Prompts;
using HelpDev.SharedKernel.Common;
using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.PromptLab.Domain.AiModels;

public sealed class AiModel : AggregateRoot<Guid>
{
    public const int NameMaxLength = 100;
    public const int SlugMaxLength = 100;
    public const int ProviderMaxLength = 80;
    public const int LogoMaxLength = 100;

    private AiModel()
    {
    }

    private AiModel(Guid id)
        : base(id)
    {
    }

    public string Name { get; private set; } = string.Empty;

    public PromptSlug Slug { get; private set; } = null!;

    public string Provider { get; private set; } = string.Empty;

    public string? Logo { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public static AiModel Create(
        Guid id,
        string name,
        string slug,
        string provider,
        string? logo,
        DateTime utcNow)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("AI model id must not be empty.", PromptLabErrorCodes.AiModelNameInvalid);
        }

        var model = new AiModel(id)
        {
            IsActive = true,
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
            Slug = PromptSlug.Create(
                slug,
                SlugMaxLength,
                PromptLabErrorCodes.AiModelSlugRequired,
                PromptLabErrorCodes.AiModelSlugInvalid),
        };

        model.ApplyDetails(name, provider, logo, force: true);
        return model;
    }

    public bool UpdateDetails(string name, string provider, string? logo, DateTime utcNow)
    {
        var changed = ApplyDetails(name, provider, logo, force: false);
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

    public void EnsureActive()
    {
        if (!IsActive)
        {
            throw new DomainException("AI model is inactive.", PromptLabErrorCodes.AiModelInactive);
        }
    }

    private bool ApplyDetails(string name, string provider, string? logo, bool force)
    {
        var normalizedName = NormalizeRequired(
            name,
            NameMaxLength,
            PromptLabErrorCodes.AiModelNameRequired,
            PromptLabErrorCodes.AiModelNameInvalid);
        var normalizedProvider = NormalizeRequired(
            provider,
            ProviderMaxLength,
            PromptLabErrorCodes.AiModelProviderRequired,
            PromptLabErrorCodes.AiModelProviderInvalid);
        var normalizedLogo = NormalizeLogo(logo);

        var changed =
            force
            || !string.Equals(Name, normalizedName, StringComparison.Ordinal)
            || !string.Equals(Provider, normalizedProvider, StringComparison.Ordinal)
            || !string.Equals(Logo, normalizedLogo, StringComparison.Ordinal);

        Name = normalizedName;
        Provider = normalizedProvider;
        Logo = normalizedLogo;
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

    private static string? NormalizeLogo(string? logo)
    {
        if (logo is null)
        {
            return null;
        }

        var trimmed = logo.Trim();
        if (trimmed.Length == 0)
        {
            return null;
        }

        if (trimmed.Length > LogoMaxLength
            || trimmed.Contains('<', StringComparison.Ordinal)
            || trimmed.Contains('>', StringComparison.Ordinal)
            || trimmed.Contains('{', StringComparison.Ordinal)
            || trimmed.Contains('}', StringComparison.Ordinal))
        {
            throw new DomainException("AI model logo must be a safe logo key.", PromptLabErrorCodes.AiModelLogoInvalid);
        }

        return trimmed;
    }
}

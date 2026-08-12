using HelpDev.SharedKernel.Exceptions;

namespace HelpDev.Modules.Content.Domain.Tools;

/// <summary>
/// Tool-specific metadata satellite. Content owns lifecycle; this entity stores
/// catalog fields (name, URLs, pricing, platforms, license). No EF in Domain.
/// </summary>
public sealed class ToolMetadata
{
    public const int MaxToolNameLength = 200;
    public const int MaxUrlLength = 2048;
    public const int MaxCompanyNameLength = 200;
    public const int MaxToolCategoryLength = 120;

    private readonly List<ToolFeature> _features = [];
    private readonly List<ToolAlternative> _alternatives = [];

    private ToolMetadata()
    {
    }

    private ToolMetadata(
        Guid id,
        Guid contentId,
        string toolName,
        string officialWebsiteUrl,
        string? githubUrl,
        Guid? logoMediaId,
        string? companyName,
        PricingModel pricingModel,
        string toolCategory,
        PlatformSupport platformSupport,
        LicenseType licenseType,
        DateTime createdAtUtc,
        DateTime updatedAtUtc)
    {
        Id = id;
        ContentId = contentId;
        ToolName = toolName;
        OfficialWebsiteUrl = officialWebsiteUrl;
        GithubUrl = githubUrl;
        LogoMediaId = logoMediaId;
        CompanyName = companyName;
        PricingModel = pricingModel;
        ToolCategory = toolCategory;
        PlatformSupport = platformSupport;
        LicenseType = licenseType;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }

    public Guid Id { get; private set; }

    public Guid ContentId { get; private set; }

    public string ToolName { get; private set; } = string.Empty;

    public string OfficialWebsiteUrl { get; private set; } = string.Empty;

    public string? GithubUrl { get; private set; }

    public Guid? LogoMediaId { get; private set; }

    public string? CompanyName { get; private set; }

    public PricingModel PricingModel { get; private set; }

    public string ToolCategory { get; private set; } = string.Empty;

    public PlatformSupport PlatformSupport { get; private set; }

    public LicenseType LicenseType { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyList<ToolFeature> Features => _features.AsReadOnly();

    public IReadOnlyList<ToolAlternative> Alternatives => _alternatives.AsReadOnly();

    public static ToolMetadata Create(
        Guid id,
        Guid contentId,
        string toolName,
        string officialWebsiteUrl,
        string? githubUrl,
        Guid? logoMediaId,
        string? companyName,
        PricingModel pricingModel,
        string toolCategory,
        PlatformSupport platformSupport,
        LicenseType licenseType,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("شناسه متادیتای ابزار الزامی است.");
        }

        if (contentId == Guid.Empty)
        {
            throw new DomainException("شناسه محتوا الزامی است.");
        }

        EnsureDefined(pricingModel, "مدل قیمت‌گذاری");
        EnsureDefined(licenseType, "نوع لایسنس");

        return new ToolMetadata(
            id,
            contentId,
            NormalizeRequiredName(toolName),
            NormalizeRequiredUrl(officialWebsiteUrl, "وب‌سایت رسمی"),
            NormalizeOptionalUrl(githubUrl, "آدرس GitHub"),
            NormalizeMediaId(logoMediaId),
            NormalizeOptionalLength(companyName, MaxCompanyNameLength, "نام شرکت"),
            pricingModel,
            NormalizeRequiredCategory(toolCategory),
            platformSupport,
            licenseType,
            createdAtUtc,
            createdAtUtc);
    }

    public void Update(
        string toolName,
        string officialWebsiteUrl,
        string? githubUrl,
        Guid? logoMediaId,
        string? companyName,
        PricingModel pricingModel,
        string toolCategory,
        PlatformSupport platformSupport,
        LicenseType licenseType,
        DateTime updatedAtUtc)
    {
        EnsureDefined(pricingModel, "مدل قیمت‌گذاری");
        EnsureDefined(licenseType, "نوع لایسنس");

        ToolName = NormalizeRequiredName(toolName);
        OfficialWebsiteUrl = NormalizeRequiredUrl(officialWebsiteUrl, "وب‌سایت رسمی");
        GithubUrl = NormalizeOptionalUrl(githubUrl, "آدرس GitHub");
        LogoMediaId = NormalizeMediaId(logoMediaId);
        CompanyName = NormalizeOptionalLength(companyName, MaxCompanyNameLength, "نام شرکت");
        PricingModel = pricingModel;
        ToolCategory = NormalizeRequiredCategory(toolCategory);
        PlatformSupport = platformSupport;
        LicenseType = licenseType;
        UpdatedAtUtc = updatedAtUtc;
    }

    public ToolFeature AddFeature(Guid featureId, string title, string? description, int order, DateTime updatedAtUtc)
    {
        var feature = ToolFeature.Create(featureId, Id, title, description, order);
        _features.Add(feature);
        UpdatedAtUtc = updatedAtUtc;
        return feature;
    }

    public void RemoveFeature(Guid featureId, DateTime updatedAtUtc)
    {
        var removed = _features.RemoveAll(f => f.Id == featureId);
        if (removed == 0)
        {
            throw new DomainException("ویژگی ابزار یافت نشد.");
        }

        UpdatedAtUtc = updatedAtUtc;
    }

    public void ReplaceAlternatives(IEnumerable<ToolAlternative> alternatives, DateTime updatedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(alternatives);
        _alternatives.Clear();
        foreach (var alternative in alternatives)
        {
            if (alternative.ToolId != Id)
            {
                throw new DomainException("جایگزین به ابزار دیگری تعلق دارد.");
            }

            if (alternative.AlternativeToolContentId == ContentId)
            {
                throw new DomainException("ابزار نمی‌تواند جایگزین خودش باشد.");
            }

            _alternatives.Add(alternative);
        }

        UpdatedAtUtc = updatedAtUtc;
    }

    private static void EnsureDefined<TEnum>(TEnum value, string fieldName)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new DomainException($"{fieldName} معتبر نیست.");
        }
    }

    private static string NormalizeRequiredName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("نام ابزار الزامی است.");
        }

        var normalized = value.Trim();
        if (normalized.Length > MaxToolNameLength)
        {
            throw new DomainException("نام ابزار بیش از حد مجاز است.");
        }

        return normalized;
    }

    private static string NormalizeRequiredCategory(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("دسته ابزار الزامی است.");
        }

        var normalized = value.Trim();
        if (normalized.Length > MaxToolCategoryLength)
        {
            throw new DomainException("دسته ابزار بیش از حد مجاز است.");
        }

        return normalized;
    }

    private static string NormalizeRequiredUrl(string? value, string fieldName)
    {
        var normalized = NormalizeOptionalUrl(value, fieldName);
        if (normalized is null)
        {
            throw new DomainException($"{fieldName} الزامی است.");
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
        if (normalized.Length > MaxUrlLength)
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

    private static Guid? NormalizeMediaId(Guid? logoMediaId)
    {
        if (logoMediaId == Guid.Empty)
        {
            throw new DomainException("شناسه لوگوی رسانه معتبر نیست.");
        }

        return logoMediaId;
    }
}

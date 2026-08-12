namespace HelpDev.Modules.Content.Application.Tools.Dtos;

public sealed record ToolFeatureDto(
    Guid Id,
    string Title,
    string? Description,
    int Order);

public sealed record ToolAlternativeDto(
    Guid Id,
    Guid AlternativeToolContentId,
    string? AlternativeToolName,
    string? AlternativeToolSlug,
    int Order);

public sealed record ToolDetailDto(
    Guid Id,
    Guid ContentId,
    string ToolName,
    string OfficialWebsiteUrl,
    string? GithubUrl,
    Guid? LogoMediaId,
    string? CompanyName,
    string PricingModel,
    string ToolCategory,
    IReadOnlyList<string> Platforms,
    string LicenseType,
    IReadOnlyList<ToolFeatureDto> Features,
    IReadOnlyList<ToolAlternativeDto> Alternatives,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record ToolListItemDto(
    Guid Id,
    Guid ContentId,
    string ToolName,
    string ToolCategory,
    string PricingModel,
    string LicenseType,
    string ContentSlug,
    string ContentStatus,
    DateTime UpdatedAtUtc);

public sealed class UpdateToolRequest
{
    public string ToolName { get; set; } = string.Empty;

    public string OfficialWebsiteUrl { get; set; } = string.Empty;

    public string? GithubUrl { get; set; }

    public Guid? LogoMediaId { get; set; }

    public string? CompanyName { get; set; }

    public string PricingModel { get; set; } = nameof(Domain.Tools.PricingModel.Free);

    public string ToolCategory { get; set; } = string.Empty;

    public IReadOnlyList<string> Platforms { get; set; } = [];

    public string LicenseType { get; set; } = nameof(Domain.Tools.LicenseType.Commercial);

    public IReadOnlyList<UpdateToolAlternativeItem>? Alternatives { get; set; }
}

public sealed class UpdateToolAlternativeItem
{
    public Guid AlternativeToolContentId { get; set; }

    public int Order { get; set; }
}

public sealed class CreateToolFeatureRequest
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int? Order { get; set; }
}

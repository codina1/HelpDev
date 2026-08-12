using HelpDev.Modules.Content.Application.Tools.Dtos;
using HelpDev.Modules.Content.Domain.Tools;

namespace HelpDev.Modules.Content.Application.Tools;

internal static class ToolMapper
{
    public static ToolDetailDto ToDetail(
        ToolMetadata tool,
        IReadOnlyDictionary<Guid, (string Name, string Slug)>? alternativeLookup = null) =>
        new(
            tool.Id,
            tool.ContentId,
            tool.ToolName,
            tool.OfficialWebsiteUrl,
            tool.GithubUrl,
            tool.LogoMediaId,
            tool.CompanyName,
            tool.PricingModel.ToString(),
            tool.ToolCategory,
            ExpandPlatforms(tool.PlatformSupport),
            tool.LicenseType.ToString(),
            tool.Features.OrderBy(f => f.Order).Select(ToFeature).ToArray(),
            tool.Alternatives
                .OrderBy(a => a.Order)
                .Select(a =>
                {
                    string? name = null;
                    string? slug = null;
                    if (alternativeLookup is not null
                        && alternativeLookup.TryGetValue(a.AlternativeToolContentId, out var info))
                    {
                        name = info.Name;
                        slug = info.Slug;
                    }

                    return new ToolAlternativeDto(
                        a.Id,
                        a.AlternativeToolContentId,
                        name,
                        slug,
                        a.Order);
                })
                .ToArray(),
            tool.CreatedAtUtc,
            tool.UpdatedAtUtc);

    public static ToolFeatureDto ToFeature(ToolFeature feature) =>
        new(feature.Id, feature.Title, feature.Description, feature.Order);

    public static IReadOnlyList<string> ExpandPlatforms(PlatformSupport platforms)
    {
        var list = new List<string>(4);
        if (platforms.HasFlag(PlatformSupport.Windows)) list.Add(nameof(PlatformSupport.Windows));
        if (platforms.HasFlag(PlatformSupport.Linux)) list.Add(nameof(PlatformSupport.Linux));
        if (platforms.HasFlag(PlatformSupport.MacOS)) list.Add(nameof(PlatformSupport.MacOS));
        if (platforms.HasFlag(PlatformSupport.Web)) list.Add(nameof(PlatformSupport.Web));
        return list;
    }
}

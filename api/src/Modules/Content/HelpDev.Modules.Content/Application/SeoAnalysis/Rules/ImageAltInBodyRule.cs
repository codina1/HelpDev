namespace HelpDev.Modules.Content.Application.SeoAnalysis.Rules;

/// <summary>Reports markdown images that lack non-empty alt text when images exist in the body.</summary>
public sealed class ImageAltInBodyRule : ISeoAnalysisRule
{
    public string RuleId => "seo.media.image_alt";

    public IReadOnlyList<SeoAnalysisFindingDto> Analyze(SeoAnalysisContext context)
    {
        var images = context.Facts.Images;
        if (images.Count == 0)
        {
            return [];
        }

        var missingAlt = images.Count(img => string.IsNullOrWhiteSpace(img.AltText));
        if (missingAlt == 0)
        {
            return
            [
                new SeoAnalysisFindingDto(
                    RuleId,
                    SeoFindingCategory.Media,
                    SeoFindingSeverity.Info,
                    Passed: true,
                    "متن جایگزین تصویر",
                    "همهٔ تصاویر markdown متن جایگزین دارند.",
                    $"image_count={images.Count}",
                    null),
            ];
        }

        return
        [
            new SeoAnalysisFindingDto(
                RuleId,
                SeoFindingCategory.Media,
                SeoFindingSeverity.Warning,
                Passed: false,
                "متن جایگزین تصویر",
                $"{missingAlt} از {images.Count} تصویر markdown بدون alt هستند.",
                $"missing_alt={missingAlt}; total={images.Count}",
                "برای هر تصویر در markdown متن جایگزین (alt) بنویسید."),
        ];
    }
}

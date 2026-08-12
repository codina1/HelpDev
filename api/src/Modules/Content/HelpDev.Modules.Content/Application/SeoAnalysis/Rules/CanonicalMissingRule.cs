namespace HelpDev.Modules.Content.Application.SeoAnalysis.Rules;

/// <summary>Surfaces missing canonical URL as an explicit checklist item (optional field).</summary>
public sealed class CanonicalMissingRule : ISeoAnalysisRule
{
    public string RuleId => "seo.canonical.missing";

    public IReadOnlyList<SeoAnalysisFindingDto> Analyze(SeoAnalysisContext context)
    {
        if (!string.IsNullOrWhiteSpace(context.CanonicalUrl))
        {
            return [];
        }

        return
        [
            new SeoAnalysisFindingDto(
                RuleId,
                SeoFindingCategory.Url,
                SeoFindingSeverity.Warning,
                Passed: false,
                "نشانی کانونیکال",
                "نشانی کانونیکال تنظیم نشده است.",
                null,
                "در صورت نیاز یک URL مطلق http(s) برای نسخهٔ کاننیکال تنظیم کنید."),
        ];
    }
}

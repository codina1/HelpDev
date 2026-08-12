namespace HelpDev.Modules.Content.Application.SeoAnalysis.Rules;

/// <summary>Warns when saved body has text but no internal/relative links.</summary>
public sealed class InternalLinksPresenceRule : ISeoAnalysisRule
{
    public string RuleId => "seo.link.no_internal";

    public IReadOnlyList<SeoAnalysisFindingDto> Analyze(SeoAnalysisContext context)
    {
        if (context.Facts.WordCount == 0)
        {
            return [];
        }

        var internalCount = 0;
        foreach (var link in context.Facts.Links)
        {
            var href = link.Href.Trim();
            if (href.Length == 0)
            {
                continue;
            }

            if (href.StartsWith('#')
                || href.StartsWith('/')
                || href.StartsWith("./", StringComparison.Ordinal)
                || href.StartsWith("../", StringComparison.Ordinal)
                || !Uri.TryCreate(href, UriKind.Absolute, out _))
            {
                internalCount++;
            }
        }

        if (internalCount > 0)
        {
            return
            [
                new SeoAnalysisFindingDto(
                    RuleId,
                    SeoFindingCategory.Links,
                    SeoFindingSeverity.Info,
                    Passed: true,
                    "پیوندهای داخلی",
                    "حداقل یک پیوند نسبی یا داخلی در متن وجود دارد.",
                    $"internal_count={internalCount}",
                    null),
            ];
        }

        return
        [
            new SeoAnalysisFindingDto(
                RuleId,
                SeoFindingCategory.Links,
                SeoFindingSeverity.Warning,
                Passed: false,
                "پیوندهای داخلی",
                "هیچ پیوند داخلی یا نسبی در متن ذخیره‌شده یافت نشد.",
                null,
                "در صورت امکان به محتوای مرتبط در همین سایت لینک دهید."),
        ];
    }
}

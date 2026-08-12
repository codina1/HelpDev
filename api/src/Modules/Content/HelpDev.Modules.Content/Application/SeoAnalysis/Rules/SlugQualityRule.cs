using HelpDev.Modules.Content.Application.SeoAnalysis.Markdown;

namespace HelpDev.Modules.Content.Application.SeoAnalysis.Rules;

public sealed class SlugQualityRule : ISeoAnalysisRule
{
    public string RuleId => "seo.slug.quality";

    public IReadOnlyList<SeoAnalysisFindingDto> Analyze(SeoAnalysisContext context)
    {
        var slug = context.Slug;
        if (slug.Length == 0)
        {
            return
            [
                new SeoAnalysisFindingDto(
                    RuleId,
                    SeoFindingCategory.Url,
                    SeoFindingSeverity.Error,
                    Passed: false,
                    "اسلاگ",
                    "اسلاگ خالی است.",
                    null,
                    "یک اسلاگ معتبر برای مسیر محتوا تنظیم کنید."),
            ];
        }

        var issues = new List<string>();
        if (slug.Contains(' ', StringComparison.Ordinal) || slug.Contains('\t', StringComparison.Ordinal))
        {
            issues.Add("whitespace");
        }

        if (slug.Contains('?', StringComparison.Ordinal) || slug.Contains('#', StringComparison.Ordinal))
        {
            issues.Add("query_or_fragment");
        }

        if (slug.Contains("//", StringComparison.Ordinal))
        {
            issues.Add("double_slash");
        }

        if (slug.Length < SeoAnalysisOptions.RecommendedSlugMin)
        {
            issues.Add("too_short");
        }
        else if (slug.Length > SeoAnalysisOptions.RecommendedSlugMax)
        {
            issues.Add("too_long");
        }

        var keywordOk = !context.HasFocusKeyword
            || MarkdownDocumentScanner.ContainsKeyword(slug, context.FocusKeyword);
        if (context.HasFocusKeyword && !keywordOk)
        {
            issues.Add("keyword_missing");
        }

        var hardFail = issues.Contains("whitespace")
            || issues.Contains("query_or_fragment")
            || issues.Contains("double_slash");

        var passed = issues.Count == 0;
        return
        [
            new SeoAnalysisFindingDto(
                RuleId,
                SeoFindingCategory.Url,
                passed
                    ? SeoFindingSeverity.Info
                    : hardFail
                        ? SeoFindingSeverity.Error
                        : SeoFindingSeverity.Warning,
                passed,
                "اسلاگ",
                passed
                    ? "اسلاگ از نظر ساختار و طول توصیه‌شده مناسب است."
                    : $"اسلاگ نیاز به توجه دارد: {string.Join(", ", issues)}.",
                SeoAnalysisContext.CapEvidence(slug),
                passed
                    ? null
                    : "اسلاگ را بدون فاصله، کوئری یا قطعه نگه دارید؛ در صورت امکان کلیدواژه را بگنجانید."),
        ];
    }
}

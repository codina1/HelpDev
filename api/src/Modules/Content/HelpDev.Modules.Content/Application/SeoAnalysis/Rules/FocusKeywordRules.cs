using HelpDev.Modules.Content.Application.SeoAnalysis.Markdown;

namespace HelpDev.Modules.Content.Application.SeoAnalysis.Rules;

public sealed class FocusKeywordPresenceRule : ISeoAnalysisRule
{
    public string RuleId => "seo.keyword.missing";

    public IReadOnlyList<SeoAnalysisFindingDto> Analyze(SeoAnalysisContext context)
    {
        var present = context.HasFocusKeyword;
        return
        [
            new SeoAnalysisFindingDto(
                RuleId,
                SeoFindingCategory.Keyword,
                present ? SeoFindingSeverity.Info : SeoFindingSeverity.Warning,
                present,
                "کلیدواژهٔ کانونی",
                present
                    ? $"کلیدواژهٔ کانونی تنظیم شده است: «{context.FocusKeyword}»."
                    : "کلیدواژهٔ کانونی تنظیم نشده است.",
                present ? context.FocusKeyword : null,
                present ? null : "برای راهنمایی تحریریه یک کلیدواژهٔ کانونی مشخص کنید."),
        ];
    }
}

public sealed class FocusKeywordCoverageRule : ISeoAnalysisRule
{
    public string RuleId => "seo.keyword.coverage";

    public IReadOnlyList<SeoAnalysisFindingDto> Analyze(SeoAnalysisContext context)
    {
        if (!context.HasFocusKeyword)
        {
            return
            [
                new SeoAnalysisFindingDto(
                    RuleId,
                    SeoFindingCategory.Keyword,
                    SeoFindingSeverity.Info,
                    Passed: true,
                    "پوشش کلیدواژه",
                    "بدون کلیدواژه، پوشش بررسی نمی‌شود.",
                    null,
                    null),
            ];
        }

        var kw = context.FocusKeyword!;
        var inTitle = MarkdownDocumentScanner.ContainsKeyword(context.EffectiveTitle, kw);
        var inDescription = MarkdownDocumentScanner.ContainsKeyword(context.EffectiveDescription, kw);
        var inSlug = MarkdownDocumentScanner.ContainsKeyword(context.Slug, kw);
        var inFirstParagraph = MarkdownDocumentScanner.ContainsKeyword(
            context.Facts.FirstParagraph?.Text,
            kw);
        var inHeading = context.Facts.Headings.Any(h =>
            MarkdownDocumentScanner.ContainsKeyword(h.Text, kw));
        var bodyCount = MarkdownDocumentScanner.CountKeywordOccurrences(context.Input.Body, kw);

        var hits = new List<string>();
        if (inTitle) hits.Add("title");
        if (inDescription) hits.Add("description");
        if (inSlug) hits.Add("slug");
        if (inFirstParagraph) hits.Add("first_paragraph");
        if (inHeading) hits.Add("heading");
        if (bodyCount > 0) hits.Add($"body×{bodyCount}");

        var coverageCount = (inTitle ? 1 : 0)
            + (inDescription ? 1 : 0)
            + (inSlug ? 1 : 0)
            + (inFirstParagraph ? 1 : 0)
            + (inHeading ? 1 : 0)
            + (bodyCount > 0 ? 1 : 0);

        var passed = coverageCount >= 3;
        return
        [
            new SeoAnalysisFindingDto(
                RuleId,
                SeoFindingCategory.Keyword,
                passed ? SeoFindingSeverity.Info : SeoFindingSeverity.Warning,
                passed,
                "پوشش کلیدواژه",
                passed
                    ? $"کلیدواژه در {coverageCount} بخش دیده می‌شود (شمارش واقعی، نه معیار رتبه)."
                    : $"کلیدواژه فقط در {coverageCount} بخش دیده می‌شود. توصیه: حداقل در چند بخش کلیدی ظاهر شود.",
                SeoAnalysisContext.CapEvidence(
                    hits.Count == 0 ? "no_occurrences" : string.Join(", ", hits)),
                passed
                    ? null
                    : "کلیدواژه را در عنوان، توضیحات، اسلاگ، پاراگراف اول یا یک سرفصل به‌صورت طبیعی استفاده کنید."),
        ];
    }
}

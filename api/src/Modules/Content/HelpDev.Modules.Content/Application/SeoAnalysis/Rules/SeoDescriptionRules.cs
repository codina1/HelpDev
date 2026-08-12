using HelpDev.Modules.Content.Application.SeoAnalysis.Markdown;

namespace HelpDev.Modules.Content.Application.SeoAnalysis.Rules;

public sealed class SeoDescriptionExistsRule : ISeoAnalysisRule
{
    public string RuleId => "seo.description.missing";

    public IReadOnlyList<SeoAnalysisFindingDto> Analyze(SeoAnalysisContext context)
    {
        var passed = context.EffectiveDescription.Length > 0;
        return
        [
            new SeoAnalysisFindingDto(
                RuleId,
                SeoFindingCategory.Description,
                passed ? SeoFindingSeverity.Info : SeoFindingSeverity.Warning,
                passed,
                "توضیحات مؤثر",
                passed
                    ? "توضیحات مؤثر (سئو یا خلاصه) موجود است."
                    : "توضیحات سئو و خلاصه هر دو خالی هستند.",
                SeoAnalysisContext.CapEvidence(context.EffectiveDescription),
                passed ? null : "توضیحات سئو یا خلاصهٔ محتوا را پر کنید."),
        ];
    }
}

public sealed class SeoDescriptionLengthRule : ISeoAnalysisRule
{
    public string RuleId => "seo.description.length";

    public IReadOnlyList<SeoAnalysisFindingDto> Analyze(SeoAnalysisContext context)
    {
        var description = context.EffectiveDescription;
        if (description.Length == 0)
        {
            return
            [
                new SeoAnalysisFindingDto(
                    RuleId,
                    SeoFindingCategory.Description,
                    SeoFindingSeverity.Info,
                    Passed: false,
                    "طول توضیحات",
                    "بدون توضیحات، بررسی طول اعمال نمی‌شود.",
                    null,
                    null),
            ];
        }

        var length = description.Length;
        if (length < SeoAnalysisOptions.RecommendedSeoDescriptionMin)
        {
            return
            [
                new SeoAnalysisFindingDto(
                    RuleId,
                    SeoFindingCategory.Description,
                    SeoFindingSeverity.Warning,
                    Passed: false,
                    "طول توضیحات",
                    $"توضیحات مؤثر کوتاه است ({length} نویسه). توصیه: حداقل {SeoAnalysisOptions.RecommendedSeoDescriptionMin}.",
                    SeoAnalysisContext.CapEvidence(description),
                    "توضیحات را غنی‌تر کنید. این توصیهٔ تحریریه است."),
            ];
        }

        if (length >= SeoAnalysisOptions.RecommendedSeoDescriptionNearMax)
        {
            return
            [
                new SeoAnalysisFindingDto(
                    RuleId,
                    SeoFindingCategory.Description,
                    SeoFindingSeverity.Warning,
                    Passed: false,
                    "طول توضیحات",
                    $"توضیحات نزدیک به سقف است ({length}/{SeoAnalysisOptions.MaxSeoDescriptionLength}).",
                    SeoAnalysisContext.CapEvidence(description),
                    "در صورت امکان کمی کوتاه‌تر کنید."),
            ];
        }

        return
        [
            new SeoAnalysisFindingDto(
                RuleId,
                SeoFindingCategory.Description,
                SeoFindingSeverity.Info,
                Passed: true,
                "طول توضیحات",
                $"طول توضیحات در بازهٔ توصیه‌شده است ({length} نویسه).",
                SeoAnalysisContext.CapEvidence(description),
                null),
        ];
    }
}

public sealed class SeoDescriptionKeywordRule : ISeoAnalysisRule
{
    public string RuleId => "seo.description.keyword_missing";

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
                    "کلیدواژه در توضیحات",
                    "کلیدواژهٔ کانونی تنظیم نشده؛ این بررسی رد شد.",
                    null,
                    null),
            ];
        }

        var present = MarkdownDocumentScanner.ContainsKeyword(
            context.EffectiveDescription,
            context.FocusKeyword);
        return
        [
            new SeoAnalysisFindingDto(
                RuleId,
                SeoFindingCategory.Keyword,
                present ? SeoFindingSeverity.Info : SeoFindingSeverity.Warning,
                present,
                "کلیدواژه در توضیحات",
                present
                    ? "کلیدواژهٔ کانونی در توضیحات مؤثر دیده می‌شود."
                    : "کلیدواژهٔ کانونی در توضیحات مؤثر دیده نمی‌شود.",
                SeoAnalysisContext.CapEvidence(
                    $"keyword={context.FocusKeyword}; description={context.EffectiveDescription}"),
                present ? null : "کلیدواژه را به‌صورت طبیعی در توضیحات سئو یا خلاصه بگنجانید."),
        ];
    }
}

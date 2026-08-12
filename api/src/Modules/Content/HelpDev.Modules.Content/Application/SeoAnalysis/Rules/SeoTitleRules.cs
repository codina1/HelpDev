using HelpDev.Modules.Content.Application.SeoAnalysis.Markdown;

namespace HelpDev.Modules.Content.Application.SeoAnalysis.Rules;

public sealed class SeoTitleExistsRule : ISeoAnalysisRule
{
    public string RuleId => "seo.title.missing";

    public IReadOnlyList<SeoAnalysisFindingDto> Analyze(SeoAnalysisContext context)
    {
        var passed = context.EffectiveTitle.Length > 0;
        return
        [
            new SeoAnalysisFindingDto(
                RuleId,
                SeoFindingCategory.Title,
                passed ? SeoFindingSeverity.Info : SeoFindingSeverity.Error,
                passed,
                "عنوان مؤثر",
                passed
                    ? "عنوان مؤثر برای نمایش در نتایج جستجو موجود است."
                    : "عنوان سئو و عنوان محتوا هر دو خالی هستند.",
                SeoAnalysisContext.CapEvidence(context.EffectiveTitle),
                passed
                    ? null
                    : "یک عنوان سئو یا عنوان محتوا تنظیم کنید."),
        ];
    }
}

public sealed class SeoTitleLengthRule : ISeoAnalysisRule
{
    public string RuleId => "seo.title.length";

    public IReadOnlyList<SeoAnalysisFindingDto> Analyze(SeoAnalysisContext context)
    {
        var title = context.EffectiveTitle;
        if (title.Length == 0)
        {
            return
            [
                new SeoAnalysisFindingDto(
                    RuleId,
                    SeoFindingCategory.Title,
                    SeoFindingSeverity.Info,
                    Passed: false,
                    "طول عنوان",
                    "بدون عنوان، بررسی طول اعمال نمی‌شود.",
                    null,
                    null),
            ];
        }

        var length = title.Length;
        if (length < SeoAnalysisOptions.RecommendedSeoTitleMin)
        {
            return
            [
                new SeoAnalysisFindingDto(
                    RuleId,
                    SeoFindingCategory.Title,
                    SeoFindingSeverity.Warning,
                    Passed: false,
                    "طول عنوان",
                    $"عنوان مؤثر کوتاه است ({length} نویسه). توصیهٔ تحریریه: حداقل {SeoAnalysisOptions.RecommendedSeoTitleMin} نویسه.",
                    SeoAnalysisContext.CapEvidence(title),
                    "عنوان را توصیفی‌تر کنید. این یک توصیهٔ تحریریه است، نه محدودیت دامنه."),
            ];
        }

        if (length >= SeoAnalysisOptions.RecommendedSeoTitleNearMax)
        {
            return
            [
                new SeoAnalysisFindingDto(
                    RuleId,
                    SeoFindingCategory.Title,
                    SeoFindingSeverity.Warning,
                    Passed: false,
                    "طول عنوان",
                    $"عنوان مؤثر نزدیک به سقف نمایش است ({length}/{SeoAnalysisOptions.MaxSeoTitleLength}).",
                    SeoAnalysisContext.CapEvidence(title),
                    "در صورت امکان عنوان را کمی کوتاه‌تر کنید تا در پیش‌نمایش بریده نشود."),
            ];
        }

        return
        [
            new SeoAnalysisFindingDto(
                RuleId,
                SeoFindingCategory.Title,
                SeoFindingSeverity.Info,
                Passed: true,
                "طول عنوان",
                $"طول عنوان مؤثر در بازهٔ توصیه‌شده است ({length} نویسه).",
                SeoAnalysisContext.CapEvidence(title),
                null),
        ];
    }
}

public sealed class SeoTitleKeywordRule : ISeoAnalysisRule
{
    public string RuleId => "seo.title.keyword_missing";

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
                    "کلیدواژه در عنوان",
                    "کلیدواژهٔ کانونی تنظیم نشده؛ این بررسی رد شد.",
                    null,
                    null),
            ];
        }

        var present = MarkdownDocumentScanner.ContainsKeyword(context.EffectiveTitle, context.FocusKeyword);
        return
        [
            new SeoAnalysisFindingDto(
                RuleId,
                SeoFindingCategory.Keyword,
                present ? SeoFindingSeverity.Info : SeoFindingSeverity.Warning,
                present,
                "کلیدواژه در عنوان",
                present
                    ? "کلیدواژهٔ کانونی در عنوان مؤثر دیده می‌شود."
                    : "کلیدواژهٔ کانونی در عنوان مؤثر دیده نمی‌شود.",
                SeoAnalysisContext.CapEvidence($"keyword={context.FocusKeyword}; title={context.EffectiveTitle}"),
                present ? null : "کلیدواژه را به‌صورت طبیعی در عنوان سئو یا عنوان محتوا بگنجانید."),
        ];
    }
}

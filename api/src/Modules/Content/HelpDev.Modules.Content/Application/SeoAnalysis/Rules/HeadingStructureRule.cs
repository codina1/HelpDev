using HelpDev.Modules.Content.Application.SeoAnalysis.Markdown;

namespace HelpDev.Modules.Content.Application.SeoAnalysis.Rules;

/// <summary>
/// Heading rules. Decision: the Content Title renders as page H1 outside the Markdown body
/// (see ContentDetailsCard / ContentPreviewPanel). Body H1 is discouraged; prefer H2+.
/// </summary>
public sealed class HeadingStructureRule : ISeoAnalysisRule
{
    public string RuleId => "seo.heading.structure";

    public IReadOnlyList<SeoAnalysisFindingDto> Analyze(SeoAnalysisContext context)
    {
        var headings = context.Facts.Headings;
        var wordCount = context.Facts.WordCount;
        var findings = new List<SeoAnalysisFindingDto>();

        if (headings.Count == 0)
        {
            var needsHeading = wordCount >= SeoAnalysisOptions.SufficientlyLongBodyWords;
            findings.Add(new SeoAnalysisFindingDto(
                "seo.heading.missing",
                SeoFindingCategory.Structure,
                needsHeading ? SeoFindingSeverity.Warning : SeoFindingSeverity.Info,
                Passed: !needsHeading,
                "سرفصل‌ها",
                needsHeading
                    ? "محتوای نسبتاً بلند بدون سرفصل است."
                    : "سرفصلی در بدنه نیست (برای متن کوتاه اختیاری است).",
                $"words={wordCount}",
                needsHeading ? "حداقل چند سرفصل Markdown (##) اضافه کنید." : null));
        }
        else
        {
            findings.Add(new SeoAnalysisFindingDto(
                "seo.heading.missing",
                SeoFindingCategory.Structure,
                SeoFindingSeverity.Info,
                Passed: true,
                "سرفصل‌ها",
                $"{headings.Count} سرفصل در بدنه یافت شد.",
                $"count={headings.Count}",
                null));
        }

        var h1Count = headings.Count(h => h.Level == 1);
        // Title is the page H1 outside the body — body H1 is discouraged.
        findings.Add(new SeoAnalysisFindingDto(
            "seo.heading.body_h1",
            SeoFindingCategory.Structure,
            h1Count == 0 ? SeoFindingSeverity.Info : SeoFindingSeverity.Warning,
            Passed: h1Count == 0,
            "H1 در بدنه",
            h1Count == 0
                ? "بدنه فاقد H1 است (عنوان صفحه به‌عنوان H1 جداگانه رندر می‌شود)."
                : $"بدنه شامل {h1Count} عنوان سطح ۱ است؛ با توجه به H1 بودن عنوان صفحه توصیه نمی‌شود.",
            $"body_h1_count={h1Count}",
            h1Count == 0 ? null : "سرفصل‌های بدنه را از ## (H2) شروع کنید."));

        var emptyHeadings = headings.Where(h => string.IsNullOrWhiteSpace(h.Text)).ToList();
        findings.Add(new SeoAnalysisFindingDto(
            "seo.heading.empty",
            SeoFindingCategory.Structure,
            emptyHeadings.Count == 0 ? SeoFindingSeverity.Info : SeoFindingSeverity.Warning,
            Passed: emptyHeadings.Count == 0,
            "سرفصل خالی",
            emptyHeadings.Count == 0
                ? "هیچ سرفصل خالی‌ای یافت نشد."
                : $"{emptyHeadings.Count} سرفصل خالی یافت شد.",
            emptyHeadings.Count == 0 ? null : $"lines={string.Join(',', emptyHeadings.Select(h => h.LineIndex))}",
            emptyHeadings.Count == 0 ? null : "متن سرفصل‌های خالی را تکمیل کنید."));

        var levelJump = false;
        for (var i = 1; i < headings.Count; i++)
        {
            if (headings[i].Level > headings[i - 1].Level + 1)
            {
                levelJump = true;
                break;
            }
        }

        findings.Add(new SeoAnalysisFindingDto(
            "seo.heading.level_jump",
            SeoFindingCategory.Structure,
            levelJump ? SeoFindingSeverity.Warning : SeoFindingSeverity.Info,
            Passed: !levelJump,
            "پرش سطح سرفصل",
            levelJump
                ? "پرش غیرمنتظره در سطح سرفصل‌ها (مثلاً H2 به H4) مشاهده شد."
                : "توالی سطح سرفصل‌ها منطقی است.",
            null,
            levelJump ? "سطوح سرفصل را به‌صورت پیوسته افزایش دهید." : null));

        if (context.HasFocusKeyword)
        {
            var inHeading = headings.Any(h =>
                MarkdownDocumentScanner.ContainsKeyword(h.Text, context.FocusKeyword));
            findings.Add(new SeoAnalysisFindingDto(
                "seo.heading.keyword_missing",
                SeoFindingCategory.Keyword,
                inHeading ? SeoFindingSeverity.Info : SeoFindingSeverity.Warning,
                inHeading,
                "کلیدواژه در سرفصل",
                inHeading
                    ? "کلیدواژهٔ کانونی در حداقل یک سرفصل دیده می‌شود."
                    : "کلیدواژهٔ کانونی در هیچ سرفصلی دیده نمی‌شود.",
                context.FocusKeyword,
                inHeading ? null : "کلیدواژه را در یکی از سرفصل‌ها به‌صورت طبیعی بگنجانید."));
        }

        return findings;
    }
}

namespace HelpDev.Modules.Content.Application.SeoAnalysis.Rules;

public sealed class ContentLengthRule : ISeoAnalysisRule
{
    public string RuleId => "seo.content.length";

    public IReadOnlyList<SeoAnalysisFindingDto> Analyze(SeoAnalysisContext context)
    {
        var words = context.Facts.WordCount;
        var chars = context.Facts.CharacterCount;
        var paragraphs = context.Facts.Paragraphs.Count;
        var headings = context.Facts.Headings.Count;

        SeoFindingSeverity severity;
        bool passed;
        string message;
        string? recommendation;

        if (words == 0)
        {
            severity = SeoFindingSeverity.Error;
            passed = false;
            message = "بدنه خالی است.";
            recommendation = "متن محتوا را اضافه کنید.";
        }
        else if (words < SeoAnalysisOptions.ShortBodyWords)
        {
            severity = SeoFindingSeverity.Warning;
            passed = false;
            message =
                $"متن کوتاه است ({words} واژه / {chars} نویسه / {paragraphs} پاراگراف / {headings} سرفصل).";
            recommendation =
                "برای مقالات آموزشی معمولاً متن کامل‌تری توصیه می‌شود. این توصیهٔ تحریریه است، نه رد اعتبار.";
        }
        else if (words < SeoAnalysisOptions.SufficientlyLongBodyWords)
        {
            severity = SeoFindingSeverity.Info;
            passed = true;
            message =
                $"طول محتوا متوسط است ({words} واژه / {chars} نویسه / {paragraphs} پاراگراف / {headings} سرفصل).";
            recommendation = null;
        }
        else
        {
            severity = SeoFindingSeverity.Info;
            passed = true;
            message =
                $"طول محتوا مناسب است ({words} واژه / {chars} نویسه / {paragraphs} پاراگراف / {headings} سرفصل).";
            recommendation = null;
        }

        return
        [
            new SeoAnalysisFindingDto(
                RuleId,
                SeoFindingCategory.Content,
                severity,
                passed,
                "طول محتوا",
                message,
                $"words={words}; chars={chars}; paragraphs={paragraphs}; headings={headings}",
                recommendation),
        ];
    }
}

public sealed class FirstParagraphRule : ISeoAnalysisRule
{
    public string RuleId => "seo.content.first_paragraph";

    public IReadOnlyList<SeoAnalysisFindingDto> Analyze(SeoAnalysisContext context)
    {
        var first = context.Facts.FirstParagraph;
        if (first is null)
        {
            return
            [
                new SeoAnalysisFindingDto(
                    RuleId,
                    SeoFindingCategory.Content,
                    SeoFindingSeverity.Warning,
                    Passed: false,
                    "پاراگراف اول",
                    "پاراگراف معناداری در ابتدای بدنه یافت نشد.",
                    null,
                    "پس از سرفصل‌ها، یک پاراگراف معرفی بنویسید."),
            ];
        }

        var findings = new List<SeoAnalysisFindingDto>
        {
            new(
                "seo.content.first_paragraph.exists",
                SeoFindingCategory.Content,
                SeoFindingSeverity.Info,
                Passed: true,
                "پاراگراف اول",
                "پاراگراف اول معنادار استخراج شد.",
                SeoAnalysisContext.CapEvidence(first.Text),
                null),
        };

        var tooLong = first.Text.Length > SeoAnalysisOptions.RecommendedFirstParagraphMaxChars;
        findings.Add(new SeoAnalysisFindingDto(
            "seo.content.first_paragraph.length",
            SeoFindingCategory.Content,
            tooLong ? SeoFindingSeverity.Warning : SeoFindingSeverity.Info,
            Passed: !tooLong,
            "طول پاراگراف اول",
            tooLong
                ? $"پاراگراف اول طولانی است ({first.Text.Length} نویسه)."
                : $"طول پاراگراف اول مناسب است ({first.Text.Length} نویسه).",
            $"chars={first.Text.Length}",
            tooLong
                ? $"توصیه: زیر {SeoAnalysisOptions.RecommendedFirstParagraphMaxChars} نویسه نگه دارید."
                : null));

        if (context.HasFocusKeyword)
        {
            var present = Markdown.MarkdownDocumentScanner.ContainsKeyword(
                first.Text,
                context.FocusKeyword);
            findings.Add(new SeoAnalysisFindingDto(
                "seo.content.first_paragraph.keyword_missing",
                SeoFindingCategory.Keyword,
                present ? SeoFindingSeverity.Info : SeoFindingSeverity.Warning,
                present,
                "کلیدواژه در پاراگراف اول",
                present
                    ? "کلیدواژهٔ کانونی در پاراگراف اول دیده می‌شود."
                    : "کلیدواژهٔ کانونی در پاراگراف اول دیده نمی‌شود.",
                context.FocusKeyword,
                present ? null : "کلیدواژه را در جملهٔ آغازین به‌صورت طبیعی بگنجانید."));
        }

        return findings;
    }
}

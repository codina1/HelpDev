using HelpDev.Modules.Content.Application.SeoAnalysis;
using HelpDev.Modules.Content.Application.SeoAnalysis.Markdown;

namespace HelpDev.Content.Tests.SeoAnalysis;

public sealed class ContentSeoAnalyzerTests
{
    private readonly ContentSeoAnalyzer _analyzer = new();
    private static readonly DateTime AnalyzedAt = new(2026, 7, 22, 12, 0, 0, DateTimeKind.Utc);

    private static SeoAnalysisInput Input(
        string title = "Learning ASP.NET Core Basics",
        string slug = "learning-aspnet-core-basics",
        string body = """
            ASP.NET Core Basics opens with a clear intro paragraph about ASP.NET Core Basics.

            ## ASP.NET Core Basics Overview

            More body text about ASP.NET Core Basics for developers.

            See [docs](/docs/aspnet) and [external](https://example.com).

            ```csharp
            Console.WriteLine("hi");
            ```
            """,
        string excerpt = "ASP.NET Core Basics excerpt covering the fundamentals briefly enough for SEO.",
        string? coverImage = "https://cdn.example.com/cover.png",
        string contentType = "Article",
        string? seoTitle = "ASP.NET Core Basics Guide for Developers",
        string? seoDescription = "ASP.NET Core Basics explained for developers with practical examples and guidance.",
        string? canonicalUrl = "https://example.com/learning-aspnet-core-basics",
        string? ogImage = "https://cdn.example.com/og.png",
        string? focusKeyword = "ASP.NET Core Basics") =>
        new(title, slug, body, excerpt, coverImage, contentType, seoTitle, seoDescription, canonicalUrl, ogImage, focusKeyword);

    [Fact]
    public void Analyze_is_deterministic_except_timestamp()
    {
        var input = Input();
        var a = _analyzer.Analyze(input, AnalyzedAt);
        var b = _analyzer.Analyze(input, AnalyzedAt.AddMinutes(5));

        Assert.Equal(a.Findings.Select(f => f.RuleId), b.Findings.Select(f => f.RuleId));
        Assert.Equal(a.Findings.Select(f => f.Passed), b.Findings.Select(f => f.Passed));
        Assert.Equal(a.Summary, b.Summary);
        Assert.Equal(a.Statistics, b.Statistics);
        Assert.NotEqual(a.AnalyzedAtUtc, b.AnalyzedAtUtc);
    }

    [Fact]
    public void Summary_counts_match_findings()
    {
        var report = _analyzer.Analyze(Input(), AnalyzedAt);
        var findings = report.Findings;

        Assert.Equal(findings.Count(f => f.Passed), report.Summary.PassedCount);
        Assert.Equal(findings.Count(f => !f.Passed && f.Severity == SeoFindingSeverity.Warning), report.Summary.WarningCount);
        Assert.Equal(findings.Count(f => !f.Passed && f.Severity == SeoFindingSeverity.Error), report.Summary.ErrorCount);
        Assert.Equal(findings.Count(f => f.Severity == SeoFindingSeverity.Info), report.Summary.InformationalCount);
    }

    [Fact]
    public void Findings_are_sorted_deterministically()
    {
        var report = _analyzer.Analyze(Input(), AnalyzedAt);
        var ordered = report.Findings
            .OrderBy(f => f.Category)
            .ThenBy(f => f.RuleId, StringComparer.Ordinal)
            .ToList();

        Assert.Equal(ordered.Select(f => (f.Category, f.RuleId)), report.Findings.Select(f => (f.Category, f.RuleId)));
    }

    [Fact]
    public void RuleIds_are_stable_and_language_neutral()
    {
        var report = _analyzer.Analyze(Input(), AnalyzedAt);
        Assert.All(report.Findings, f =>
        {
            Assert.StartsWith("seo.", f.RuleId);
            Assert.DoesNotContain(' ', f.RuleId);
            Assert.NotEqual(f.RuleId, f.Title);
        });
    }

    [Fact]
    public void Report_does_not_include_score_or_percentage_fields()
    {
        var props = typeof(SeoAnalysisReportDto).GetProperties().Select(p => p.Name).ToArray();
        Assert.DoesNotContain(props, p => p.Contains("Score", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(props, p => p.Contains("Percent", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(props, p => p.Contains("Rank", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Seo_title_falls_back_to_content_title()
    {
        var report = _analyzer.Analyze(Input(seoTitle: null, title: "Content Title Fallback"), AnalyzedAt);
        var length = Assert.Single(report.Findings, f => f.RuleId == "seo.title.length");
        Assert.Contains("Content Title Fallback", length.Evidence);
    }

    [Fact]
    public void Seo_title_missing_when_both_blank()
    {
        var report = _analyzer.Analyze(Input(title: "   ", seoTitle: null), AnalyzedAt);
        var finding = Assert.Single(report.Findings, f => f.RuleId == "seo.title.missing");
        Assert.False(finding.Passed);
        Assert.Equal(SeoFindingSeverity.Error, finding.Severity);
    }

    [Fact]
    public void Seo_title_short_and_near_max_recommendations()
    {
        var shortReport = _analyzer.Analyze(Input(seoTitle: "Short title here"), AnalyzedAt);
        var shortFinding = Assert.Single(shortReport.Findings, f => f.RuleId == "seo.title.length");
        Assert.False(shortFinding.Passed);
        Assert.Equal(SeoFindingSeverity.Warning, shortFinding.Severity);

        var nearMax = new string('a', 62);
        var nearReport = _analyzer.Analyze(Input(seoTitle: nearMax, focusKeyword: null), AnalyzedAt);
        var nearFinding = Assert.Single(nearReport.Findings, f => f.RuleId == "seo.title.length");
        Assert.False(nearFinding.Passed);
        Assert.Equal(SeoFindingSeverity.Warning, nearFinding.Severity);
    }

    [Fact]
    public void Seo_title_keyword_present_and_missing()
    {
        var present = _analyzer.Analyze(Input(seoTitle: "ASP.NET Core Basics Guide", focusKeyword: "ASP.NET Core Basics"), AnalyzedAt);
        Assert.True(Assert.Single(present.Findings, f => f.RuleId == "seo.title.keyword_missing").Passed);

        var missing = _analyzer.Analyze(Input(seoTitle: "Completely different title text here now", focusKeyword: "ASP.NET Core Basics"), AnalyzedAt);
        Assert.False(Assert.Single(missing.Findings, f => f.RuleId == "seo.title.keyword_missing").Passed);
    }

    [Fact]
    public void Description_uses_seo_then_excerpt_fallback()
    {
        var withSeo = _analyzer.Analyze(Input(seoDescription: "SEO description with enough characters for recommendation band."), AnalyzedAt);
        Assert.Contains(
            "SEO description",
            Assert.Single(withSeo.Findings, f => f.RuleId == "seo.description.length").Evidence!);

        var fallback = _analyzer.Analyze(
            Input(seoDescription: null, excerpt: "Excerpt fallback text that is long enough for SEO description recommendation."),
            AnalyzedAt);
        Assert.Contains(
            "Excerpt fallback",
            Assert.Single(fallback.Findings, f => f.RuleId == "seo.description.length").Evidence!);
    }

    [Fact]
    public void Description_missing_and_keyword_checks()
    {
        var missing = _analyzer.Analyze(Input(seoDescription: null, excerpt: "  "), AnalyzedAt);
        Assert.False(Assert.Single(missing.Findings, f => f.RuleId == "seo.description.missing").Passed);

        var withKw = _analyzer.Analyze(
            Input(seoDescription: "ASP.NET Core Basics explained for developers with practical examples and guidance.", focusKeyword: "ASP.NET Core Basics"),
            AnalyzedAt);
        Assert.True(Assert.Single(withKw.Findings, f => f.RuleId == "seo.description.keyword_missing").Passed);

        var withoutKw = _analyzer.Analyze(
            Input(seoDescription: "Something unrelated that is long enough for the recommended description band here.", focusKeyword: "ASP.NET Core Basics"),
            AnalyzedAt);
        Assert.False(Assert.Single(withoutKw.Findings, f => f.RuleId == "seo.description.keyword_missing").Passed);
    }

    [Fact]
    public void Focus_keyword_absent_does_not_block_analysis()
    {
        var report = _analyzer.Analyze(Input(focusKeyword: null), AnalyzedAt);
        var missing = Assert.Single(report.Findings, f => f.RuleId == "seo.keyword.missing");
        Assert.True(missing.Severity is SeoFindingSeverity.Info or SeoFindingSeverity.Warning);
        Assert.NotEmpty(report.Findings);
    }

    [Fact]
    public void Focus_keyword_coverage_across_fields_including_persian()
    {
        var body = """
            کلیدواژه اصلی در ابتدای پاراگراف معنی‌دار قرار دارد و ادامه دارد.

            ## کلیدواژه اصلی چیست

            متن بیشتر درباره کلیدواژه اصلی.
            """;

        var report = _analyzer.Analyze(
            Input(
                title: "کلیدواژه اصلی در عنوان",
                slug: "کلیدواژه-اصلی",
                body: body,
                excerpt: "کلیدواژه اصلی در توضیح",
                seoTitle: null,
                seoDescription: null,
                focusKeyword: "کلیدواژه اصلی"),
            AnalyzedAt);

        var coverage = Assert.Single(report.Findings, f => f.RuleId == "seo.keyword.coverage");
        Assert.True(coverage.Passed);
        Assert.Contains("title", coverage.Evidence!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Slug_rules_cover_whitespace_query_fragment_and_persian()
    {
        var persian = _analyzer.Analyze(Input(slug: "راهنمای-aspnet", focusKeyword: null), AnalyzedAt);
        Assert.True(Assert.Single(persian.Findings, f => f.RuleId == "seo.slug.quality").Passed
                    || persian.Findings.Any(f => f.RuleId == "seo.slug.quality"));

        var whitespace = _analyzer.Analyze(Input(slug: "bad slug", focusKeyword: null), AnalyzedAt);
        Assert.False(Assert.Single(whitespace.Findings, f => f.RuleId == "seo.slug.quality").Passed);

        var fragment = _analyzer.Analyze(Input(slug: "ok-slug#frag", focusKeyword: null), AnalyzedAt);
        Assert.False(Assert.Single(fragment.Findings, f => f.RuleId == "seo.slug.quality").Passed);

        var query = _analyzer.Analyze(Input(slug: "ok-slug?x=1", focusKeyword: null), AnalyzedAt);
        Assert.False(Assert.Single(query.Findings, f => f.RuleId == "seo.slug.quality").Passed);
    }

    [Fact]
    public void Heading_structure_detects_level_jump_empty_and_body_h1()
    {
        var jump = _analyzer.Analyze(
            Input(body: "## Section\n\nText\n\n#### Too deep\n\nMore text about topics."),
            AnalyzedAt);
        Assert.Contains(jump.Findings, f => f.RuleId == "seo.heading.level_jump" && !f.Passed);

        var empty = _analyzer.Analyze(Input(body: "##   \n\nParagraph text here."), AnalyzedAt);
        Assert.Contains(empty.Findings, f => f.RuleId == "seo.heading.empty" && !f.Passed);

        var bodyH1 = _analyzer.Analyze(Input(body: "# Body H1\n\nParagraph under h1."), AnalyzedAt);
        Assert.Contains(bodyH1.Findings, f => f.RuleId == "seo.heading.body_h1");
    }

    [Fact]
    public void Content_length_reports_factual_counts_for_empty_short_and_long()
    {
        var empty = _analyzer.Analyze(Input(body: ""), AnalyzedAt);
        Assert.Equal(0, empty.Statistics.WordCount);
        Assert.Equal(0, empty.Statistics.ParagraphCount);

        var shortBody = _analyzer.Analyze(Input(body: "Short body text."), AnalyzedAt);
        Assert.True(shortBody.Statistics.WordCount < SeoAnalysisOptions.ShortBodyWords);
        var lengthFinding = Assert.Single(shortBody.Findings, f => f.RuleId == "seo.content.length");
        Assert.False(lengthFinding.Passed);
        Assert.Equal(SeoFindingSeverity.Warning, lengthFinding.Severity);

        var words = string.Join(' ', Enumerable.Repeat("word", 350));
        var longBody = _analyzer.Analyze(Input(body: words), AnalyzedAt);
        Assert.True(longBody.Statistics.WordCount >= SeoAnalysisOptions.SufficientlyLongBodyWords);
    }

    [Fact]
    public void Links_classify_relative_external_empty_and_unsafe()
    {
        var report = _analyzer.Analyze(
            Input(body: """
                See [internal](/path), [external](https://example.com), [](https://example.com/empty),
                and [bad](javascript:alert(1)).
                """),
            AnalyzedAt);

        Assert.True(report.Statistics.InternalLinkCount >= 1);
        Assert.True(report.Statistics.ExternalLinkCount >= 1);
        Assert.Contains(report.Findings, f => f.RuleId == "seo.link.empty_label" && !f.Passed);
        Assert.Contains(report.Findings, f => f.RuleId == "seo.link.unsafe_scheme" && f.Severity == SeoFindingSeverity.Error);
    }

    [Fact]
    public void Code_blocks_recommend_language_labels_without_penalizing_code_heavy_content()
    {
        var report = _analyzer.Analyze(
            Input(body: """
                Intro.

                ```csharp
                var x = 1;
                ```

                ```
                raw
                ```

                More text.
                """),
            AnalyzedAt);

        Assert.Equal(2, report.Statistics.CodeBlockCount);
        Assert.Equal(1, report.Statistics.LanguageLabelledCodeBlockCount);
        Assert.Equal(1, report.Statistics.UnlabelledCodeBlockCount);
        Assert.Contains(report.Findings, f => f.RuleId == "seo.code.language_missing");
        Assert.DoesNotContain(report.Findings, f =>
            f.Message.Contains("poor SEO", StringComparison.OrdinalIgnoreCase)
            || f.Message.Contains("سئوی ضعیف", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Canonical_and_media_rules_without_network()
    {
        var missing = _analyzer.Analyze(Input(canonicalUrl: null, coverImage: null, ogImage: null), AnalyzedAt);
        Assert.Contains(missing.Findings, f => f.RuleId == "seo.canonical");
        Assert.Contains(missing.Findings, f => f.RuleId == "seo.media.cover");
        Assert.Contains(missing.Findings, f => f.RuleId == "seo.media.og_image");

        var withQuery = _analyzer.Analyze(Input(canonicalUrl: "https://example.com/a?q=1"), AnalyzedAt);
        Assert.Contains(withQuery.Findings, f => f.RuleId == "seo.canonical.query");

        var withFragment = _analyzer.Analyze(Input(canonicalUrl: "https://example.com/a#x"), AnalyzedAt);
        Assert.Contains(withFragment.Findings, f => f.RuleId == "seo.canonical.fragment");
    }

    [Fact]
    public void Large_article_analysis_completes_with_bounded_behavior()
    {
        var paragraph = string.Join(' ', Enumerable.Repeat("ASP.NET Core Basics word", 40));
        var body = string.Join("\n\n", Enumerable.Range(0, 120).Select(i =>
            i % 8 == 0
                ? $"## ASP.NET Core Basics Section {i}\n\n{paragraph} [link](/p/{i})"
                : paragraph));

        var report = _analyzer.Analyze(
            Input(body: body, focusKeyword: "ASP.NET Core Basics"),
            AnalyzedAt);

        Assert.True(report.Statistics.WordCount > 1000);
        Assert.NotEmpty(report.Findings);
        Assert.Equal(
            report.Findings.Count(f => f.Passed),
            report.Summary.PassedCount);
    }
}

using HelpDev.Modules.Content.Application.SeoAnalysis;
using HelpDev.Modules.Content.Application.SeoAnalysis.Rules;

namespace HelpDev.Content.Tests.SeoAnalysis;

public sealed class SeoPlatformRuleTests
{
    [Fact]
    public void InternalLinksPresenceRule_warns_when_body_has_no_internal_links()
    {
        var rule = new InternalLinksPresenceRule();
        var context = SeoTestContextFactory.Create(body: "متن بدون پیوند.");

        var findings = rule.Analyze(context);

        Assert.Contains(findings, f => f.RuleId == "seo.link.no_internal" && !f.Passed);
    }

    [Fact]
    public void CanonicalMissingRule_flags_missing_canonical()
    {
        var rule = new CanonicalMissingRule();
        var context = SeoTestContextFactory.Create(canonicalUrl: null);

        var findings = rule.Analyze(context);

        Assert.Single(findings);
        Assert.Equal("seo.canonical.missing", findings[0].RuleId);
        Assert.False(findings[0].Passed);
    }

    [Fact]
    public void SeoMetadataValidityRule_flags_overlong_title()
    {
        var rule = new SeoMetadataValidityRule();
        var longTitle = new string('a', HelpDev.Modules.Content.Domain.ValueObjects.SeoMetadata.MaxSeoTitleLength + 1);
        var context = SeoTestContextFactory.Create(seoTitle: longTitle);

        var findings = rule.Analyze(context);

        Assert.Contains(findings, f => f.RuleId == "seo.metadata.seoTitle_length" && !f.Passed);
    }

    [Fact]
    public void ImageAltInBodyRule_warns_when_markdown_images_lack_alt()
    {
        var rule = new ImageAltInBodyRule();
        var context = SeoTestContextFactory.Create(body: "![](/img.png)");

        var findings = rule.Analyze(context);

        Assert.Contains(findings, f => f.RuleId == "seo.media.image_alt" && !f.Passed);
    }

    [Fact]
    public void SeoAuditMapper_maps_to_platform_categories_without_score()
    {
        var engine = new SeoAnalysisReportDto(
            DateTime.UtcNow,
            new SeoAnalysisSummaryDto(1, 1, 0, 2),
            [
                new SeoAnalysisFindingDto(
                    "seo.title.missing",
                    SeoFindingCategory.Title,
                    SeoFindingSeverity.Warning,
                    Passed: false,
                    "عنوان",
                    "missing",
                    null,
                    "add title"),
            ],
            new SeoAnalysisStatisticsDto(1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 1));

        var audit = SeoAuditMapper.ToDto(Guid.NewGuid(), engine);

        Assert.Equal(SeoPlatformCategory.Metadata, audit.Findings[0].Category);
        Assert.Null(typeof(SeoAuditReportDto).GetProperty("Score"));
        Assert.Null(typeof(SeoAuditReportDto).GetProperty("Statistics"));
    }
}

internal static class SeoTestContextFactory
{
    public static SeoAnalysisContext Create(
        string? body = "Hello",
        string? canonicalUrl = null,
        string? seoTitle = null)
    {
        var input = new SeoAnalysisInput(
            "Title",
            "slug",
            body ?? string.Empty,
            string.Empty,
            null,
            "Article",
            seoTitle,
            null,
            canonicalUrl,
            null,
            null);

        var facts = HelpDev.Modules.Content.Application.SeoAnalysis.Markdown.MarkdownDocumentScanner.Scan(body);
        return new SeoAnalysisContext(input, facts);
    }
}

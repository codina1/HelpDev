using HelpDev.Modules.Content.Domain.ValueObjects;

namespace HelpDev.Modules.Content.Application.SeoAnalysis.Rules;

/// <summary>
/// Flags SEO field values that would fail domain validation if saved as-is
/// (length or canonical shape). Does not call <see cref="SeoMetadata.Create"/>.
/// </summary>
public sealed class SeoMetadataValidityRule : ISeoAnalysisRule
{
    public string RuleId => "seo.metadata.invalid";

    public IReadOnlyList<SeoAnalysisFindingDto> Analyze(SeoAnalysisContext context)
    {
        var findings = new List<SeoAnalysisFindingDto>();

        CheckLength(findings, context.Input.SeoTitle, SeoMetadata.MaxSeoTitleLength, "seoTitle", "عنوان سئو");
        CheckLength(findings, context.Input.SeoDescription, SeoMetadata.MaxSeoDescriptionLength, "seoDescription", "توضیحات سئو");
        CheckLength(findings, context.Input.OgImage, SeoMetadata.MaxOgImageLength, "ogImage", "تصویر OG");
        CheckLength(findings, context.Input.FocusKeyword, SeoMetadata.MaxFocusKeywordLength, "focusKeyword", "کلمه کلیدی");

        var canonical = context.Input.CanonicalUrl;
        if (!string.IsNullOrWhiteSpace(canonical))
        {
            var trimmed = canonical.Trim();
            if (trimmed.Length > SeoMetadata.MaxCanonicalUrlLength)
            {
                findings.Add(InvalidFinding(
                    "seo.metadata.canonical_length",
                    "نشانی کانونیکال از حد مجاز طولانی‌تر است.",
                    "canonicalUrl",
                    "نشانی را کوتاه‌تر کنید."));
            }
            else if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri)
                     || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                findings.Add(InvalidFinding(
                    "seo.metadata.canonical_shape",
                    "نشانی کانونیکال ذخیره‌شده شکل URL مطلق http(s) ندارد.",
                    "canonicalUrl",
                    "یک URL مطلق معتبر وارد کنید یا فیلد را خالی بگذارید."));
            }
        }

        if (findings.Count == 0)
        {
            findings.Add(new SeoAnalysisFindingDto(
                RuleId,
                SeoFindingCategory.Metadata,
                SeoFindingSeverity.Info,
                Passed: true,
                "اعتبار متادیتای سئو",
                "فیلدهای سئو از نظر طول و شکل کانونیکال با قوانین ذخیره‌سازی سازگارند.",
                null,
                null));
        }

        return findings;
    }

    private static void CheckLength(
        List<SeoAnalysisFindingDto> findings,
        string? value,
        int maxLength,
        string fieldKey,
        string fieldLabel)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        var trimmed = value.Trim();
        if (trimmed.Length <= maxLength)
        {
            return;
        }

        findings.Add(InvalidFinding(
            $"seo.metadata.{fieldKey}_length",
            $"{fieldLabel} از حد مجاز ({maxLength} نویسه) طولانی‌تر است.",
            fieldKey,
            "مقدار را کوتاه کنید تا ذخیره‌سازی ممکن شود."));
    }

    private static SeoAnalysisFindingDto InvalidFinding(
        string ruleId,
        string message,
        string fieldKey,
        string suggestion) =>
        new(
            ruleId,
            SeoFindingCategory.Metadata,
            SeoFindingSeverity.Error,
            Passed: false,
            "متادیتای سئو",
            message,
            fieldKey,
            suggestion);
}

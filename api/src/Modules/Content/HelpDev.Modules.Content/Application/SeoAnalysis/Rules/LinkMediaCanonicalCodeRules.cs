namespace HelpDev.Modules.Content.Application.SeoAnalysis.Rules;

public sealed class LinkSafetyRule : ISeoAnalysisRule
{
    public string RuleId => "seo.link.summary";

    public IReadOnlyList<SeoAnalysisFindingDto> Analyze(SeoAnalysisContext context)
    {
        var links = context.Facts.Links;
        var internalCount = 0;
        var externalCount = 0;
        var emptyLabel = 0;
        var unsafeScheme = 0;
        var malformed = 0;

        foreach (var link in links)
        {
            if (string.IsNullOrWhiteSpace(link.Label))
            {
                emptyLabel++;
            }

            var href = link.Href.Trim();
            if (href.Length == 0)
            {
                malformed++;
                continue;
            }

            if (href.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase)
                || href.StartsWith("data:", StringComparison.OrdinalIgnoreCase)
                || href.StartsWith("vbscript:", StringComparison.OrdinalIgnoreCase))
            {
                unsafeScheme++;
                continue;
            }

            if (href.StartsWith('#') || href.StartsWith('/') || href.StartsWith("./") || href.StartsWith("../"))
            {
                internalCount++;
                continue;
            }

            if (Uri.TryCreate(href, UriKind.Absolute, out var absolute))
            {
                if (absolute.Scheme is "http" or "https")
                {
                    externalCount++;
                }
                else
                {
                    unsafeScheme++;
                }
            }
            else if (href.Contains(' ', StringComparison.Ordinal))
            {
                malformed++;
            }
            else
            {
                // Relative-looking without leading slash — treat as internal-ish path.
                internalCount++;
            }
        }

        var findings = new List<SeoAnalysisFindingDto>
        {
            new(
                RuleId,
                SeoFindingCategory.Links,
                SeoFindingSeverity.Info,
                Passed: true,
                "خلاصهٔ پیوندها",
                $"پیوندها: کل={links.Count}، نسبی/داخلی={internalCount}، مطلق/خارجی={externalCount}.",
                $"total={links.Count}; internal={internalCount}; external={externalCount}",
                null),
        };

        findings.Add(new SeoAnalysisFindingDto(
            "seo.link.empty_label",
            SeoFindingCategory.Links,
            emptyLabel == 0 ? SeoFindingSeverity.Info : SeoFindingSeverity.Warning,
            Passed: emptyLabel == 0,
            "برچسب خالی پیوند",
            emptyLabel == 0
                ? "همهٔ پیوندها برچسب دارند."
                : $"{emptyLabel} پیوند با برچسب خالی یافت شد.",
            emptyLabel == 0 ? null : $"empty_label_count={emptyLabel}",
            emptyLabel == 0 ? null : "برای هر پیوند یک متن توصیفی بنویسید."));

        findings.Add(new SeoAnalysisFindingDto(
            "seo.link.unsafe_scheme",
            SeoFindingCategory.Links,
            unsafeScheme == 0 ? SeoFindingSeverity.Info : SeoFindingSeverity.Error,
            Passed: unsafeScheme == 0,
            "طرح ناامن پیوند",
            unsafeScheme == 0
                ? "طرح ناامن (javascript/data/…) یافت نشد."
                : $"{unsafeScheme} پیوند با طرح ناامن یافت شد.",
            unsafeScheme == 0 ? null : $"unsafe_count={unsafeScheme}",
            unsafeScheme == 0 ? null : "پیوندهای javascript:/data: را حذف یا اصلاح کنید."));

        findings.Add(new SeoAnalysisFindingDto(
            "seo.link.malformed",
            SeoFindingCategory.Links,
            malformed == 0 ? SeoFindingSeverity.Info : SeoFindingSeverity.Warning,
            Passed: malformed == 0,
            "پیوند ناقص",
            malformed == 0
                ? "پیوند ناقص واضحی یافت نشد."
                : $"{malformed} پیوند ناقص یا خالی یافت شد.",
            malformed == 0 ? null : $"malformed_count={malformed}",
            malformed == 0 ? null : "آدرس پیوندها را بررسی کنید."));

        return findings;
    }
}

public sealed class MediaPresenceRule : ISeoAnalysisRule
{
    public string RuleId => "seo.media.cover";

    public IReadOnlyList<SeoAnalysisFindingDto> Analyze(SeoAnalysisContext context)
    {
        var findings = new List<SeoAnalysisFindingDto>();

        findings.Add(BuildImageFinding(
            "seo.media.cover",
            "تصویر کاور",
            context.CoverImage,
            "تصویر کاور تنظیم نشده است.",
            "یک نشانی http(s) معتبر برای تصویر کاور تنظیم کنید."));

        findings.Add(BuildImageFinding(
            "seo.media.og_image",
            "تصویر Open Graph",
            context.OgImage,
            "تصویر OG تنظیم نشده است (پیش‌نمایش فرانت ممکن است به کاور برگردد، اما متادیتای ذخیره‌شده خالی است).",
            "برای اشتراک‌گذاری اجتماعی یک تصویر OG تنظیم کنید."));

        return findings;
    }

    private static SeoAnalysisFindingDto BuildImageFinding(
        string ruleId,
        string title,
        string? url,
        string missingMessage,
        string recommendation)
    {
        if (url is null)
        {
            return new SeoAnalysisFindingDto(
                ruleId,
                SeoFindingCategory.Media,
                SeoFindingSeverity.Warning,
                Passed: false,
                title,
                missingMessage,
                null,
                recommendation);
        }

        var valid = IsAcceptedImageUrl(url);
        return new SeoAnalysisFindingDto(
            ruleId,
            SeoFindingCategory.Media,
            valid ? SeoFindingSeverity.Info : SeoFindingSeverity.Warning,
            valid,
            title,
            valid ? $"{title} تنظیم شده است." : $"{title} نشانی معتبری ندارد.",
            SeoAnalysisContext.CapEvidence(url),
            valid ? null : "از نشانی مطلق http(s) استفاده کنید. تصویر دانلود نمی‌شود.");
    }

    private static bool IsAcceptedImageUrl(string url)
    {
        if (url.StartsWith("/", StringComparison.Ordinal) || url.StartsWith("./", StringComparison.Ordinal))
        {
            return true;
        }

        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}

public sealed class CanonicalUrlRule : ISeoAnalysisRule
{
    public string RuleId => "seo.canonical";

    public IReadOnlyList<SeoAnalysisFindingDto> Analyze(SeoAnalysisContext context)
    {
        if (context.CanonicalUrl is null)
        {
            return
            [
                new SeoAnalysisFindingDto(
                    RuleId,
                    SeoFindingCategory.Url,
                    SeoFindingSeverity.Info,
                    Passed: true,
                    "نشانی کانونیکال",
                    "نشانی کانونیکال اختیاری است و تنظیم نشده.",
                    null,
                    "در صورت نیاز یک URL مطلق http(s) تنظیم کنید."),
            ];
        }

        var url = context.CanonicalUrl;
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return
            [
                new SeoAnalysisFindingDto(
                    RuleId,
                    SeoFindingCategory.Url,
                    SeoFindingSeverity.Error,
                    Passed: false,
                    "نشانی کانونیکال",
                    "نشانی کانونیکال باید یک URL مطلق http(s) باشد.",
                    SeoAnalysisContext.CapEvidence(url),
                    "یک URL مطلق معتبر وارد کنید."),
            ];
        }

        var findings = new List<SeoAnalysisFindingDto>
        {
            new(
                "seo.canonical.valid",
                SeoFindingCategory.Url,
                SeoFindingSeverity.Info,
                Passed: true,
                "نشانی کانونیکال",
                "نشانی کانونیکال مطلق و معتبر است.",
                SeoAnalysisContext.CapEvidence(url),
                null),
        };

        if (!string.IsNullOrEmpty(uri.Fragment))
        {
            findings.Add(new SeoAnalysisFindingDto(
                "seo.canonical.fragment",
                SeoFindingCategory.Url,
                SeoFindingSeverity.Warning,
                Passed: false,
                "قطعه در کانونیکال",
                "نشانی کانونیکال شامل fragment است.",
                uri.Fragment,
                "fragment (#…) را از کانونیکال حذف کنید."));
        }

        if (!string.IsNullOrEmpty(uri.Query))
        {
            findings.Add(new SeoAnalysisFindingDto(
                "seo.canonical.query",
                SeoFindingCategory.Url,
                SeoFindingSeverity.Warning,
                Passed: false,
                "کوئری در کانونیکال",
                "نشانی کانونیکال شامل query string است.",
                uri.Query,
                "در صورت امکان پارامترهای کوئری را از کانونیکال حذف کنید."));
        }

        return findings;
    }
}

public sealed class CodeBlockRule : ISeoAnalysisRule
{
    public string RuleId => "seo.code.summary";

    public IReadOnlyList<SeoAnalysisFindingDto> Analyze(SeoAnalysisContext context)
    {
        var total = context.Facts.CodeBlocks.Count;
        var labelled = context.Facts.LanguageLabelledCodeBlockCount;
        var unlabelled = context.Facts.UnlabelledCodeBlockCount;

        return
        [
            new SeoAnalysisFindingDto(
                RuleId,
                SeoFindingCategory.Content,
                SeoFindingSeverity.Info,
                Passed: true,
                "بلوک‌های کد",
                $"بلوک کد فنس‌شده: {total} (برچسب‌دار: {labelled}، بدون زبان: {unlabelled}). محتوای کد‌محور برای HelpDev منفی تلقی نمی‌شود.",
                $"total={total}; labelled={labelled}; unlabelled={unlabelled}",
                null),
            new SeoAnalysisFindingDto(
                "seo.code.language_missing",
                SeoFindingCategory.Content,
                unlabelled == 0 ? SeoFindingSeverity.Info : SeoFindingSeverity.Warning,
                Passed: unlabelled == 0,
                "برچسب زبان کد",
                unlabelled == 0
                    ? "همهٔ بلوک‌های فنس‌شده برچسب زبان دارند (یا بلوکی نیست)."
                    : $"{unlabelled} بلوک فنس‌شده بدون برچسب زبان است.",
                unlabelled == 0 ? null : $"unlabelled={unlabelled}",
                unlabelled == 0
                    ? null
                    : "برای بلوک‌ها زبان مشخص کنید، مثلاً ```csharp."),
        ];
    }
}

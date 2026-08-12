using HelpDev.SharedContracts.Ai;
using Microsoft.Extensions.Options;

namespace HelpDev.Infrastructure.Ai;

/// <summary>
/// Deterministic, non-LLM generator for tests and local development when ProviderName=Fake.
/// Does not invent rankings or scores. Output is clearly marked as Fake.
/// </summary>
public sealed class FakeAiTextGenerator : IAiTextGenerator
{
    private readonly AiProviderOptions _options;
    private readonly FakeAiFailureInjector _failureInjector;

    public FakeAiTextGenerator(IOptions<AiProviderOptions> options, FakeAiFailureInjector failureInjector)
    {
        _options = options.Value;
        _failureInjector = failureInjector;
    }

    public Task<AiGenerationResult> GenerateSafeAsync(
        AiTextRequest request,
        CancellationToken cancellationToken = default) =>
        AiTextGeneratorCompat.SafeFromAsync(ct => GenerateAsync(request, ct), "Fake", cancellationToken);

    public Task<AiTextResponse> GenerateAsync(AiTextRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        if (!_options.Enabled)
        {
            throw new InvalidOperationException("AI provider is disabled.");
        }

        if (_failureInjector.TryConsume(out var errorCode))
        {
            // Surface stable error codes so SafeFromAsync can map them.
            throw new InvalidOperationException(errorCode);
        }

        var input = request.InputText ?? string.Empty;
        var titleLine = ExtractFirstLine(input);
        var text = request.TaskType switch
        {
            "ContentAnalysis" => BuildAnalysis(titleLine, input),
            "TitleSuggestion" => BuildTitles(titleLine),
            "MetaDescription" => BuildMeta(titleLine, input),
            "OutlineGeneration" => BuildOutline(titleLine),
            "FaqGeneration" => BuildFaq(titleLine),
            "WorkflowResearch" => BuildWorkflowResearch(titleLine),
            "WorkflowOutline" => BuildWorkflowOutline(titleLine),
            "WorkflowDraft" => BuildWorkflowDraft(titleLine),
            "WorkflowSeo" => BuildWorkflowSeo(titleLine),
            "LearningRecommend" => BuildLearningRecommend(titleLine),
            "LearningRoadmap" => BuildLearningRoadmap(titleLine),
            _ => "Unsupported task type.",
        };

        var inputTokens = EstimateTokens(request.SystemInstruction) + EstimateTokens(input);
        var outputTokens = EstimateTokens(text);

        return Task.FromResult(new AiTextResponse(
            text,
            _options.Model,
            "Fake",
            new AiTokenUsage(inputTokens, outputTokens)));
    }

    private static string ExtractFirstLine(string input)
    {
        var line = input.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault() ?? "محتوا";
        return line.Length > 80 ? line[..80] : line;
    }

    private static string BuildAnalysis(string title, string input) =>
        $"""
        [Fake] تحلیل تحریریه‌ای برای «{title}»
        - طول تقریبی ورودی: {input.Length} نویسه
        - پیشنهاد: عنوان و توضیحات SEO را بازبینی کنید
        - پیشنهاد: ساختار سرفصل‌ها را شفاف‌تر کنید
        این خروجی مدل واقعی نیست و فقط برای توسعه/تست است.
        """;

    private static string BuildTitles(string title) =>
        $"""
        [Fake] پیشنهاد عنوان
        1. {title}
        2. راهنمای عملی: {title}
        3. {title} — نکات کلیدی
        """;

    private static string BuildMeta(string title, string input)
    {
        var snippet = input.Replace('\n', ' ').Trim();
        if (snippet.Length > 120)
        {
            snippet = snippet[..117] + "…";
        }

        return $"[Fake] توضیحات SEO پیشنهادی برای «{title}»: {snippet}";
    }

    private static string BuildOutline(string title) =>
        $"""
        [Fake] ساختار پیشنهادی برای «{title}»
        1. مقدمه
        2. مفاهیم پایه
        3. پیاده‌سازی گام‌به‌گام
        4. اشتباهات رایج
        5. جمع‌بندی
        """;

    private static string BuildFaq(string title) =>
        $"""
        [Fake] پرسش‌های متداول درباره «{title}»
        س: این موضوع برای چه کسانی مناسب است؟
        ج: خوانندگان سطح متوسط که می‌خواهند مفاهیم را عملی بیاموزند.
        س: پیش‌نیاز چیست؟
        ج: آشنایی اولیه با مفاهیم مرتبط کافی است.
        """;

    private static string BuildWorkflowResearch(string title) =>
        $"""
        [Fake] خلاصه پژوهشی برای «{title}»
        دانش بازیابی‌شده از HelpDev را مرور کنید و نکات اصلی را در پیش‌نویس لحاظ کنید.
        این خروجی مدل واقعی نیست.
        """;

    private static string BuildWorkflowOutline(string title) =>
        $"""
        # {title}
        ## مقدمه
        ### چرا این موضوع مهم است
        ## مفاهیم پایه
        ## پیاده‌سازی
        ### گام‌های عملی
        ## جمع‌بندی
        """;

    private static string BuildWorkflowDraft(string title) =>
        $"""
        # {title}

        [Fake] این پیش‌نویس آزمایشی است و باید توسط نویسنده بازبینی شود.

        ## مقدمه
        در این مقاله مفاهیم مرتبط با {title} را مرور می‌کنیم.

        ## مفاهیم پایه
        تعاریف و پیش‌نیازها را اینجا تکمیل کنید.

        ## پیاده‌سازی
        مراحل عملی را بر اساس تجربه HelpDev بنویسید.

        ## جمع‌بندی
        نکات کلیدی را خلاصه کنید.
        """;

    private static string BuildWorkflowSeo(string title) =>
        "{\"title\":\"[Fake] " + title.Replace('"', '\'') +
        "\",\"description\":\"[Fake] راهنمای عملی برای توسعه‌دهندگان HelpDev.\",\"keywords\":[\"helpdev\",\"guide\"]}";

    private static string BuildLearningRecommend(string title) =>
        $"""
        REASON: پیشنهادهای یادگیری بر اساس پروفایل و دانش HelpDev برای «{title}» توضیح داده شده‌اند. این خروجی مدل واقعی نیست.
        NEXT:
        - پروفایل یادگیری را تکمیل کنید
        - دوره مرتبط را ادامه دهید یا مرور کنید
        - مفاهیم پایه را از دانشنامه HelpDev بخوانید
        """;

    private static string BuildLearningRoadmap(string title)
    {
        var safe = title.Replace('"', '\'');
        return
            "{\"steps\":[" +
            "{\"title\":\"مبانی زبان و ابزارها\",\"description\":\"[Fake] مرور پیش‌نیازها\",\"relatedCourseId\":null}," +
            "{\"title\":\"مسیر تخصصی\",\"description\":\"[Fake] تمرکز روی هدف: " + safe + "\",\"relatedCourseId\":null}," +
            "{\"title\":\"API و سرویس‌ها\",\"description\":\"[Fake] کار با سرویس‌های کاربردی\",\"relatedCourseId\":null}," +
            "{\"title\":\"RAG و دانش\",\"description\":\"[Fake] بازیابی دانش HelpDev\",\"relatedCourseId\":null}," +
            "{\"title\":\"پروژه کاربردی\",\"description\":\"[Fake] ساخت یک پروژه کوچک\",\"relatedCourseId\":null}" +
            "]}";
    }

    private static int EstimateTokens(string? text) =>
        string.IsNullOrEmpty(text) ? 0 : Math.Max(1, text.Length / 4);
}

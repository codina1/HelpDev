using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Persistence;
using HelpDev.Modules.Content.Application.Tools;
using HelpDev.Modules.Content.Domain.Enums;
using HelpDev.SharedKernel.Time;

namespace HelpDev.Modules.Content.Application.Tools.Ai;

/// <summary>
/// Tool AI Assistant foundation — suggestion only; never persists automatically.
/// </summary>
public interface IToolAiAssistantService
{
    Task<ToolAiSuggestionDto> SuggestSummaryAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default);

    Task<ToolAiSuggestionDto> SuggestFeaturesAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default);
}

public sealed record ToolAiSuggestionDto(
    string Kind,
    string Title,
    string Body,
    IReadOnlyList<string> BulletSuggestions,
    bool RequiresHumanApply);

public sealed class ToolAiAssistantService : IToolAiAssistantService
{
    private readonly IContentRepository _contentRepository;
    private readonly IToolRepository _toolRepository;
    private readonly IDateTimeProvider _clock;

    public ToolAiAssistantService(
        IContentRepository contentRepository,
        IToolRepository toolRepository,
        IDateTimeProvider clock)
    {
        _contentRepository = contentRepository;
        _toolRepository = toolRepository;
        _clock = clock;
    }

    public async Task<ToolAiSuggestionDto> SuggestSummaryAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default)
    {
        var (content, tool) = await LoadAsync(actor, contentId, cancellationToken).ConfigureAwait(false);
        var name = tool?.ToolName ?? content.Title;
        var category = tool?.ToolCategory ?? "ابزار";
        var body =
            $"{name} یک {category} است" +
            (string.IsNullOrWhiteSpace(tool?.CompanyName) ? "" : $" از {tool!.CompanyName}") +
            ". این خلاصه پیشنهادی است و باید توسط نویسنده بررسی و اعمال شود.";

        return new ToolAiSuggestionDto(
            "summary",
            "پیشنهاد خلاصه ابزار",
            body,
            [
                $"تمرکز روی مخاطب {category}",
                "ذکر مدل قیمت و پلتفرم‌های اصلی",
                "اجتناب از ادعاهای رتبه‌بندی بدون منبع",
            ],
            RequiresHumanApply: true);
    }

    public async Task<ToolAiSuggestionDto> SuggestFeaturesAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default)
    {
        var (content, tool) = await LoadAsync(actor, contentId, cancellationToken).ConfigureAwait(false);
        var name = tool?.ToolName ?? content.Title;
        var existing = tool?.Features.Select(f => f.Title).ToArray() ?? [];
        var suggestions = new List<string>
        {
            $"{name} — قابلیت اصلی",
            "یکپارچگی با گردش‌کار توسعه",
            "پشتیبانی از همکاری تیمی",
            "مستندات و اکستنشن‌ها",
        };
        foreach (var title in existing)
        {
            suggestions.RemoveAll(s => s.Contains(title, StringComparison.OrdinalIgnoreCase));
        }

        return new ToolAiSuggestionDto(
            "features",
            "پیشنهاد استخراج ویژگی‌ها",
            "فهرست پیشنهادی ویژگی‌ها — فقط با تأیید انسان به متادیتا اضافه شود.",
            suggestions,
            RequiresHumanApply: true);
    }

    private async Task<(Domain.Entities.Content Content, Domain.Tools.ToolMetadata? Tool)> LoadAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken)
    {
        var content = await _contentRepository.GetByIdAsync(contentId, cancellationToken).ConfigureAwait(false);
        if (content is null)
        {
            throw new ContentException("محتوا یافت نشد.", ContentErrorCodes.NotFound);
        }

        ContentService.EnsureCanManage(content, actor);
        if (content.Type != ContentType.Tool)
        {
            throw new ContentException("این محتوا از نوع ابزار نیست.", ContentErrorCodes.Validation);
        }

        var tool = await _toolRepository.GetByContentIdAsync(content.Id, cancellationToken).ConfigureAwait(false);
        _ = _clock.UtcNow;
        return (content, tool);
    }
}

using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Persistence;
using HelpDev.Modules.Content.Domain.Enums;
using HelpDev.SharedKernel.Time;

namespace HelpDev.Modules.Content.Application.Roadmaps.Ai;

/// <summary>
/// Roadmap AI Assistant foundation — suggestion only; never persists automatically.
/// </summary>
public interface IRoadmapAiAssistantService
{
    Task<RoadmapAiSuggestionDto> SuggestOutlineAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default);

    Task<RoadmapAiSuggestionDto> SuggestPhasesAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default);

    Task<RoadmapAiSuggestionDto> SuggestTopicsAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default);
}

public sealed record RoadmapAiSuggestionDto(
    string Kind,
    string Title,
    string Body,
    IReadOnlyList<string> BulletSuggestions,
    bool RequiresHumanApply);

public sealed class RoadmapAiAssistantService : IRoadmapAiAssistantService
{
    private readonly IContentRepository _contentRepository;
    private readonly IRoadmapRepository _roadmapRepository;

    public RoadmapAiAssistantService(
        IContentRepository contentRepository,
        IRoadmapRepository roadmapRepository,
        IDateTimeProvider clock)
    {
        _contentRepository = contentRepository;
        _roadmapRepository = roadmapRepository;
        _ = clock;
    }

    public async Task<RoadmapAiSuggestionDto> SuggestOutlineAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default)
    {
        var (content, roadmap) = await LoadAsync(actor, contentId, cancellationToken).ConfigureAwait(false);
        var title = content.Title;
        var goal = roadmap?.Goal ?? "مهارت هدف";
        return new RoadmapAiSuggestionDto(
            "outline",
            "پیشنهاد ساختار کلی نقشه راه",
            $"برای «{title}» با هدف «{goal}» یک ساختار فازبندی‌شده پیشنهاد می‌شود. اعمال فقط با تأیید نویسنده.",
            [
                "فاز مبانی (مفاهیم پایه)",
                "فاز میانی (پروژه کوچک)",
                "فاز پیشرفته (پروژه واقعی)",
                "فاز تثبیت (مرور و تمرین)",
            ],
            RequiresHumanApply: true);
    }

    public async Task<RoadmapAiSuggestionDto> SuggestPhasesAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default)
    {
        var (content, roadmap) = await LoadAsync(actor, contentId, cancellationToken).ConfigureAwait(false);
        var existing = roadmap?.Steps.OrderBy(s => s.Order).Select(s => s.Title).ToArray() ?? [];
        var suggestions = existing.Length == 0
            ? new[] { "HTML & CSS", "JavaScript", "React", "Next.js" }
            : existing.Select(t => $"گسترش فاز: {t}").Take(4).ToArray();

        return new RoadmapAiSuggestionDto(
            "phases",
            "پیشنهاد فازها",
            $"فازهای پیشنهادی برای «{content.Title}». هیچ فازی خودکار ایجاد نمی‌شود.",
            suggestions,
            RequiresHumanApply: true);
    }

    public async Task<RoadmapAiSuggestionDto> SuggestTopicsAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default)
    {
        var (content, roadmap) = await LoadAsync(actor, contentId, cancellationToken).ConfigureAwait(false);
        var focus = roadmap?.Steps.OrderBy(s => s.Order).FirstOrDefault()?.Title ?? content.Title;
        return new RoadmapAiSuggestionDto(
            "topics",
            "پیشنهاد موضوعات",
            $"موضوعات پیشنهادی برای فاز «{focus}». تأیید انسانی لازم است.",
            ["مفاهیم پایه", "تمرین عملی", "اشکال‌زدایی", "پروژه کوچک"],
            RequiresHumanApply: true);
    }

    private async Task<(Domain.Entities.Content Content, Domain.Roadmaps.RoadmapMetadata? Roadmap)> LoadAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(actor);
        var content = await _contentRepository.GetByIdAsync(contentId, cancellationToken).ConfigureAwait(false);
        if (content is null)
        {
            throw new ContentException("محتوا یافت نشد.", ContentErrorCodes.NotFound);
        }

        ContentService.EnsureCanManage(content, actor);
        if (content.Type != ContentType.Roadmap)
        {
            throw new ContentException("این محتوا از نوع نقشه راه نیست.", ContentErrorCodes.Validation);
        }

        var roadmap = await _roadmapRepository.GetByContentIdAsync(content.Id, cancellationToken).ConfigureAwait(false);
        return (content, roadmap);
    }
}

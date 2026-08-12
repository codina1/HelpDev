using HelpDev.Modules.Content.Application.Contents;

namespace HelpDev.Modules.Content.Application.ContentAi;

public interface IContentAiAssistantService
{
    Task<ContentAiResultDto> AnalyzeContentAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default);

    Task<ContentAiResultDto> GenerateTitleSuggestionsAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default);

    Task<ContentAiResultDto> GenerateMetaDescriptionAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default);

    Task<ContentAiResultDto> GenerateOutlineAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default);

    Task<ContentAiResultDto> GenerateFaqAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default);
}

public sealed record ContentAiResultDto(
    string TaskType,
    string GeneratedText,
    DateTime CreatedAtUtc,
    string Model,
    string Provider);

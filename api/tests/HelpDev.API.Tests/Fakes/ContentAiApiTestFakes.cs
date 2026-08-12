using HelpDev.Modules.Content.Application.ContentAi;
using HelpDev.Modules.Content.Application.Contents;

namespace HelpDev.API.Tests.Fakes;

internal sealed class FakeContentAiAssistantService : IContentAiAssistantService
{
    public ContentManagementActor? LastActor { get; private set; }

    public Guid? LastContentId { get; private set; }

    public string? LastOperation { get; private set; }

    public Exception? ExceptionToThrow { get; set; }

    public ContentAiResultDto ResultToReturn { get; set; } = new(
        "ContentAnalysis",
        "sample generated text",
        new DateTime(2026, 7, 22, 12, 0, 0, DateTimeKind.Utc),
        "fake-v1",
        "Fake");

    public Task<ContentAiResultDto> AnalyzeContentAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default) =>
        Complete(actor, contentId, nameof(AnalyzeContentAsync));

    public Task<ContentAiResultDto> GenerateTitleSuggestionsAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default) =>
        Complete(actor, contentId, nameof(GenerateTitleSuggestionsAsync));

    public Task<ContentAiResultDto> GenerateMetaDescriptionAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default) =>
        Complete(actor, contentId, nameof(GenerateMetaDescriptionAsync));

    public Task<ContentAiResultDto> GenerateOutlineAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default) =>
        Complete(actor, contentId, nameof(GenerateOutlineAsync));

    public Task<ContentAiResultDto> GenerateFaqAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default) =>
        Complete(actor, contentId, nameof(GenerateFaqAsync));

    private Task<ContentAiResultDto> Complete(
        ContentManagementActor actor,
        Guid contentId,
        string operation)
    {
        if (ExceptionToThrow is not null)
        {
            throw ExceptionToThrow;
        }

        LastActor = actor;
        LastContentId = contentId;
        LastOperation = operation;
        return Task.FromResult(ResultToReturn);
    }
}

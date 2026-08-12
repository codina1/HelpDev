using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Tools.Ai;

namespace HelpDev.API.Tests.Fakes;

internal sealed class FakeToolAiAssistantService : IToolAiAssistantService
{
    public Task<ToolAiSuggestionDto> SuggestSummaryAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new ToolAiSuggestionDto("summary", "t", "b", [], true));

    public Task<ToolAiSuggestionDto> SuggestFeaturesAsync(
        ContentManagementActor actor,
        Guid contentId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new ToolAiSuggestionDto("features", "t", "b", [], true));
}

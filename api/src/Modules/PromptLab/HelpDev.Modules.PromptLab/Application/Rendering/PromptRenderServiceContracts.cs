using System.Text.Json;

namespace HelpDev.Modules.PromptLab.Application.Rendering;

public sealed record RenderPromptRequest(IReadOnlyDictionary<string, JsonElement> Values);

public sealed record PromptRenderResultDto(
    Guid? RenderId,
    string PromptSlug,
    int VersionNumber,
    bool Succeeded,
    string RenderedText,
    string? ErrorCode,
    string? ErrorMessage,
    int DurationMilliseconds,
    DateTime RenderedAtUtc);

public interface IPromptRenderService
{
    Task<PromptRenderResultDto> RenderAsync(
        string slug,
        RenderPromptRequest request,
        Guid? userId = null,
        CancellationToken cancellationToken = default);
}

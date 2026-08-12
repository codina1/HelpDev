namespace HelpDev.Modules.Search.Application.Rag;

public sealed record RagSourceDto(
    string Title,
    string SourceUrl,
    string SourceType,
    Guid SourceId,
    double Similarity);

public sealed record RagAnswerDto(
    string Answer,
    IReadOnlyList<RagSourceDto> Sources,
    DateTime GeneratedAtUtc);

public interface IRagAnswerService
{
    Task<RagAnswerDto> AskAsync(string question, CancellationToken cancellationToken = default);
}

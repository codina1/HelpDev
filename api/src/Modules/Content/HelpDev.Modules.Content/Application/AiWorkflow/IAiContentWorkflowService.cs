using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Domain.AiWorkflow;

namespace HelpDev.Modules.Content.Application.AiWorkflow;

/// <summary>
/// Port for RAG knowledge retrieval. Implemented outside Content (Infrastructure adapter → Search).
/// </summary>
public interface IWorkflowKnowledgeRetriever
{
    Task<WorkflowKnowledgeContext> RetrieveAsync(
        string topic,
        int take = 8,
        CancellationToken cancellationToken = default);
}

public sealed record WorkflowKnowledgeSource(
    string Title,
    string Url,
    string SourceType,
    string Snippet,
    double Similarity);

public sealed record WorkflowKnowledgeContext(
    string Topic,
    IReadOnlyList<WorkflowKnowledgeSource> Sources);

public interface IContentIdeaRepository
{
    Task<ContentIdea?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(ContentIdea idea, CancellationToken cancellationToken = default);
}

public interface IAiContentWorkflowSessionRepository
{
    Task<AiContentWorkflowSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(AiContentWorkflowSession session, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AiContentWorkflowSession>> ListByCreatorAsync(
        Guid? createdByUserId,
        CancellationToken cancellationToken = default);
}

public interface IAiResearchService
{
    Task<AiResearchResultDto> ResearchAsync(
        ContentManagementActor actor,
        Guid workflowId,
        CancellationToken cancellationToken = default);
}

public interface IAiContentWorkflowService
{
    Task<AiContentWorkflowSessionDto> CreateAsync(
        ContentManagementActor actor,
        CreateAiContentWorkflowRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AiContentWorkflowListItemDto>> ListAsync(
        ContentManagementActor actor,
        CancellationToken cancellationToken = default);

    Task<AiContentWorkflowSessionDto> GetByIdAsync(
        ContentManagementActor actor,
        Guid workflowId,
        CancellationToken cancellationToken = default);

    Task<AiResearchResultDto> ResearchAsync(
        ContentManagementActor actor,
        Guid workflowId,
        CancellationToken cancellationToken = default);

    Task<ContentOutlineDto> GenerateOutlineAsync(
        ContentManagementActor actor,
        Guid workflowId,
        GenerateOutlineRequest request,
        CancellationToken cancellationToken = default);

    Task<DraftSuggestionDto> GenerateDraftAsync(
        ContentManagementActor actor,
        Guid workflowId,
        GenerateDraftRequest request,
        CancellationToken cancellationToken = default);

    Task<SeoOptimizationSuggestionDto> GenerateSeoAsync(
        ContentManagementActor actor,
        Guid workflowId,
        GenerateSeoRequest request,
        CancellationToken cancellationToken = default);

    Task<ApplyDraftResultDto> ApplyDraftAsync(
        ContentManagementActor actor,
        Guid workflowId,
        ApplyDraftRequest request,
        CancellationToken cancellationToken = default);
}

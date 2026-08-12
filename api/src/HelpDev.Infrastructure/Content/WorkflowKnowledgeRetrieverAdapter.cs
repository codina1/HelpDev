using HelpDev.Modules.Content.Application.AiWorkflow;
using HelpDev.Modules.Search.Application.Rag;

namespace HelpDev.Infrastructure.Content;

/// <summary>Bridges Content workflow research to Search RAG context (no Content→Search project reference).</summary>
public sealed class WorkflowKnowledgeRetrieverAdapter : IWorkflowKnowledgeRetriever
{
    private readonly IRagContextBuilder _ragContextBuilder;

    public WorkflowKnowledgeRetrieverAdapter(IRagContextBuilder ragContextBuilder)
    {
        _ragContextBuilder = ragContextBuilder;
    }

    public async Task<WorkflowKnowledgeContext> RetrieveAsync(
        string topic,
        int take = 8,
        CancellationToken cancellationToken = default)
    {
        var context = await _ragContextBuilder.BuildAsync(topic, cancellationToken);
        var sources = context.Chunks
            .Take(take)
            .Select(chunk => new WorkflowKnowledgeSource(
                chunk.Title,
                chunk.Url,
                chunk.SourceType,
                chunk.Snippet,
                chunk.Similarity))
            .ToList();

        return new WorkflowKnowledgeContext(topic, sources);
    }
}

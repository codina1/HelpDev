using HelpDev.Modules.Learning.Application.Personalization;
using HelpDev.Modules.Search.Application.Rag;

namespace HelpDev.Infrastructure.Learning;

/// <summary>Bridges Learning personalization to Search RAG (no Learning→Search project reference).</summary>
public sealed class LearningKnowledgeRetrieverAdapter : ILearningKnowledgeRetriever
{
    private readonly IRagContextBuilder _ragContextBuilder;

    public LearningKnowledgeRetrieverAdapter(IRagContextBuilder ragContextBuilder)
    {
        _ragContextBuilder = ragContextBuilder;
    }

    public async Task<LearningKnowledgeContext> RetrieveAsync(
        string topic,
        int take = 8,
        CancellationToken cancellationToken = default)
    {
        var context = await _ragContextBuilder.BuildAsync(topic, cancellationToken);
        var sources = context.Chunks
            .Take(take)
            .Select(chunk => new LearningKnowledgeSnippet(
                chunk.Title,
                chunk.Url,
                chunk.SourceType,
                chunk.Snippet))
            .ToList();

        return new LearningKnowledgeContext(topic, sources);
    }
}

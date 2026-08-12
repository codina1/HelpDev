namespace HelpDev.Modules.Search.Application.Chunking;

/// <summary>Deterministic knowledge chunker for all HelpDev source kinds.</summary>
public interface IKnowledgeChunker
{
    IReadOnlyList<ContentChunkDto> Chunk(string title, string body, string? sourceUrl = null);
}

/// <summary>
/// Alias implementation: markdown/plain splitter shared with content indexing.
/// </summary>
public sealed class MarkdownKnowledgeChunker : IKnowledgeChunker, IContentChunker
{
    private readonly MarkdownContentChunker _inner;

    public MarkdownKnowledgeChunker()
        : this(new MarkdownContentChunker())
    {
    }

    public MarkdownKnowledgeChunker(MarkdownContentChunker inner)
    {
        _inner = inner;
    }

    public IReadOnlyList<ContentChunkDto> Chunk(string title, string body, string? sourceUrl = null) =>
        _inner.Chunk(title, body, sourceUrl);
}

namespace HelpDev.Modules.Search.Application.Chunking;

public sealed record ContentChunkDto(
    int ChunkIndex,
    string Content,
    string Title,
    string? Metadata);

/// <summary>Deterministic, non-AI content splitter. No persistence.</summary>
public interface IContentChunker
{
    IReadOnlyList<ContentChunkDto> Chunk(string title, string body, string? sourceUrl = null);
}

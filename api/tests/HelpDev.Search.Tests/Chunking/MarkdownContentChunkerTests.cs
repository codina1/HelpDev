using HelpDev.Modules.Search.Application.Chunking;

namespace HelpDev.Search.Tests.Chunking;

public sealed class MarkdownContentChunkerTests
{
    [Fact]
    public void Chunk_splits_by_headings_deterministically()
    {
        var chunker = new MarkdownContentChunker();
        var body = """
            Intro paragraph about HelpDev.

            ## Setup
            Install the tools and configure PostgreSQL.

            ## Usage
            Publish content and search.
            """;

        var first = chunker.Chunk("Guide", body, "/content/guide");
        var second = chunker.Chunk("Guide", body, "/content/guide");

        Assert.Equal(first.Count, second.Count);
        Assert.Equal(first.Select(c => c.Content), second.Select(c => c.Content));
        Assert.Contains(first, c => c.Title.Contains("Setup", StringComparison.Ordinal));
        Assert.All(first, c => Assert.False(string.IsNullOrWhiteSpace(c.Content)));
    }

    [Fact]
    public void Chunk_respects_max_size()
    {
        var chunker = new MarkdownContentChunker(maxChunkChars: 200, minChunkChars: 20);
        var body = string.Join("\n\n", Enumerable.Range(0, 20).Select(i => $"Paragraph {i} " + new string('x', 80)));

        var chunks = chunker.Chunk("Long", body);
        Assert.NotEmpty(chunks);
        Assert.All(chunks, c => Assert.True(c.Content.Length <= 200));
    }

    [Fact]
    public void Chunk_returns_empty_for_blank_body()
    {
        var chunker = new MarkdownContentChunker();
        Assert.Empty(chunker.Chunk("Title", "   "));
    }
}

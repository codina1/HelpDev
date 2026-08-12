using HelpDev.Modules.Search.Application.Chunking;
using HelpDev.Modules.Search.Application.Rag;
using HelpDev.Modules.Search.Application.Semantic;
using HelpDev.Modules.Search.Domain;

namespace HelpDev.Search.Tests.Knowledge;

public sealed class KnowledgeSourceTypeTests
{
    [Fact]
    public void All_known_types_are_normalized()
    {
        foreach (var type in KnowledgeSourceType.All)
        {
            Assert.Equal(type, SearchSourceTypes.NormalizeOrThrow(type));
            Assert.True(SearchSourceTypes.IsKnown(type));
        }
    }

    [Fact]
    public void Unknown_type_throws()
    {
        Assert.Throws<ArgumentException>(() => SearchSourceTypes.NormalizeOrThrow("blog"));
    }
}

public sealed class MarkdownKnowledgeChunkerTests
{
    [Theory]
    [InlineData("Article", "# Intro\n\nBody paragraph about HelpDev.")]
    [InlineData("Course", "Learn C# with HelpDev courses.")]
    [InlineData("Tool", "JSON formatter tool description.")]
    [InlineData("Prompt", "You are a helpful coding assistant.\n\n{{input}}")]
    public void Chunks_source_kinds_deterministically(string title, string body)
    {
        var chunker = new MarkdownKnowledgeChunker();
        var a = chunker.Chunk(title, body, "/x");
        var b = chunker.Chunk(title, body, "/x");

        Assert.NotEmpty(a);
        Assert.Equal(a.Count, b.Count);
        Assert.Equal(a[0].Content, b[0].Content);
    }
}

public sealed class RagContextBuilderTests
{
    [Fact]
    public async Task Build_orders_by_similarity_and_removes_duplicates()
    {
        var id = Guid.NewGuid();
        var semantic = new StubSemantic
        {
            Result = new SearchContextDto(
                "q",
                [
                    new SearchContextItemDto("B", "second", "/b", "tool", Guid.NewGuid(), 0.5),
                    new SearchContextItemDto("A", "first", "/a", "content", id, 0.9),
                    new SearchContextItemDto("A-dup", "dup", "/a2", "content", id, 0.8),
                ]),
        };

        var builder = new RagContextBuilder(semantic, take: 8, maxContextChars: 6000);
        var context = await builder.BuildAsync("question");

        Assert.Equal(2, context.Chunks.Count);
        Assert.Equal("A", context.Chunks[0].Title);
        Assert.Equal("B", context.Chunks[1].Title);
    }

    [Fact]
    public async Task Build_respects_max_context_size()
    {
        var semantic = new StubSemantic
        {
            Result = new SearchContextDto(
                "q",
                [
                    new SearchContextItemDto("One", new string('a', 200), "/1", "content", Guid.NewGuid(), 0.9),
                    new SearchContextItemDto("Two", new string('b', 200), "/2", "course", Guid.NewGuid(), 0.8),
                ]),
        };

        var builder = new RagContextBuilder(semantic, take: 8, maxContextChars: 250);
        var context = await builder.BuildAsync("q");

        Assert.Single(context.Chunks);
    }

    private sealed class StubSemantic : ISemanticSearchQueries
    {
        public SearchContextDto Result { get; set; } = new("q", []);

        public Task<SearchContextDto> SearchSimilarAsync(string query, int take = 8, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result);

        public Task<SearchContextDto> RetrieveContextAsync(string query, int take = 6, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result);

        public Task<SearchContextDto> SearchRelatedToSourceAsync(string sourceType, Guid sourceId, int take = 6, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result);
    }
}

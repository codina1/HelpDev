using HelpDev.Modules.Search.Application.Semantic;

namespace HelpDev.Modules.Search.Application.Rag;

public sealed record RagContextSource(
    string Title,
    string SourceType,
    Guid SourceId,
    string Url,
    string Snippet,
    double Similarity);

public sealed record RagContext(
    string Question,
    IReadOnlyList<RagContextSource> Chunks,
    int MaxContextChars);

public interface IRagContextBuilder
{
    Task<RagContext> BuildAsync(string question, CancellationToken cancellationToken = default);
}

/// <summary>
/// Deterministic RAG context assembly: similarity order, duplicate removal, size cap.
/// </summary>
public sealed class RagContextBuilder : IRagContextBuilder
{
    public const int DefaultTake = 8;
    public const int DefaultMaxContextChars = 6000;

    private readonly ISemanticSearchQueries _semanticSearch;
    private readonly int _take;
    private readonly int _maxContextChars;

    public RagContextBuilder(
        ISemanticSearchQueries semanticSearch,
        int take = DefaultTake,
        int maxContextChars = DefaultMaxContextChars)
    {
        _semanticSearch = semanticSearch;
        _take = take < 1 ? DefaultTake : take;
        _maxContextChars = maxContextChars < 1 ? DefaultMaxContextChars : maxContextChars;
    }

    public async Task<RagContext> BuildAsync(string question, CancellationToken cancellationToken = default)
    {
        var trimmed = (question ?? string.Empty).Trim();
        var retrieved = await _semanticSearch.RetrieveContextAsync(trimmed, _take, cancellationToken);

        var selected = new List<RagContextSource>();
        var seen = new HashSet<(string Type, Guid Id)>();
        var usedChars = 0;

        foreach (var item in retrieved.Items.OrderByDescending(i => i.Similarity).ThenBy(i => i.Title, StringComparer.Ordinal))
        {
            var key = (item.SourceType, item.SourceId);
            if (!seen.Add(key))
            {
                continue;
            }

            var snippet = item.Snippet ?? string.Empty;
            var projected = snippet.Length + item.Title.Length + 32;
            if (selected.Count > 0 && usedChars + projected > _maxContextChars)
            {
                break;
            }

            selected.Add(new RagContextSource(
                item.Title,
                item.SourceType,
                item.SourceId,
                item.SourceUrl,
                snippet,
                item.Similarity));
            usedChars += projected;
        }

        return new RagContext(trimmed, selected, _maxContextChars);
    }
}

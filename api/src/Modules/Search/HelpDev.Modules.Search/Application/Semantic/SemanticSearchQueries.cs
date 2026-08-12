using System.Text.Json;
using HelpDev.Modules.Search.Application.Persistence;
using HelpDev.Modules.Search.Domain;
using HelpDev.SharedContracts.Ai;

namespace HelpDev.Modules.Search.Application.Semantic;

public sealed class SemanticSearchQueries : ISemanticSearchQueries
{
    public const int MaxTake = 20;
    public const double MinSimilarity = 0.05;

    private readonly IEmbeddingGenerator _embeddingGenerator;
    private readonly ISearchVectorRepository _vectorRepository;
    private readonly ISearchChunkRepository _chunkRepository;

    public SemanticSearchQueries(
        IEmbeddingGenerator embeddingGenerator,
        ISearchVectorRepository vectorRepository,
        ISearchChunkRepository chunkRepository)
    {
        _embeddingGenerator = embeddingGenerator;
        _vectorRepository = vectorRepository;
        _chunkRepository = chunkRepository;
    }

    public Task<SearchContextDto> SearchSimilarAsync(
        string query,
        int take = 8,
        CancellationToken cancellationToken = default) =>
        SearchCoreAsync(query, take, cancellationToken);

    public Task<SearchContextDto> RetrieveContextAsync(
        string query,
        int take = 6,
        CancellationToken cancellationToken = default) =>
        SearchCoreAsync(query, take, cancellationToken);

    public async Task<SearchContextDto> SearchRelatedToSourceAsync(
        string sourceType,
        Guid sourceId,
        int take = 6,
        CancellationToken cancellationToken = default)
    {
        var normalized = SearchSourceTypes.NormalizeOrThrow(sourceType);
        var chunks = await _chunkRepository.ListBySourceAsync(normalized, sourceId, cancellationToken);
        if (chunks.Count == 0)
        {
            return new SearchContextDto(string.Empty, []);
        }

        // Use the first chunk text as an anchor query (no user prompt storage).
        var anchor = chunks[0].Content;
        var result = await SearchCoreAsync(anchor, take + 5, cancellationToken);
        var filtered = result.Items
            .Where(item => !(item.SourceType == normalized && item.SourceId == sourceId))
            .Take(ClampTake(take))
            .ToList();

        return new SearchContextDto(chunks[0].Title, filtered);
    }

    private async Task<SearchContextDto> SearchCoreAsync(
        string query,
        int take,
        CancellationToken cancellationToken)
    {
        var trimmed = (query ?? string.Empty).Trim();
        if (trimmed.Length < 2)
        {
            return new SearchContextDto(trimmed, []);
        }

        var limitedTake = ClampTake(take);
        var embedding = await _embeddingGenerator.GenerateAsync(trimmed, cancellationToken);
        var hits = await _vectorRepository.SearchSimilarAsync(embedding.Vector, limitedTake, cancellationToken);

        var items = hits
            .Where(hit => hit.Similarity >= MinSimilarity)
            .Select(Map)
            .GroupBy(item => new { item.SourceType, item.SourceId })
            .Select(group => group.OrderByDescending(item => item.Similarity).First())
            .OrderByDescending(item => item.Similarity)
            .Take(limitedTake)
            .ToList();

        return new SearchContextDto(trimmed, items);
    }

    private static SearchContextItemDto Map(SemanticHit hit)
    {
        var url = ExtractUrl(hit.Metadata) ?? string.Empty;
        var snippet = hit.Content.Length <= 280 ? hit.Content : hit.Content[..277] + "…";
        return new SearchContextItemDto(
            hit.Title,
            snippet,
            url,
            hit.SourceType,
            hit.SourceId,
            Math.Round(hit.Similarity, 4));
    }

    private static string? ExtractUrl(string? metadata)
    {
        if (string.IsNullOrWhiteSpace(metadata))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(metadata);
            if (doc.RootElement.TryGetProperty("url", out var url) && url.ValueKind == JsonValueKind.String)
            {
                return url.GetString();
            }
        }
        catch (JsonException)
        {
            // Ignore malformed metadata.
        }

        return null;
    }

    private static int ClampTake(int take) =>
        take < 1 ? 1 : Math.Min(take, MaxTake);
}

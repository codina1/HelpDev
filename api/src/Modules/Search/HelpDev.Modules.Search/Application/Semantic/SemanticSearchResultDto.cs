namespace HelpDev.Modules.Search.Application.Semantic;

/// <summary>Public semantic search hit — never exposes vectors or internal chunk IDs.</summary>
public sealed record SemanticSearchResultDto(
    string Title,
    string Type,
    string Snippet,
    string Url,
    double Similarity);

public sealed record SemanticSearchResponseDto(
    string Query,
    IReadOnlyList<SemanticSearchResultDto> Results);

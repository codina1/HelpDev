using HelpDev.Modules.Search.Domain;

namespace HelpDev.Search.Tests;

/// <summary>
/// Mirrors the deterministic CASE ranking used by SearchQueries (exact → prefix → title contains → summary → newer → Id).
/// </summary>
public sealed class SearchRankingOrderTests
{
    [Fact]
    public void Ranking_prefers_exact_then_prefix_then_title_contains_then_summary()
    {
        var docs = new[]
        {
            Doc("summary-only", "zzz", "learn csharp here", updated: 1),
            Doc("title-contains", "intro to csharp", "x", updated: 1),
            Doc("prefix", "csharp advanced", "x", updated: 1),
            Doc("exact", "csharp", "x", updated: 1),
            Doc("exact-newer", "csharp", "x", updated: 2),
        };

        var ordered = docs
            .OrderBy(document => Rank(document.Title, document.Summary, "csharp"))
            .ThenByDescending(document => document.SourceUpdatedAtUtc)
            .ThenBy(document => document.Id)
            .Select(document => document.Slug)
            .ToArray();

        Assert.Equal(
            ["exact-newer", "exact", "prefix", "title-contains", "summary-only"],
            ordered);
    }

    private static int Rank(string title, string summary, string query)
    {
        if (title.Equals(query, StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }

        if (title.StartsWith(query, StringComparison.OrdinalIgnoreCase))
        {
            return 1;
        }

        if (title.Contains(query, StringComparison.OrdinalIgnoreCase))
        {
            return 2;
        }

        if (summary.Contains(query, StringComparison.OrdinalIgnoreCase))
        {
            return 3;
        }

        return 4;
    }

    private static SearchDocument Doc(string slug, string title, string summary, int updated) =>
        new()
        {
            Id = Guid.Parse($"00000000-0000-0000-0000-00000000000{updated}"),
            SourceType = SearchSourceTypes.Content,
            SourceId = Guid.NewGuid(),
            Title = title,
            Slug = slug,
            Summary = summary,
            Url = $"/content/{slug}",
            IsPublished = true,
            SourceUpdatedAtUtc = new DateTime(2026, 1, updated, 0, 0, 0, DateTimeKind.Utc),
            IndexedAtUtc = DateTime.UtcNow,
            LastEventId = Guid.NewGuid(),
        };
}

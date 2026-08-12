using System.Text.Json;

namespace HelpDev.Integration.Tests.Helpers;

public static class PaginationAssertionHelper
{
    public static void AssertNoDuplicateIds(JsonElement page, string idPropertyName = "id")
    {
        Assert.True(page.TryGetProperty("items", out var items));
        var ids = items.EnumerateArray()
            .Select(item => item.GetProperty(idPropertyName).GetGuid())
            .ToList();

        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    public static void AssertNoDuplicateIdsAcrossPages(JsonElement page1, JsonElement page2, string idPropertyName = "id")
    {
        Assert.True(page1.TryGetProperty("items", out var items1));
        Assert.True(page2.TryGetProperty("items", out var items2));

        var ids1 = items1.EnumerateArray().Select(item => item.GetProperty(idPropertyName).GetGuid()).ToHashSet();
        var ids2 = items2.EnumerateArray().Select(item => item.GetProperty(idPropertyName).GetGuid()).ToList();

        Assert.Empty(ids2.Where(ids1.Contains));
        AssertNoDuplicateIds(page1, idPropertyName);
        AssertNoDuplicateIds(page2, idPropertyName);
    }

    public static Task EventuallyAsync(
        Func<Task> assertion,
        TimeSpan? timeout = null,
        TimeSpan? pollInterval = null,
        CancellationToken cancellationToken = default) =>
        EventuallyAsyncHelper.EventuallyAsync(assertion, timeout, pollInterval, cancellationToken);
}

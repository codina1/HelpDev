using System.Net.Http.Json;
using System.Text.Json;
using HelpDev.Infrastructure.Persistence;
using HelpDev.Modules.Auditing.Domain.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDev.Integration.Tests.Helpers;

public static class AuditAssertionHelper
{
    private static readonly string[] DefaultSensitiveSubstrings =
    [
        "otp",
        "eyJ",
        "Bearer ",
        "password",
        "SETTING_SECRET_SENTINEL",
        "TOOL_INPUT_PRIVATE_SENTINEL",
        "SEARCH_QUERY_SENTINEL_SECRET",
    ];

    public static async Task<JsonElement> GetAuditPageAsync(
        HttpClient adminClient,
        string? action = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var url = $"/api/admin/audit?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(action))
        {
            url += $"&action={Uri.EscapeDataString(action)}";
        }

        using var response = await adminClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        return document.RootElement.Clone();
    }

    public static async Task<IReadOnlyList<AuditRecord>> GetAuditRecordsFromDbAsync(
        HelpDevWebApplicationFactory factory,
        string? action = null,
        CancellationToken cancellationToken = default)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var query = context.AuditRecords.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(action))
        {
            query = query.Where(record => record.Action == action);
        }

        return await query.OrderBy(record => record.OccurredAtUtc).ToListAsync(cancellationToken);
    }

    public static void AssertHasMetadataKeys(JsonElement auditItem, params string[] keys)
    {
        Assert.True(auditItem.TryGetProperty("metadata", out var metadata));
        Assert.Equal(JsonValueKind.Object, metadata.ValueKind);
        foreach (var key in keys)
        {
            Assert.True(metadata.TryGetProperty(key, out _), $"Expected metadata key '{key}'.");
        }
    }

    public static void AssertMetadataLacksSensitive(
        JsonElement auditItem,
        params string[] additionalSentinels)
    {
        if (!auditItem.TryGetProperty("metadata", out var metadata)
            || metadata.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return;
        }

        var serialized = metadata.GetRawText();
        foreach (var sentinel in DefaultSensitiveSubstrings.Concat(additionalSentinels))
        {
            Assert.DoesNotContain(sentinel, serialized, StringComparison.OrdinalIgnoreCase);
        }
    }

    public static void AssertNoSensitiveInRecord(
        AuditRecord record,
        params string[] additionalSentinels)
    {
        var blob = string.Join(
            "|",
            record.Action,
            record.SubjectDisplay,
            record.ReasonCode,
            record.CorrelationId,
            record.RequestPathTemplate,
            record.Metadata is null
                ? string.Empty
                : string.Join(";", record.Metadata.Select(pair => $"{pair.Key}={pair.Value}")));

        foreach (var sentinel in DefaultSensitiveSubstrings.Concat(additionalSentinels))
        {
            Assert.DoesNotContain(sentinel, blob, StringComparison.OrdinalIgnoreCase);
        }
    }

    public static JsonElement SingleItemByAction(JsonElement page, string action)
    {
        Assert.True(page.TryGetProperty("items", out var items));
        var matches = items.EnumerateArray()
            .Where(item => item.GetProperty("action").GetString() == action)
            .ToList();
        Assert.Single(matches);
        return matches[0];
    }
}

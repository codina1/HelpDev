using System.Net;
using System.Text.Json;
using HelpDev.Testing.PostgreSQL;

namespace HelpDev.Integration.Tests.OpenApi;

[Collection(PostgreSqlCollection.Name)]
[Trait("Category", "Integration")]
[Trait("Category", "EndToEnd")]
[Trait("Category", "OpenApi")]
public sealed class OpenApiContractConsistencyTests : IntegrationTestClassBase
{
    public OpenApiContractConsistencyTests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    [PostgreSqlFact]
    public async Task Public_openapi_documents_rate_limit_errors()
    {
        using var response = await Client.GetAsync("/openapi/public-v1.json");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadAsStringAsync();
        Assert.Contains("security_rate_limit_exceeded", payload, StringComparison.Ordinal);
        Assert.Contains("\"429\"", payload, StringComparison.Ordinal);
    }

    [PostgreSqlFact]
    public async Task Public_openapi_search_operation_documents_pagination_fields()
    {
        using var response = await Client.GetAsync("/openapi/public-v1.json");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);

        var paths = document.RootElement.GetProperty("paths");
        Assert.True(paths.TryGetProperty("/api/v1/search", out var searchPath));

        var searchOperation = searchPath.GetProperty("get");
        Assert.Equal("Search_Search", searchOperation.GetProperty("operationId").GetString());

        var parameterNames = searchOperation.GetProperty("parameters")
            .EnumerateArray()
            .Select(parameter => parameter.GetProperty("name").GetString())
            .Where(name => name is not null)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("page", parameterNames);
        Assert.Contains("pageSize", parameterNames);
    }
}

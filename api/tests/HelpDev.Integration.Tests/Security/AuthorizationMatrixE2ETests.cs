using System.Net;
using System.Net.Http.Json;
using HelpDev.Integration.Tests.Helpers;
using HelpDev.Testing.PostgreSQL;

namespace HelpDev.Integration.Tests.Security;

[Collection(PostgreSqlCollection.Name)]
[Trait("Category", "Integration")]
[Trait("Category", "EndToEnd")]
[Trait("Category", "Security")]
public sealed class AuthorizationMatrixE2ETests : IntegrationTestClassBase
{
    public AuthorizationMatrixE2ETests(PostgreSqlFixture fixture)
        : base(fixture)
    {
    }

    public static IEnumerable<object[]> ReadMatrix()
    {
        // role, method, path, expected status
        yield return ["Anonymous", "GET", "/api/content", HttpStatusCode.OK];
        yield return ["User", "GET", "/api/content", HttpStatusCode.OK];
        yield return ["Admin", "GET", "/api/content", HttpStatusCode.OK];

        yield return ["Anonymous", "GET", "/health/live", HttpStatusCode.OK];
        yield return ["User", "GET", "/health/live", HttpStatusCode.OK];
        yield return ["Admin", "GET", "/health/live", HttpStatusCode.OK];

        yield return ["Anonymous", "GET", "/api/profile/me", HttpStatusCode.Unauthorized];
        yield return ["User", "GET", "/api/profile/me", HttpStatusCode.OK];
        yield return ["Admin", "GET", "/api/profile/me", HttpStatusCode.OK];

        yield return ["Anonymous", "GET", "/api/admin/audit", HttpStatusCode.Unauthorized];
        yield return ["User", "GET", "/api/admin/audit", HttpStatusCode.Forbidden];
        yield return ["Admin", "GET", "/api/admin/audit", HttpStatusCode.OK];

        yield return ["Anonymous", "GET", "/api/admin/operations/status", HttpStatusCode.Unauthorized];
        yield return ["User", "GET", "/api/admin/operations/status", HttpStatusCode.Forbidden];
        yield return ["Admin", "GET", "/api/admin/operations/status", HttpStatusCode.OK];

        yield return ["Anonymous", "GET", "/api/admin/features", HttpStatusCode.Unauthorized];
        yield return ["User", "GET", "/api/admin/features", HttpStatusCode.Forbidden];
        yield return ["Admin", "GET", "/api/admin/features", HttpStatusCode.OK];

        yield return ["Anonymous", "GET", "/api/tools", HttpStatusCode.OK];
        yield return ["User", "GET", "/api/tools", HttpStatusCode.OK];
        yield return ["Admin", "GET", "/api/tools", HttpStatusCode.OK];

        yield return ["Anonymous", "GET", "/api/prompts", HttpStatusCode.OK];
        yield return ["User", "GET", "/api/prompts", HttpStatusCode.OK];
        yield return ["Admin", "GET", "/api/prompts", HttpStatusCode.OK];

        yield return ["Anonymous", "GET", "/api/search?q=test", HttpStatusCode.OK];
        yield return ["User", "GET", "/api/search?q=test", HttpStatusCode.OK];
        yield return ["Admin", "GET", "/api/search?q=test", HttpStatusCode.OK];
    }

    [Theory]
    [MemberData(nameof(ReadMatrix))]
    public async Task Authorization_matrix_returns_expected_status(
        string role,
        string method,
        string path,
        HttpStatusCode expected)
    {
        using var client = role switch
        {
            "Anonymous" => AuthClients.CreateAnonymousClient(),
            "User" => await AuthClients.CreateUserClientAsync(),
            "Admin" => await AuthClients.CreateAdminClientAsync(),
            _ => throw new ArgumentOutOfRangeException(nameof(role)),
        };

        using var request = new HttpRequestMessage(new HttpMethod(method), path);
        using var response = await client.SendAsync(request);
        Assert.Equal(expected, response.StatusCode);
    }

    [PostgreSqlFact]
    public async Task Admin_can_create_feature_flag()
    {
        using var admin = await AuthClients.CreateAdminClientAsync();
        var key = TestIds.Truncate($"ff.authz.{Guid.NewGuid():N}", 40);

        using var response = await admin.PostAsJsonAsync("/api/admin/features", new
        {
            key,
            isEnabled = true,
            description = "authz matrix",
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [PostgreSqlFact]
    public async Task User_cannot_create_feature_flag()
    {
        using var user = await AuthClients.CreateUserClientAsync();
        using var response = await user.PostAsJsonAsync("/api/admin/features", new
        {
            key = TestIds.Truncate($"ff.denied.{Guid.NewGuid():N}", 40),
            isEnabled = true,
            description = (string?)null,
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}

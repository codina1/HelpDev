using HelpDev.API.OpenApi;

namespace HelpDev.API.Tests.OpenApi;

public sealed class OpenApiPathHelpersTests
{
    [Theory]
    [InlineData("api/v1/content", true)]
    [InlineData("api/v2/search", true)]
    [InlineData("API/V1/content", true)]
    [InlineData("api/content", false)]
    [InlineData("health/live", false)]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    public void IsCanonicalVersionedApiPath_identifies_versioned_routes(string? relativePath, bool expected)
    {
        Assert.Equal(expected, OpenApiPathHelpers.IsCanonicalVersionedApiPath(relativePath));
    }
}

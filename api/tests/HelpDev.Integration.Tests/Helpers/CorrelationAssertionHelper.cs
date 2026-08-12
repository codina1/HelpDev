using HelpDev.API.Security;

namespace HelpDev.Integration.Tests.Helpers;

public static class CorrelationAssertionHelper
{
    public const string HeaderName = CorrelationIdMiddleware.HeaderName;

    public static void SetCorrelationId(HttpRequestMessage request, string correlationId) =>
        request.Headers.TryAddWithoutValidation(HeaderName, correlationId);

    public static string? GetCorrelationId(HttpResponseMessage response) =>
        response.Headers.TryGetValues(HeaderName, out var values)
            ? values.FirstOrDefault()
            : null;

    public static void AssertEchoed(HttpResponseMessage response, string expectedCorrelationId)
    {
        var actual = GetCorrelationId(response);
        Assert.Equal(expectedCorrelationId, actual);
    }

    public static void AssertPresent(HttpResponseMessage response)
    {
        var actual = GetCorrelationId(response);
        Assert.False(string.IsNullOrWhiteSpace(actual));
    }
}

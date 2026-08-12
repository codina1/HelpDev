namespace HelpDev.Integration.Tests.Helpers;

public static class SensitiveLogAssertionHelper
{
    public static void AssertSentinelsAbsent(
        IEnumerable<CapturedLogEntry> logs,
        params string[] sentinels)
    {
        foreach (var log in logs)
        {
            AssertSentinelAbsent(log.Message, sentinels, "message");
            AssertSentinelAbsent(log.StateAsString, sentinels, "state");
            AssertSentinelAbsent(log.Exception?.ToString(), sentinels, "exception");

            foreach (var scope in log.Scopes)
            {
                AssertSentinelAbsent($"{scope.Key}={scope.Value}", sentinels, "scope");
            }
        }
    }

    private static void AssertSentinelAbsent(string? haystack, string[] sentinels, string source)
    {
        if (string.IsNullOrEmpty(haystack))
        {
            return;
        }

        foreach (var sentinel in sentinels)
        {
            Assert.DoesNotContain(
                sentinel,
                haystack,
                StringComparison.Ordinal);
        }
    }
}

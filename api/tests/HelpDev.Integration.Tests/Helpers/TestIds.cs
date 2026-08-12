namespace HelpDev.Integration.Tests.Helpers;

public static class TestIds
{
    public static string FeatureKey(string prefix) =>
        Truncate($"{prefix}.{Guid.NewGuid():N}", 40);

    public static string Slug(string prefix, int maxLength = 20) =>
        Truncate($"{prefix}-{Guid.NewGuid():N}", maxLength);

    public static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}

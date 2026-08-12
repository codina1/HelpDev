namespace HelpDev.API.Tests.OpenApi;

internal static class OpenApiArtifactLocator
{
    public static string? TryLocateArtifactsDirectory()
    {
        var start = new DirectoryInfo(AppContext.BaseDirectory);
        for (var current = start; current is not null; current = current.Parent)
        {
            var candidate = Path.Combine(current.FullName, "artifacts", "openapi");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    public static string RequireArtifactsDirectory()
    {
        var directory = TryLocateArtifactsDirectory();
        Assert.True(
            directory is not null,
            "OpenAPI artifacts not found. Expected api/artifacts/openapi under the repository root. Run the OpenAPI export to generate helpdev-*-v1.json files.");

        return directory!;
    }

    public static IReadOnlyList<string> GetVersionedArtifactFiles(string directory) =>
        Directory.GetFiles(directory, "helpdev-*-v1.json", SearchOption.TopDirectoryOnly)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();
}

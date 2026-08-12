namespace HelpDev.API.Tests.Deployment;

/// <summary>
/// Sprint 46 — release artifact presence checks (documentation + manifest shape expectations).
/// </summary>
[Trait("Category", "ProductionCertification")]
[Trait("Category", "Release")]
public sealed class ReleaseArtifactCertificationTests
{
    [Fact]
    public void Release_manifest_reports_expected_migration_count()
    {
        var manifestPath = ResolveRepoPath("api", "artifacts", "release", "release-manifest.json");
        Assert.True(File.Exists(manifestPath), $"Missing release manifest at {manifestPath}");

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(manifestPath));
        var root = document.RootElement;

        Assert.Equal("HelpDev.API", root.GetProperty("application").GetString());
        Assert.Equal("1.0.0", root.GetProperty("version").GetString());
        Assert.Equal(23, root.GetProperty("migrationCount").GetInt32());
        Assert.True(root.TryGetProperty("testCount", out var testCount));
        Assert.True(testCount.ValueKind == System.Text.Json.JsonValueKind.Number);
        Assert.True(testCount.GetInt32() >= 0);
        Assert.False(string.IsNullOrWhiteSpace(root.GetProperty("buildTimestampUtc").GetString()));
        Assert.Equal(64, root.GetProperty("binarySha256").GetString()?.Length);

        var raw = File.ReadAllText(manifestPath);
        Assert.DoesNotContain("Password=", raw, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ConnectionString", raw, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\"jwtSecret\"", raw, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("docs", "deployment", "backup-restore-validation-v1.md")]
    [InlineData("docs", "operations", "production-runbook-v1.md")]
    [InlineData("docs", "release", "helpdev-v1-release-candidate.md")]
    public void Certification_documents_exist(params string[] relativeSegments)
    {
        var path = ResolveRepoPath(relativeSegments);
        Assert.True(File.Exists(path), $"Missing certification document: {path}");
        var text = File.ReadAllText(path);
        Assert.Contains("Sprint", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Password=", text, StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveRepoPath(params string[] segments)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(new[] { dir.FullName }.Concat(segments).ToArray());
            if (File.Exists(candidate) || Directory.Exists(Path.Combine(dir.FullName, segments[0])))
            {
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            // Prefer repo root that contains both api/ and docs/.
            var api = Path.Combine(dir.FullName, "api");
            var docs = Path.Combine(dir.FullName, "docs");
            if (Directory.Exists(api) && Directory.Exists(docs))
            {
                return Path.Combine(new[] { dir.FullName }.Concat(segments).ToArray());
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException(
            $"Could not resolve repository root from {AppContext.BaseDirectory} for {string.Join('/', segments)}");
    }
}

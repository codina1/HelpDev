using System.Text.Json;
using System.Text.RegularExpressions;

namespace HelpDev.API.Tests.OpenApi;

[Trait("Category", "OpenApi")]
public sealed class OpenApiArtifactSecurityScanTests
{
    private static readonly string[] ForbiddenSubstrings =
    [
        "Host=",
        "Password=",
        "Username=",
        "Npgsql",
        "stack trace",
        "System.Exception",
        "\"Authorization: Bearer\"",
        "eyJ",
        "OTP_SENTINEL",
        "PHONE_SENTINEL",
        "PROMPT_TEMPLATE_PRIVATE_SENTINEL",
        "TOOL_INPUT_PRIVATE_SENTINEL",
        "203.0.113.",
        "/home/",
        @"C:\",
        "bin/Debug",
        "obj/",
    ];

    private static readonly Regex SqlSelectPattern = new(@"\sSELECT\s", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex SqlInsertPattern = new(@"\sINSERT\s", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex SqlUpdatePattern = new(@"\sUPDATE\s", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    [Fact]
    public void Exported_openapi_artifacts_do_not_leak_sensitive_content()
    {
        var directory = OpenApiArtifactLocator.RequireArtifactsDirectory();
        var files = OpenApiArtifactLocator.GetVersionedArtifactFiles(directory);

        Assert.NotEmpty(files);

        foreach (var file in files)
        {
            var content = File.ReadAllText(file);

            foreach (var forbidden in ForbiddenSubstrings)
            {
                Assert.DoesNotContain(
                    forbidden,
                    content,
                    StringComparison.OrdinalIgnoreCase);
            }

            Assert.DoesNotMatch(SqlSelectPattern, content);
            Assert.DoesNotMatch(SqlInsertPattern, content);
            Assert.DoesNotMatch(SqlUpdatePattern, content);
        }
    }

    [Fact]
    public void Exported_openapi_artifacts_are_valid_json_documents()
    {
        var directory = OpenApiArtifactLocator.RequireArtifactsDirectory();
        var files = OpenApiArtifactLocator.GetVersionedArtifactFiles(directory);

        foreach (var file in files)
        {
            using var document = JsonDocument.Parse(File.ReadAllText(file));
            Assert.True(document.RootElement.TryGetProperty("openapi", out _));
            Assert.True(document.RootElement.TryGetProperty("paths", out _));
        }
    }
}

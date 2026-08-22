using System.Text.Json;
using HelpDev.API.Deployment;

namespace HelpDev.API.Tests.Deployment;

[Trait("Category", "ReleaseCandidate")]
[Trait("Category", "Deployment")]
public sealed class DeploymentCommandsTests
{
    [Fact]
    public void Parse_returns_null_when_no_command_flags_present()
    {
        Assert.Null(DeploymentCommandParser.Parse([]));
        Assert.Null(DeploymentCommandParser.Parse(["--export-openapi", "artifacts/openapi"]));
    }

    [Fact]
    public void Parse_recognizes_validate_production_config()
    {
        var command = DeploymentCommandParser.Parse(["--validate-production-config"]);

        Assert.NotNull(command);
        Assert.Equal(DeploymentCommandKind.Validate, command!.Kind);
        Assert.False(command.ValidateDatabase);
    }

    [Fact]
    public void Parse_recognizes_validate_database_flag()
    {
        var command = DeploymentCommandParser.Parse(["--validate-production-config", "--validate-database"]);

        Assert.NotNull(command);
        Assert.Equal(DeploymentCommandKind.Validate, command!.Kind);
        Assert.True(command.ValidateDatabase);
    }

    [Fact]
    public void Parse_recognizes_apply_migrations_with_seed()
    {
        var command = DeploymentCommandParser.Parse(["--apply-migrations", "--seed-required-system-data"]);

        Assert.NotNull(command);
        Assert.Equal(DeploymentCommandKind.ApplyMigrations, command!.Kind);
        Assert.True(command.SeedRequiredSystemData);
    }

    [Fact]
    public void Parse_recognizes_emit_release_manifest_with_output_and_test_count()
    {
        var command = DeploymentCommandParser.Parse(
            ["--emit-release-manifest", "-o", "artifacts/release/release-manifest.json", "--test-count", "1234"]);

        Assert.NotNull(command);
        Assert.Equal(DeploymentCommandKind.EmitReleaseManifest, command!.Kind);
        Assert.Equal("artifacts/release/release-manifest.json", command.OutputPath);
        Assert.Equal(1234, command.TestCount);
    }

    [Fact]
    public void HasDevelopmentDemoSeedRequest_detects_forbidden_flag()
    {
        Assert.True(DeploymentCommandParser.HasDevelopmentDemoSeedRequest(["--apply-migrations", "--seed-development-demo"]));
        Assert.False(DeploymentCommandParser.HasDevelopmentDemoSeedRequest(["--apply-migrations"]));
    }

    [Fact]
    public async Task EmitReleaseManifest_writes_deterministic_manifest_without_secrets()
    {
        var outputPath = Path.Combine(Path.GetTempPath(), $"release-manifest-{Guid.NewGuid():N}.json");
        var previousVersion = Environment.GetEnvironmentVariable("RELEASE_VERSION");
        var previousCommit = Environment.GetEnvironmentVariable("RELEASE_COMMIT");
        var previousTimestamp = Environment.GetEnvironmentVariable("RELEASE_BUILD_TIMESTAMP");

        try
        {
            Environment.SetEnvironmentVariable("RELEASE_VERSION", "9.9.9");
            Environment.SetEnvironmentVariable("RELEASE_COMMIT", "deadbeef");
            Environment.SetEnvironmentVariable("RELEASE_BUILD_TIMESTAMP", "2026-07-21T00:00:00Z");

            var command = new DeploymentCommand(
                DeploymentCommandKind.EmitReleaseManifest,
                OutputPath: outputPath,
                TestCount: 4321);

            var exitCode = await DeploymentCommands.EmitReleaseManifestAsync(command);

            Assert.Equal(DeploymentCommands.ExitSuccess, exitCode);
            Assert.True(File.Exists(outputPath));

            using var document = JsonDocument.Parse(await File.ReadAllTextAsync(outputPath));
            var root = document.RootElement;

            Assert.Equal("HelpDev.API", root.GetProperty("application").GetString());
            Assert.Equal("9.9.9", root.GetProperty("version").GetString());
            Assert.Equal("deadbeef", root.GetProperty("commit").GetString());
            Assert.Equal("2026-07-21T00:00:00Z", root.GetProperty("buildTimestampUtc").GetString());
            Assert.Equal("net8.0", root.GetProperty("targetFramework").GetString());
            Assert.Equal("Release", root.GetProperty("configuration").GetString());
            Assert.Equal("v1", root.GetProperty("openApiVersion").GetString());
            Assert.Equal(27, root.GetProperty("migrationCount").GetInt32());
            Assert.Equal(4321, root.GetProperty("testCount").GetInt32());

            var rawText = await File.ReadAllTextAsync(outputPath);
            Assert.DoesNotContain("Password", rawText, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("ConnectionString", rawText, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("Secret", rawText, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Environment.SetEnvironmentVariable("RELEASE_VERSION", previousVersion);
            Environment.SetEnvironmentVariable("RELEASE_COMMIT", previousCommit);
            Environment.SetEnvironmentVariable("RELEASE_BUILD_TIMESTAMP", previousTimestamp);
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
        }
    }
}

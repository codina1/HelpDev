using HelpDev.API.Deployment;
using HelpDev.Infrastructure.Persistence;
using NetArchTest.Rules;

namespace HelpDev.Architecture.Tests;

[Trait("Category", "Deployment")]
public sealed class DeploymentArchitectureTests
{
    private static readonly string[] DomainAndApplicationNamespaces =
    [
        "HelpDev.Modules.Identity.Domain",
        "HelpDev.Modules.Content.Domain",
        "HelpDev.Modules.Learning.Domain",
        "HelpDev.Modules.Search.Domain",
        "HelpDev.Modules.Administration.Domain",
        "HelpDev.Modules.Toolbox.Domain",
        "HelpDev.Modules.PromptLab.Domain",
        "HelpDev.Modules.Analytics.Domain",
        "HelpDev.Modules.Auditing.Domain",
    ];

    [Fact]
    public void ProductionSafetyValidator_lives_in_the_api_deployment_namespace()
    {
        Assert.Equal("HelpDev.API.Deployment", typeof(ProductionSafetyValidator).Namespace);
        Assert.Equal("HelpDev.API.Deployment", typeof(IProductionSafetyValidator).Namespace);
    }

    [Fact]
    public void Database_startup_manager_lives_in_infrastructure_persistence()
    {
        Assert.Equal("HelpDev.Infrastructure.Persistence", typeof(DatabaseStartupManager).Namespace);
    }

    [Fact]
    public void Controllers_do_not_reference_the_database_startup_manager()
    {
        var result = Types.InAssembly(typeof(HelpDev.API.Controllers.ContentController).Assembly)
            .That()
            .ResideInNamespace("HelpDev.API.Controllers")
            .ShouldNot()
            .HaveDependencyOn("HelpDev.Infrastructure.Persistence.DatabaseStartupManager")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Domain_layers_do_not_depend_on_deployment_types()
    {
        var deploymentAssembly = typeof(ProductionSafetyValidator).Assembly;
        var result = Types.InAssembly(typeof(HelpDev.Modules.Content.Domain.Entities.Content).Assembly)
            .That()
            .ResideInNamespaceMatching("HelpDev.Modules..*.Domain")
            .ShouldNot()
            .HaveDependencyOn("HelpDev.API.Deployment")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
        Assert.NotNull(deploymentAssembly);
    }

    [Fact]
    public void MigrationAdvisoryLockKey_is_a_stable_documented_constant()
    {
        Assert.Equal(4207770001L, DatabaseStartupManager.MigrationAdvisoryLockKey);
    }

    [Fact]
    public void Production_settings_file_contains_no_committed_secrets()
    {
        var path = FindFile("appsettings.Production.json", "HelpDev.API");
        var lines = File.ReadAllLines(path)
            .Where(line => !line.TrimStart().StartsWith("\"$comment\"", StringComparison.Ordinal));
        var text = string.Join(Environment.NewLine, lines);

        // Non-secret policy defaults only: no connection string, JWT secret, or partition key values.
        Assert.DoesNotContain("Host=", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Password=", text, StringComparison.Ordinal);
        Assert.DoesNotContain("\"Secret\"", text, StringComparison.Ordinal);
        Assert.DoesNotContain("PartitionHashKey", text, StringComparison.Ordinal);
        Assert.DoesNotContain("DefaultConnection", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Production_migration_mode_defaults_to_validate_in_production()
    {
        var options = new DatabaseStartupOptions();

        Assert.Equal(DatabaseMigrationMode.Validate, options.ResolveMigrationMode(isProduction: true));
        Assert.Equal(DatabaseMigrationMode.Apply, options.ResolveMigrationMode(isProduction: false));
    }

    [Fact]
    public void Production_seed_mode_defaults_to_none_outside_development()
    {
        var options = new DatabaseStartupOptions();

        Assert.Equal(DatabaseSeedMode.None, options.ResolveSeedMode(isDevelopment: false));
        Assert.Equal(DatabaseSeedMode.DevelopmentDemo, options.ResolveSeedMode(isDevelopment: true));
    }

    private static string FindFile(string fileName, string directoryHint)
    {
        var start = new DirectoryInfo(AppContext.BaseDirectory);
        for (var current = start; current is not null; current = current.Parent)
        {
            IEnumerable<FileInfo> matches;
            try
            {
                matches = current.GetFiles(fileName, SearchOption.AllDirectories);
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }

            var filtered = matches
                .Where(file => file.DirectoryName?.Contains(directoryHint, StringComparison.OrdinalIgnoreCase) == true)
                .Where(file => !file.FullName.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                    && !file.FullName.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
                .ToList();

            if (filtered.Count > 0)
            {
                return filtered[0].FullName;
            }
        }

        throw new FileNotFoundException($"Could not locate {fileName}.");
    }

    private static string FormatFailures(TestResult result) =>
        result.FailingTypeNames is null || !result.FailingTypeNames.Any()
            ? "Architecture rule failed."
            : string.Join(Environment.NewLine, result.FailingTypeNames);
}

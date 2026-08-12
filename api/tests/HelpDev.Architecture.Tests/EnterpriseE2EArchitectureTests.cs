using HelpDev.Modules.Auditing;
using HelpDev.SharedContracts.Auditing;
using InfrastructureDi = HelpDev.Infrastructure.DependencyInjection;

namespace HelpDev.Architecture.Tests;

public sealed class EnterpriseE2EArchitectureTests
{
    [Fact]
    public void Production_assemblies_do_not_reference_integration_tests()
    {
        var productionAssemblies = new[]
        {
            typeof(Program).Assembly,
            typeof(InfrastructureDi).Assembly,
            typeof(ModuleMarker).Assembly,
            typeof(IAuditRecorder).Assembly,
        };

        foreach (var assembly in productionAssemblies)
        {
            var referenced = assembly.GetReferencedAssemblies()
                .Select(name => name.Name)
                .Where(name => name is not null)
                .ToList();

            Assert.DoesNotContain(
                referenced,
                name => name!.Contains("HelpDev.Integration.Tests", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void Integration_tests_csproj_does_not_reference_ef_inmemory_or_sqlite()
    {
        var csprojPath = FindFile("HelpDev.Integration.Tests.csproj");
        var text = File.ReadAllText(csprojPath);

        Assert.DoesNotContain("Microsoft.EntityFrameworkCore.InMemory", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Microsoft.EntityFrameworkCore.Sqlite", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("UseInMemoryDatabase", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("UseSqlite", text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Auditing_di_registers_noop_audit_persistence_failure_injector()
    {
        var path = FindFile("DependencyInjection.cs", "HelpDev.Modules.Auditing");
        var text = File.ReadAllText(path);

        Assert.Contains("IAuditPersistenceFailureInjector", text, StringComparison.Ordinal);
        Assert.Contains("NoOpAuditPersistenceFailureInjector", text, StringComparison.Ordinal);
        Assert.DoesNotContain("TestAuditPersistenceFailureInjector", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Cors_policy_source_has_no_wildcard_with_origins()
    {
        var path = FindFile("SecurityServiceCollectionExtensions.cs");
        var text = File.ReadAllText(path);

        Assert.DoesNotContain("""WithOrigins("*")""", text, StringComparison.Ordinal);
        Assert.DoesNotContain("WithOrigins(\"*\")", text, StringComparison.Ordinal);
    }

    [Fact]
    public void RequestLoggingMiddleware_source_does_not_read_request_body()
    {
        var path = FindFile("RequestLoggingMiddleware.cs");
        var text = File.ReadAllText(path);

        Assert.DoesNotContain("Request.Body", text, StringComparison.Ordinal);
        Assert.DoesNotContain("EnableBuffering", text, StringComparison.Ordinal);
    }

    private static string FindFile(string fileName, string? directoryHint = null)
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
                .Where(file => directoryHint is null
                    || file.DirectoryName?.Contains(directoryHint, StringComparison.OrdinalIgnoreCase) == true)
                .Where(file => !file.FullName.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                    && !file.FullName.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
                .ToList();

            if (filtered.Count > 0)
            {
                return filtered[0].FullName;
            }
        }

        throw new FileNotFoundException($"Could not locate {fileName} (hint={directoryHint}).");
    }
}

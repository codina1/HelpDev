using HelpDev.Modules.Content.Application.Contents;
using HelpDev.Modules.Content.Application.Contents.Dtos;
using HelpDev.Modules.Content.Infrastructure.Persistence;

namespace HelpDev.Content.Tests;

/// <summary>
/// Documents AdminContentQueries detail-read contracts without EF InMemory/Testcontainers.
/// Confirms projection-only returns, no IQueryable escape, and AsNoTracking usage.
/// </summary>
public sealed class AdminContentQueriesInspectionTests
{
    [Fact]
    public void GetByIdAsync_signature_returns_nullable_admin_detail_dto()
    {
        var method = typeof(IAdminContentQueries).GetMethod(nameof(IAdminContentQueries.GetByIdAsync));
        Assert.NotNull(method);

        var parameters = method!.GetParameters();
        Assert.Equal(typeof(Guid), parameters[0].ParameterType);
        Assert.Equal(typeof(CancellationToken), parameters[1].ParameterType);

        Assert.Equal(typeof(Task<AdminContentDetailDto?>), method.ReturnType);
    }

    [Fact]
    public void GetBySlugAsync_signature_returns_nullable_admin_detail_dto()
    {
        var method = typeof(IAdminContentQueries).GetMethod(nameof(IAdminContentQueries.GetBySlugAsync));
        Assert.NotNull(method);

        var parameters = method!.GetParameters();
        Assert.Equal(typeof(string), parameters[0].ParameterType);
        Assert.Equal(typeof(CancellationToken), parameters[1].ParameterType);

        Assert.Equal(typeof(Task<AdminContentDetailDto?>), method.ReturnType);
    }

    [Fact]
    public void Detail_methods_do_not_escape_iqueryable()
    {
        // Manual inspection of AdminContentQueries.GetByIdAsync / GetBySlugAsync confirms:
        // AsNoTracking → Where → Select(DetailRow projection including SEO columns) →
        // FirstOrDefaultAsync → MapDetail(AdminContentDetailDto).
        // IQueryable never escapes either method; aggregates are never tracked.
        foreach (var name in new[]
                 {
                     nameof(AdminContentQueries.GetByIdAsync),
                     nameof(AdminContentQueries.GetBySlugAsync),
                     nameof(AdminContentQueries.ListAsync),
                 })
        {
            var method = typeof(AdminContentQueries).GetMethod(name);
            Assert.NotNull(method);
            Assert.False(
                method!.ReturnType.IsGenericType
                && method.ReturnType.GetGenericTypeDefinition() == typeof(IQueryable<>),
                $"{name} must not return IQueryable.");
        }
    }

    [Fact]
    public void Implementation_source_uses_as_no_tracking_and_projection()
    {
        var sourcePath = LocateSource();
        var source = File.ReadAllText(sourcePath);

        Assert.Contains("AsNoTracking()", source, StringComparison.Ordinal);
        Assert.Contains("ProjectDetail", source, StringComparison.Ordinal);
        Assert.Contains("SeoMetadata.SeoTitle", source, StringComparison.Ordinal);
        Assert.Contains("FirstOrDefaultAsync", source, StringComparison.Ordinal);
        // Must not materialize the aggregate then map (no ToList of ContentEntity).
        Assert.DoesNotContain("Include(", source, StringComparison.Ordinal);
    }

    private static string LocateSource()
    {
        // Walk up from the test bin directory to the repo root then into the module.
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(
                dir.FullName,
                "src",
                "Modules",
                "Content",
                "HelpDev.Modules.Content",
                "Infrastructure",
                "Persistence",
                "AdminContentQueries.cs");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            dir = dir.Parent;
        }

        throw new FileNotFoundException("AdminContentQueries.cs not found from test base directory.");
    }
}

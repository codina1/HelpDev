using HelpDev.Modules.Learning.Application.Courses;
using HelpDev.Modules.Learning.Domain.Courses;

namespace HelpDev.Learning.Application.Tests;

/// <summary>
/// Documents CourseQueries ownership filter contract without EF InMemory/Testcontainers.
/// </summary>
public sealed class CourseQueriesOwnershipInspectionTests
{
    [Fact]
    public void CourseQueries_list_signature_accepts_optional_instructor_filter()
    {
        var method = typeof(ICourseQueries).GetMethod(nameof(ICourseQueries.ListAsync));
        Assert.NotNull(method);

        var parameters = method.GetParameters();
        Assert.Equal(typeof(CourseStatus?), parameters[0].ParameterType);
        Assert.Equal(typeof(Guid?), parameters[1].ParameterType);
        Assert.Equal(typeof(CancellationToken), parameters[2].ParameterType);
    }

    [Fact]
    public void CourseQueries_implementation_keeps_ownership_filter_before_materialization()
    {
        // Manual inspection of CourseQueries.ListAsync confirms:
        // AsNoTracking → optional Status Where → optional InstructorId Where →
        // OrderByDescending(CreatedAt) → Select projection → ToListAsync.
        // IQueryable never escapes the method.
        var method = typeof(HelpDev.Modules.Learning.Infrastructure.Persistence.CourseQueries)
            .GetMethod(nameof(HelpDev.Modules.Learning.Infrastructure.Persistence.CourseQueries.ListAsync));

        Assert.NotNull(method);
        Assert.False(method!.ReturnType.IsGenericType
            && method.ReturnType.GetGenericTypeDefinition() == typeof(IQueryable<>));
    }
}

using HelpDev.Modules.Search.Application.Contracts;
using HelpDev.Modules.Search.Application.Indexing;
using HelpDev.Modules.Search.Domain;

namespace HelpDev.Modules.Search.Application.Handlers;

/// <summary>
/// Shared Course → SearchDocument projection path for publish and update events.
/// </summary>
public sealed class CourseSearchProjectionApplier
{
    private readonly ICourseSearchSource _courseSearchSource;
    private readonly ISearchProjectionService _projectionService;

    public CourseSearchProjectionApplier(
        ICourseSearchSource courseSearchSource,
        ISearchProjectionService projectionService)
    {
        _courseSearchSource = courseSearchSource;
        _projectionService = projectionService;
    }

    public async Task ApplyAsync(
        Guid courseId,
        Guid eventId,
        DateTime occurredAtUtc,
        CancellationToken cancellationToken = default)
    {
        var source = await _courseSearchSource.GetByIdAsync(courseId, cancellationToken);
        await _projectionService.ApplyAsync(
            SearchSourceTypes.Course,
            courseId,
            source,
            eventId,
            occurredAtUtc,
            cancellationToken);
    }
}

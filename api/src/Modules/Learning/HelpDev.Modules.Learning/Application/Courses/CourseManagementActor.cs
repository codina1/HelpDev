namespace HelpDev.Modules.Learning.Application.Courses;

/// <summary>
/// Framework-neutral management actor constructed by the API from authenticated claims.
/// </summary>
public sealed record CourseManagementActor
{
    public CourseManagementActor(Guid userId, bool canManageAllCourses)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id must not be empty.", nameof(userId));
        }

        UserId = userId;
        CanManageAllCourses = canManageAllCourses;
    }

    public Guid UserId { get; init; }

    public bool CanManageAllCourses { get; init; }
}

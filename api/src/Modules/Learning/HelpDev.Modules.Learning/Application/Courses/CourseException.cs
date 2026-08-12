namespace HelpDev.Modules.Learning.Application.Courses;

public sealed class CourseException : Exception
{
    public CourseException(string message, string code)
        : base(message)
    {
        Code = code;
    }

    public CourseException(string message, string code, Exception innerException)
        : base(message, innerException)
    {
        Code = code;
    }

    public string Code { get; }
}

public static class CourseErrorCodes
{
    public const string NotFound = "course_not_found";
    public const string SlugInvalid = "course_slug_invalid";
    public const string SlugDuplicate = "course_slug_duplicate";
    public const string InstructorIdInvalid = "instructor_id_invalid";
    public const string OperationInvalid = "course_operation_invalid";
}

namespace HelpDev.Modules.Learning.Application.Enrollments;

public sealed class EnrollmentException : Exception
{
    public EnrollmentException(string message, string code)
        : base(message)
    {
        Code = code;
    }

    public EnrollmentException(string message, string code, Exception innerException)
        : base(message, innerException)
    {
        Code = code;
    }

    public string Code { get; }
}

public static class EnrollmentErrorCodes
{
    public const string NotFound = "enrollment_not_found";
    public const string AlreadyExists = "enrollment_already_exists";
    public const string UserInvalid = "enrollment_user_invalid";
    public const string CourseInvalid = "enrollment_course_invalid";
    public const string CourseNotFound = "enrollment_course_not_found";
    public const string CourseNotPublished = "enrollment_course_not_published";
    public const string CourseHasNoLessons = "enrollment_course_has_no_lessons";
    public const string LessonInvalid = "enrollment_lesson_invalid";
    public const string LessonNotInCourse = "enrollment_lesson_not_in_course";
    public const string OperationInvalid = "enrollment_operation_invalid";
}

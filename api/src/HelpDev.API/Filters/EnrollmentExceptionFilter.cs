using HelpDev.Modules.Learning.Application.Enrollments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HelpDev.API.Filters;

/// <summary>
/// Maps Learning EnrollmentException to the API JSON error envelope without per-action try/catch.
/// </summary>
public sealed class EnrollmentExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not EnrollmentException exception)
        {
            return;
        }

        var statusCode = exception.Code switch
        {
            EnrollmentErrorCodes.NotFound => StatusCodes.Status404NotFound,
            EnrollmentErrorCodes.CourseNotFound => StatusCodes.Status404NotFound,
            EnrollmentErrorCodes.LessonNotInCourse => StatusCodes.Status404NotFound,
            EnrollmentErrorCodes.AlreadyExists => StatusCodes.Status409Conflict,
            EnrollmentErrorCodes.CourseNotPublished => StatusCodes.Status409Conflict,
            EnrollmentErrorCodes.CourseHasNoLessons => StatusCodes.Status409Conflict,
            EnrollmentErrorCodes.OperationInvalid => StatusCodes.Status409Conflict,
            EnrollmentErrorCodes.UserInvalid => StatusCodes.Status400BadRequest,
            EnrollmentErrorCodes.CourseInvalid => StatusCodes.Status400BadRequest,
            EnrollmentErrorCodes.LessonInvalid => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status400BadRequest,
        };

        context.Result = new ObjectResult(new { message = exception.Message, code = exception.Code })
        {
            StatusCode = statusCode,
        };
        context.ExceptionHandled = true;
    }
}

using HelpDev.Modules.Learning.Application.Courses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HelpDev.API.Filters;

/// <summary>
/// Maps Learning CourseException to the API JSON error envelope without per-action try/catch.
/// </summary>
public sealed class CourseExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not CourseException exception)
        {
            return;
        }

        var statusCode = exception.Code switch
        {
            CourseErrorCodes.NotFound => StatusCodes.Status404NotFound,
            CourseErrorCodes.SlugDuplicate => StatusCodes.Status409Conflict,
            CourseErrorCodes.OperationInvalid => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest,
        };

        context.Result = new ObjectResult(new { message = exception.Message, code = exception.Code })
        {
            StatusCode = statusCode,
        };
        context.ExceptionHandled = true;
    }
}

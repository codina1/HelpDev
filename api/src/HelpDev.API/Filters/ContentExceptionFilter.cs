using HelpDev.Modules.Content.Application.ContentAi;
using HelpDev.Modules.Content.Application.Contents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HelpDev.API.Filters;

/// <summary>
/// Maps Content <see cref="ContentException"/> / <see cref="ContentAiException"/> to the API JSON error envelope.
/// </summary>
public sealed class ContentExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is ContentAiException aiException)
        {
            var aiStatus = aiException.Code switch
            {
                ContentAiErrorCodes.NotFound => StatusCodes.Status404NotFound,
                ContentAiErrorCodes.Disabled => StatusCodes.Status403Forbidden,
                ContentAiErrorCodes.TaskNotAllowed => StatusCodes.Status403Forbidden,
                ContentAiErrorCodes.ProviderFailed => StatusCodes.Status502BadGateway,
                _ => StatusCodes.Status400BadRequest,
            };

            context.Result = new ObjectResult(new { message = aiException.Message, code = aiException.Code })
            {
                StatusCode = aiStatus,
            };
            context.ExceptionHandled = true;
            return;
        }

        if (context.Exception is not ContentException exception)
        {
            return;
        }

        var statusCode = exception.Code switch
        {
            ContentErrorCodes.NotFound => StatusCodes.Status404NotFound,
            ContentErrorCodes.SlugDuplicate => StatusCodes.Status409Conflict,
            ContentErrorCodes.OperationInvalid => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest,
        };

        context.Result = new ObjectResult(new { message = exception.Message, code = exception.Code })
        {
            StatusCode = statusCode,
        };
        context.ExceptionHandled = true;
    }
}

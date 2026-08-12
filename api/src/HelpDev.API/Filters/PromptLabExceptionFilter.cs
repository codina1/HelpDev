using HelpDev.Modules.PromptLab.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HelpDev.API.Filters;

public sealed class PromptLabExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not PromptLabException exception)
        {
            return;
        }

        var statusCode = exception.Code switch
        {
            PromptLabApplicationErrorCodes.CategoryNotFound
                or PromptLabApplicationErrorCodes.PromptNotFound
                or PromptLabApplicationErrorCodes.PromptDisabled
                or PromptLabApplicationErrorCodes.PromptUnpublished
                or PromptLabApplicationErrorCodes.PromptVersionNotFound
                or PromptLabApplicationErrorCodes.HistoryNotFound => StatusCodes.Status404NotFound,

            PromptLabApplicationErrorCodes.RenderRequiresAuthentication
                or PromptLabApplicationErrorCodes.FavoriteRequiresAuthentication
                or PromptLabApplicationErrorCodes.HistoryAccessDenied => StatusCodes.Status401Unauthorized,

            PromptLabApplicationErrorCodes.CategorySlugDuplicate
                or PromptLabApplicationErrorCodes.PromptSlugDuplicate
                or PromptLabApplicationErrorCodes.CategoryInactive
                or PromptLabApplicationErrorCodes.PromptCannotPublish
                or PromptLabApplicationErrorCodes.PromptCategoryInvalid => StatusCodes.Status409Conflict,

            _ => StatusCodes.Status400BadRequest,
        };

        context.Result = new ObjectResult(new { message = exception.Message, code = exception.Code })
        {
            StatusCode = statusCode,
        };
        context.ExceptionHandled = true;
    }
}

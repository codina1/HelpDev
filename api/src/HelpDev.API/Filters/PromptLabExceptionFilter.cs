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
                or PromptLabApplicationErrorCodes.HistoryNotFound
                or PromptLabApplicationErrorCodes.AiModelNotFound => StatusCodes.Status404NotFound,

            PromptLabApplicationErrorCodes.PromptEditForbidden => StatusCodes.Status403Forbidden,

            PromptLabApplicationErrorCodes.RenderRequiresAuthentication
                or PromptLabApplicationErrorCodes.FavoriteRequiresAuthentication
                or PromptLabApplicationErrorCodes.HistoryAccessDenied => StatusCodes.Status401Unauthorized,

            PromptLabApplicationErrorCodes.CategorySlugDuplicate
                or PromptLabApplicationErrorCodes.PromptSlugDuplicate
                or PromptLabApplicationErrorCodes.CategoryInactive
                or PromptLabApplicationErrorCodes.AiModelInactive
                or PromptLabApplicationErrorCodes.PromptCannotPublish
                or PromptLabApplicationErrorCodes.PromptCategoryInvalid
                or PromptLabApplicationErrorCodes.PromptNotDraft
                or PromptLabApplicationErrorCodes.PromptStatusInvalid => StatusCodes.Status409Conflict,

            PromptLabApplicationErrorCodes.PromptRejectionReasonRequired
                or PromptLabApplicationErrorCodes.PromptRejectionReasonInvalid => StatusCodes.Status400BadRequest,

            _ => StatusCodes.Status400BadRequest,
        };

        context.Result = new ObjectResult(new { message = exception.Message, code = exception.Code })
        {
            StatusCode = statusCode,
        };
        context.ExceptionHandled = true;
    }
}

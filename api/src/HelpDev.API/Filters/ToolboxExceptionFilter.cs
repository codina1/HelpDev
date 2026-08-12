using HelpDev.Modules.Toolbox.Application.Execution;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HelpDev.API.Filters;

public sealed class ToolboxExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not ToolboxException exception)
        {
            return;
        }

        var statusCode = exception.Code switch
        {
            ToolboxApplicationErrorCodes.CategoryNotFound
                or ToolboxApplicationErrorCodes.ToolNotFound
                or ToolboxApplicationErrorCodes.ToolDisabled
                or ToolboxApplicationErrorCodes.ToolUnpublished
                or ToolboxApplicationErrorCodes.FavoriteNotFound
                or ToolboxApplicationErrorCodes.HistoryNotFound => StatusCodes.Status404NotFound,

            ToolboxApplicationErrorCodes.ToolRequiresAuthentication
                or ToolboxApplicationErrorCodes.FavoriteRequiresAuthentication
                or ToolboxApplicationErrorCodes.HistoryAccessDenied => StatusCodes.Status401Unauthorized,

            ToolboxApplicationErrorCodes.CategorySlugDuplicate
                or ToolboxApplicationErrorCodes.ToolSlugDuplicate
                or ToolboxApplicationErrorCodes.CategoryInactive
                or ToolboxApplicationErrorCodes.ToolCannotPublish
                or ToolboxApplicationErrorCodes.ToolCategoryInvalid => StatusCodes.Status409Conflict,

            _ => StatusCodes.Status400BadRequest,
        };

        context.Result = new ObjectResult(new { message = exception.Message, code = exception.Code })
        {
            StatusCode = statusCode,
        };
        context.ExceptionHandled = true;
    }
}

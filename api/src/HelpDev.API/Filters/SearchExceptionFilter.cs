using HelpDev.Modules.Search.Application.Reindex;
using HelpDev.Modules.Search.Application.Search;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HelpDev.API.Filters;

public sealed class SearchExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is SearchReindexException reindexException)
        {
            context.Result = new ObjectResult(new { message = reindexException.Message, code = reindexException.Code })
            {
                StatusCode = reindexException.Code switch
                {
                    SearchReindexErrorCodes.AlreadyRunning => StatusCodes.Status409Conflict,
                    _ => StatusCodes.Status400BadRequest,
                },
            };
            context.ExceptionHandled = true;
            return;
        }

        if (context.Exception is not SearchException exception)
        {
            return;
        }

        context.Result = new ObjectResult(new { message = exception.Message, code = exception.Code })
        {
            StatusCode = StatusCodes.Status400BadRequest,
        };
        context.ExceptionHandled = true;
    }
}

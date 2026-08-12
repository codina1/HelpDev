using HelpDev.Infrastructure.Outbox.Operations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HelpDev.API.Filters;

public sealed class OutboxOperationsExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not OutboxOperationsException exception)
        {
            return;
        }

        var statusCode = exception.Code switch
        {
            OutboxOperationsErrorCodes.MessageNotFound => StatusCodes.Status404NotFound,
            OutboxOperationsErrorCodes.MessageAlreadyProcessed => StatusCodes.Status409Conflict,
            OutboxOperationsErrorCodes.MessageCurrentlyProcessing => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest,
        };

        context.Result = new ObjectResult(new { message = exception.Message, code = exception.Code })
        {
            StatusCode = statusCode,
        };
        context.ExceptionHandled = true;
    }
}

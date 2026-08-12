using HelpDev.Modules.Auditing.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HelpDev.API.Filters;

public sealed class AuditExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not AuditException ex)
        {
            return;
        }

        var statusCode = ex.Code switch
        {
            AuditErrorCodes.RecordNotFound => StatusCodes.Status404NotFound,
            AuditErrorCodes.PageInvalid or AuditErrorCodes.DateRangeInvalid or AuditErrorCodes.DateRangeTooLarge or
            AuditErrorCodes.CategoryInvalid or AuditErrorCodes.ActionUnsupported or AuditErrorCodes.OutcomeInvalid =>
                StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status400BadRequest,
        };

        context.Result = new ObjectResult(new { message = ex.Message, code = ex.Code })
        {
            StatusCode = statusCode,
        };
        context.ExceptionHandled = true;
    }
}

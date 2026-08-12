using HelpDev.Modules.Administration.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HelpDev.API.Filters;

public sealed class AdministrationExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not AdministrationException exception)
        {
            return;
        }

        var statusCode = exception.Code switch
        {
            AdministrationApplicationErrorCodes.FeatureNotFound
                or AdministrationApplicationErrorCodes.SettingNotFound
                or AdministrationApplicationErrorCodes.AnnouncementNotFound => StatusCodes.Status404NotFound,

            AdministrationApplicationErrorCodes.FeatureKeyDuplicate
                or AdministrationApplicationErrorCodes.SettingKeyDuplicate
                or AdministrationApplicationErrorCodes.AnnouncementCannotDeletePublished
                or AdministrationApplicationErrorCodes.AnnouncementStatusInvalid => StatusCodes.Status409Conflict,

            AdministrationApplicationErrorCodes.DashboardUnavailable => StatusCodes.Status503ServiceUnavailable,

            _ => StatusCodes.Status400BadRequest,
        };

        context.Result = new ObjectResult(new { message = exception.Message, code = exception.Code })
        {
            StatusCode = statusCode,
        };
        context.ExceptionHandled = true;
    }
}

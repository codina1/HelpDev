using HelpDev.Modules.Analytics.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HelpDev.API.Filters;

public sealed class AnalyticsExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not AnalyticsException exception)
        {
            return;
        }

        var statusCode = exception.Code switch
        {
            AnalyticsApplicationErrorCodes.DateRangeInvalid
                or AnalyticsApplicationErrorCodes.DateRangeTooLarge
                or AnalyticsApplicationErrorCodes.MetricKeyInvalid
                or AnalyticsApplicationErrorCodes.DimensionInvalid
                or AnalyticsApplicationErrorCodes.LimitInvalid
                or AnalyticsApplicationErrorCodes.EventDimensionsInvalid
                or AnalyticsApplicationErrorCodes.EventDimensionNotAllowed
                or AnalyticsApplicationErrorCodes.EventQuantityInvalid
                or AnalyticsApplicationErrorCodes.EventTypeUnsupported
                or AnalyticsApplicationErrorCodes.EventSchemaVersionUnsupported
                or AnalyticsApplicationErrorCodes.EventTimestampInvalid => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status400BadRequest,
        };

        context.Result = new ObjectResult(new { message = exception.Message, code = exception.Code })
        {
            StatusCode = statusCode,
        };
        context.ExceptionHandled = true;
    }
}

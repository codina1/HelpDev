using HelpDev.API.Contracts;
using HelpDev.Modules.Learning.Application.Personalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HelpDev.API.Filters;

public sealed class LearningPersonalizationExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not LearningPersonalizationException exception)
        {
            return;
        }

        var statusCode = exception.Code switch
        {
            LearningPersonalizationErrorCodes.RoadmapNotFound => StatusCodes.Status404NotFound,
            LearningPersonalizationErrorCodes.InvalidExperienceLevel => StatusCodes.Status400BadRequest,
            LearningPersonalizationErrorCodes.InvalidUser => StatusCodes.Status400BadRequest,
            LearningPersonalizationErrorCodes.ProfileRequired => StatusCodes.Status400BadRequest,
            LearningPersonalizationErrorCodes.ProviderFailed => StatusCodes.Status502BadGateway,
            _ => StatusCodes.Status400BadRequest,
        };

        context.Result = new ObjectResult(new ApiErrorResponse
            {
                Message = exception.Message,
                Code = exception.Code,
            })
        {
            StatusCode = statusCode,
        };
        context.ExceptionHandled = true;
    }
}

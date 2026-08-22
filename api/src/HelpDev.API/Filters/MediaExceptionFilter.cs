using HelpDev.Modules.Media.Application.Assets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HelpDev.API.Filters;

public sealed class MediaExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not MediaException exception)
        {
            return;
        }

        var statusCode = exception.Code switch
        {
            MediaErrorCodes.NotFound => StatusCodes.Status404NotFound,
            MediaErrorCodes.Forbidden => StatusCodes.Status403Forbidden,
            MediaErrorCodes.PayloadTooLarge => StatusCodes.Status413PayloadTooLarge,
            MediaErrorCodes.UnsupportedType => StatusCodes.Status415UnsupportedMediaType,
            MediaErrorCodes.StorageFailed => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status400BadRequest,
        };

        context.Result = new ObjectResult(new { message = exception.Message, code = exception.Code })
        {
            StatusCode = statusCode,
        };
        context.ExceptionHandled = true;
    }
}

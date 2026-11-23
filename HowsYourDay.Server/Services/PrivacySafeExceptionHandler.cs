using Microsoft.AspNetCore.Diagnostics;

namespace HowsYourDay.Server.Services;

public class PrivacySafeExceptionHandler(ILogger<PrivacySafeExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var status = exception is BadHttpRequestException badRequest
            ? badRequest.StatusCode
            : StatusCodes.Status500InternalServerError;

        if (status >= StatusCodes.Status500InternalServerError)
            logger.LogError("Unhandled request failure. Trace ID {TraceId}", context.TraceIdentifier);

        await Results.Problem(statusCode: status).ExecuteAsync(context);
        return true;
    }
}

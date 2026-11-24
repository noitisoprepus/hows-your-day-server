using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Components.Endpoints;
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
        {
            logger.LogError("Unhandled request failure. Trace ID {TraceId}", context.TraceIdentifier);

            if (IsBrowserPage(context))
            {
                await ErrorPage(context, status).ExecuteAsync(context);
                return true;
            }
        }

        await Results.Problem(statusCode: status).ExecuteAsync(context);
        return true;
    }

    private static bool IsBrowserPage(HttpContext context)
    {
        // ExceptionHandlerMiddleware clears the active endpoint before calling handlers.
        var endpoint = context.Features.Get<IExceptionHandlerFeature>()?.Endpoint;
        return endpoint?.Metadata.GetMetadata<ComponentTypeMetadata>() is not null ||
               context.Request.Path.StartsWithSegments("/ui/account");
    }

    private static IResult ErrorPage(HttpContext context, int status)
    {
        var traceId = HtmlEncoder.Default.Encode(context.TraceIdentifier);
        var home = HtmlEncoder.Default.Encode(context.Request.PathBase + "/");

        return Results.Content($"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="utf-8">
                <meta name="viewport" content="width=device-width, initial-scale=1">
                <meta name="color-scheme" content="light dark">
                <title>Something went wrong · HowsYourDay</title>
                <script src="{home}theme.js"></script>
                <link rel="stylesheet" href="{home}app.css">
            </head>
            <body>
                <main class="page">
                    <section class="journal-card">
                        <h1>Something went wrong</h1>
                        <p>We couldn't finish your request. You can go back to Today and try again.</p>
                        <p>Reference ID: <code>{traceId}</code></p>
                        <p><a href="{home}">Back to Today</a></p>
                    </section>
                </main>
            </body>
            </html>
            """, "text/html; charset=utf-8", statusCode: status);
    }
}

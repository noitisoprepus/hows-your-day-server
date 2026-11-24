using System.Text;
using System.Text.Json;
using HowsYourDay.Server.Components.Pages;
using HowsYourDay.Server.Services;
using Microsoft.AspNetCore.Components.Endpoints;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace HowsYourDay.Server.Tests;

public class PrivacySafeExceptionHandlerTests
{
    [Theory]
    [InlineData("/", true)]
    [InlineData("/ui/account/login", false)]
    public async Task BrowserFailure_ReturnsSafeHtmlWithEncodedReference(string path, bool isRazorPage)
    {
        using var services = CreateServices();
        using var body = new MemoryStream();
        var context = CreateContext(services, body, path);
        if (isRazorPage)
        {
            context.Features.Set<IExceptionHandlerFeature>(new ExceptionHandlerFeature
            {
                Endpoint = new Endpoint(null, new EndpointMetadataCollection(new ComponentTypeMetadata(typeof(Today))), "Today")
            });
        }

        var handler = new PrivacySafeExceptionHandler(NullLogger<PrivacySafeExceptionHandler>.Instance);
        var handled = await handler.TryHandleAsync(context, new InvalidOperationException("Private journal text"), default);
        var html = Encoding.UTF8.GetString(body.ToArray());

        Assert.True(handled);
        Assert.Equal(500, context.Response.StatusCode);
        Assert.StartsWith("text/html", context.Response.ContentType);
        Assert.Contains("Something went wrong", html);
        Assert.Contains("""<script src="/theme.js"></script>""", html);
        Assert.Contains("request-&lt;id&gt;", html);
        Assert.Contains("Back to Today", html);
        Assert.DoesNotContain("Private journal text", html);
    }

    [Theory]
    [InlineData("/api/day/me/entries", 500)]
    [InlineData("/ui/account/login", 413)]
    public async Task ApiFailuresAndClientErrors_KeepProblemDetails(string path, int status)
    {
        using var services = CreateServices();
        using var body = new MemoryStream();
        var context = CreateContext(services, body, path);
        Exception exception = status == 500
            ? new InvalidOperationException("Private journal text")
            : new BadHttpRequestException("Private journal text", status);

        var handler = new PrivacySafeExceptionHandler(NullLogger<PrivacySafeExceptionHandler>.Instance);
        var handled = await handler.TryHandleAsync(context, exception, default);
        using var problem = JsonDocument.Parse(body.ToArray());

        Assert.True(handled);
        Assert.Equal(status, context.Response.StatusCode);
        Assert.StartsWith("application/problem+json", context.Response.ContentType);
        Assert.Equal(status, problem.RootElement.GetProperty("status").GetInt32());
        Assert.DoesNotContain("Private journal text", problem.RootElement.GetRawText());
    }

    private static ServiceProvider CreateServices() => new ServiceCollection()
        .AddLogging()
        .AddProblemDetails()
        .BuildServiceProvider();

    private static DefaultHttpContext CreateContext(IServiceProvider services, Stream body, string path)
    {
        var context = new DefaultHttpContext { RequestServices = services, TraceIdentifier = "request-<id>" };
        context.Request.Path = path;
        context.Response.Body = body;
        return context;
    }
}

using HowsYourDay.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace HowsYourDay.Server.Extensions;

public static class AccountFormEndpoints
{
    public static void MapAccountFormEndpoints(this WebApplication app)
    {
        var account = app.MapGroup("/ui/account").ExcludeFromDescription();

        account.MapPost("/login", LoginAsync).AllowAnonymous().RequireRateLimiting("Auth");
        account.MapPost("/register", RegisterAsync).AllowAnonymous().RequireRateLimiting("Auth");
        account.MapPost("/logout", LogoutAsync).RequireAuthorization("Ui");
        account.MapPost("/delete", DeleteAsync).RequireAuthorization("Ui");
    }

    // Form binding requires antiforgery validation, including for sign-out.
    private static async Task<IResult> LoginAsync([FromForm] IFormCollection form, IAccountService accounts)
    {
        var userName = form["userName"].ToString();
        var password = form["password"].ToString();

        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            return Results.LocalRedirect("/login?error=required");

        var result = await accounts.LoginUiAsync(userName, password);
        return Results.LocalRedirect(result.Succeeded ? "/" : "/login?error=credentials");
    }

    private static async Task<IResult> RegisterAsync([FromForm] IFormCollection form, IAccountService accounts)
    {
        var userName = form["userName"].ToString();
        var password = form["password"].ToString();

        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            return Results.LocalRedirect("/register?error=required");

        var result = await accounts.RegisterAsync(userName, password);
        if (result.Succeeded)
            return Results.LocalRedirect("/login?registered=true");

        var error = result.Errors.Any(error => error.Code.StartsWith("Password", StringComparison.Ordinal))
            ? "password"
            : "username";
        return Results.LocalRedirect($"/register?error={error}");
    }

    private static async Task<IResult> LogoutAsync([FromForm] IFormCollection form, IAccountService accounts)
    {
        await accounts.LogoutUiAsync();
        return Results.LocalRedirect("/login");
    }

    private static async Task<IResult> DeleteAsync(
        [FromForm] IFormCollection form, HttpContext context, IAccountService accounts)
    {
        if (form["confirm"] != "yes")
            return Results.LocalRedirect("/account/delete?error=confirmation");

        var user = await accounts.GetUserAsync(context.User.GetUserId());
        if (user is null || !(await accounts.DeleteUserAsync(user)).Succeeded)
            return Results.LocalRedirect("/account/delete?error=delete");

        await accounts.LogoutUiAsync();
        return Results.LocalRedirect("/login");
    }
}

using Fuzn.FluentHttp.TestApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace Fuzn.FluentHttp.TestApi.Endpoints;

public static class CookiesEndpoints
{
    public static void MapCookiesEndpoints(this WebApplication app)
    {
        app.MapGet("/api/cookies/set", (HttpContext context, [FromQuery] string name, [FromQuery] string value) =>
        {
            context.Response.Cookies.Append(name, value, new CookieOptions
            {
                Path = "/",
                HttpOnly = true,
                MaxAge = TimeSpan.FromHours(1)
            });
            return Results.Ok(new { cookieSet = true, name, value });
        });

        app.MapGet("/api/cookies/set-multiple", (HttpContext context) =>
        {
            context.Response.Cookies.Append("cookie1", "value1", new CookieOptions { Path = "/" });
            context.Response.Cookies.Append("cookie2", "value2", new CookieOptions { Path = "/" });
            return Results.Ok(new { cookiesSet = true });
        });

        app.MapGet("/api/cookies/echo", (HttpContext context) =>
        {
            var cookies = context.Request.Cookies.ToDictionary(c => c.Key, c => c.Value);
            return Results.Ok(new CookiesEchoResponse { Cookies = cookies });
        });
    }
}

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace backenddemo.ApiService.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        Console.WriteLine($"[Request] {context.Request.Method} {context.Request.Path}");
        await _next(context);
    }
}

public class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestTimingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        await _next(context);
        stopwatch.Stop();
        Console.WriteLine($"[Timing] {context.Request.Method} {context.Request.Path} completed in {stopwatch.ElapsedMilliseconds} ms");
    }
}

public class HeaderValidationMiddleware
{
    private readonly RequestDelegate _next;

    public HeaderValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path;

        if (path == "/" || path == "/hello" || path == "/login" || path.StartsWithSegments("/swagger"))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.ContainsKey("x-api-key"))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { error = "API Key Missing" });
            return;
        }

        await _next(context);
    }
}

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["Referrer-Policy"] = "no-referrer";
        context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=()";
        context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";

        await _next(context);
    }
}

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder) => builder.UseMiddleware<RequestLoggingMiddleware>();
    public static IApplicationBuilder UseRequestTiming(this IApplicationBuilder builder) => builder.UseMiddleware<RequestTimingMiddleware>();
    public static IApplicationBuilder UseHeaderValidation(this IApplicationBuilder builder) => builder.UseMiddleware<HeaderValidationMiddleware>();
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder) => builder.UseMiddleware<SecurityHeadersMiddleware>();
}

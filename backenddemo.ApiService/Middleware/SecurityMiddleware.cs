using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace backenddemo.ApiService.Middleware;

// ── Logging Middleware ───────────────────────────────────────────────────────
public class RequestLoggingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] {context.Request.Method} {context.Request.Path}");
        await next(context);
        Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] {context.Request.Method} {context.Request.Path} → {context.Response.StatusCode}");
    }
}

// ── Timing Middleware ────────────────────────────────────────────────────────
public class RequestTimingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        await next(context);
        sw.Stop();
        Console.WriteLine($"[Timing] {context.Request.Method} {context.Request.Path} completed in {sw.ElapsedMilliseconds} ms");
    }
}

// ── Header Validation Middleware ─────────────────────────────────────────────
// Validates that POST/PUT/PATCH requests include Content-Type: application/json
public class HeaderValidationMiddleware(RequestDelegate next)
{
    private static readonly string[] _writeMethods = ["POST", "PUT", "PATCH"];

    public async Task InvokeAsync(HttpContext context)
    {
        var method = context.Request.Method.ToUpperInvariant();

        if (_writeMethods.Contains(method)
            && context.Request.ContentLength > 0
            && !context.Request.Path.StartsWithSegments("/swagger"))
        {
            var contentType = context.Request.ContentType ?? string.Empty;
            if (!contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Content-Type must be application/json for write operations.",
                    status = 415
                });
                return;
            }
        }

        await next(context);
    }
}

// ── Security Headers Middleware ──────────────────────────────────────────────
public class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["Referrer-Policy"] = "no-referrer";
        context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=()";
        await next(context);
    }
}

// ── Extension Methods ────────────────────────────────────────────────────────
public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder b) =>
        b.UseMiddleware<RequestLoggingMiddleware>();

    public static IApplicationBuilder UseRequestTiming(this IApplicationBuilder b) =>
        b.UseMiddleware<RequestTimingMiddleware>();

    public static IApplicationBuilder UseHeaderValidation(this IApplicationBuilder b) =>
        b.UseMiddleware<HeaderValidationMiddleware>();

    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder b) =>
        b.UseMiddleware<SecurityHeadersMiddleware>();
}

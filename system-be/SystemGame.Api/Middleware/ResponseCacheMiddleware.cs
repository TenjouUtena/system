using Microsoft.Extensions.Caching.Memory;

namespace SystemGame.Api.Middleware;

/// <summary>
/// Simple in-memory response caching middleware for GET requests
/// </summary>
public class ResponseCacheMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ResponseCacheMiddleware> _logger;

    public ResponseCacheMiddleware(
        RequestDelegate next,
        IMemoryCache cache,
        ILogger<ResponseCacheMiddleware> logger)
    {
        _next = next;
        _cache = cache;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only cache GET requests
        if (context.Request.Method != HttpMethods.Get)
        {
            await _next(context);
            return;
        }

        // Skip caching for authenticated endpoints (unless explicitly allowed)
        if (context.Request.Path.StartsWithSegments("/api/auth") ||
            context.Request.Path.StartsWithSegments("/api/combat") ||
            context.Request.Path.StartsWithSegments("/api/health"))
        {
            await _next(context);
            return;
        }

        var cacheKey = $"response_{context.Request.Path}{context.Request.QueryString}";

        // Try to get from cache
        if (_cache.TryGetValue(cacheKey, out byte[]? cachedResponse))
        {
            context.Response.Headers.Append("X-Cache", "HIT");
            context.Response.ContentType = "application/json";
            await context.Response.Body.WriteAsync(cachedResponse);
            return;
        }

        // Capture response
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context);

        // Cache successful responses
        if (context.Response.StatusCode == 200)
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var response = await new StreamReader(context.Response.Body).ReadToEndAsync();
            var responseBytes = System.Text.Encoding.UTF8.GetBytes(response);

            // Cache for 30 seconds
            _cache.Set(cacheKey, responseBytes, TimeSpan.FromSeconds(30));

            context.Response.Headers.Append("X-Cache", "MISS");
            context.Response.Body.Seek(0, SeekOrigin.Begin);
        }

        await responseBody.CopyToAsync(originalBodyStream);
    }
}

public static class ResponseCacheMiddlewareExtensions
{
    public static IApplicationBuilder UseResponseCache(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ResponseCacheMiddleware>();
    }
}

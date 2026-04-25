using System.Diagnostics;
using System.Text.Json;

namespace Ecommerce.API.Middleware;

public sealed class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

            logger.LogError(ex, "Unhandled exception. TraceId: {TraceId}", traceId);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";

            var response = new
            {
                type = "https://httpstatuses.com/500",
                title = "Internal Server Error",
                status = StatusCodes.Status500InternalServerError,
                detail = environment.IsDevelopment() ? ex.Message : "Ocurrió un error inesperado.",
                traceId
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}

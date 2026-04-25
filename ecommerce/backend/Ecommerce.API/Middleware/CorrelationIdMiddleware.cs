using System.ComponentModel.DataAnnotations;
using System.Net;

namespace Ecommerce.API.Middleware;

public sealed class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, type) = exception switch
        {
            ValidationException => (HttpStatusCode.BadRequest, "Request inválida", "validation_error"),
            UnauthorizedAccessException => (HttpStatusCode.Forbidden, "No autorizado", "authorization_error"),
            KeyNotFoundException => (HttpStatusCode.NotFound, "Recurso no encontrado", "not_found"),
            _ => (HttpStatusCode.InternalServerError, "Error interno del servidor", "server_error")
        };

        logger.LogError(
            exception,
            "Unhandled exception for {Method} {Path} | CorrelationId={CorrelationId}",
            context.Request.Method,
            context.Request.Path,
            context.TraceIdentifier);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var payload = new
        {
            type,
            title,
            status = (int)statusCode,
            detail = appDetail(exception, statusCode),
            traceId = context.TraceIdentifier
        };

        await context.Response.WriteAsJsonAsync(payload);
    }

    private static string appDetail(Exception exception, HttpStatusCode statusCode)
    {
        if (statusCode != HttpStatusCode.InternalServerError)
        {
            return exception.Message;
        }

        return "Ocurrió un error inesperado. Intentalo nuevamente más tarde.";
    }
}

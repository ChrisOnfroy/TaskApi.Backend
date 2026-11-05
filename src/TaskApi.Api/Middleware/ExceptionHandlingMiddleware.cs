using Microsoft.AspNetCore.Mvc;
using TaskApi.Application.Exceptions;
using TaskApi.Domain.Exceptions;

namespace TaskApi.Api.Middleware;

/// <summary>
/// Middleware global de manejo de excepciones.
/// Intercepta cualquier excepción no controlada y la traduce a una respuesta estándar
/// RFC 7807 (ProblemDetails / ValidationProblemDetails) con el código HTTP apropiado.
/// </summary>
public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
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
        var (statusCode, problemDetails) = exception switch
        {
            RequestValidationException validationEx => (
                StatusCodes.Status400BadRequest,
                (ProblemDetails)new ValidationProblemDetails(
                    validationEx.Errors.ToDictionary(k => k.Key, v => v.Value))
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Error de validación",
                    Detail = validationEx.Message,
                    Instance = context.Request.Path
                }),

            DomainException domainEx => (
                StatusCodes.Status422UnprocessableEntity,
                new ProblemDetails
                {
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Title = "Regla de negocio incumplida",
                    Detail = domainEx.Message,
                    Instance = context.Request.Path
                }),

            NotFoundException notFoundEx => (
                StatusCodes.Status404NotFound,
                new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Recurso no encontrado",
                    Detail = notFoundEx.Message,
                    Instance = context.Request.Path
                }),

            _ => (
                StatusCodes.Status500InternalServerError,
                new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Error interno del servidor",
                    Detail = "Ha ocurrido un error inesperado al procesar la solicitud.",
                    Instance = context.Request.Path
                })
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Excepción no controlada en {Method} {Path}",
                context.Request.Method, context.Request.Path);
        }
        else
        {
            logger.LogWarning("Respuesta {StatusCode} en {Method} {Path}: {Message}",
                statusCode, context.Request.Method, context.Request.Path, exception.Message);
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(
            problemDetails,
            problemDetails.GetType(),
            cancellationToken: context.RequestAborted);
    }
}
